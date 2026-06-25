using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using k8s;
using k8s.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(provider =>
{
    try
    {
        return new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile());
    }
    catch
    {
        return new Kubernetes(KubernetesClientConfiguration.InClusterConfig());
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

const string InstanceLabelKey = "app";
const string InstanceLabelValue = "game-instance-api";
const string ManagedByLabelKey = "managed-by";
const string ManagedByLabelValue = "game-manager-api";
const string InstanceNameLabelKey = "instance-name";
const string DefaultImage = "game-instance-api-image:latest";
const int ContainerPort = 8080;
const int ServicePort = 80;

var PortForwards = new Dictionary<string, PortForwardInfo>();

app.MapPost("/instances", async (CreateInstanceRequest request, Kubernetes kubernetes) =>
{
    var instanceName = string.IsNullOrWhiteSpace(request?.Name)
        ? $"game-instance-{Guid.NewGuid():N}".Substring(0, 20)
        : request.Name.Trim().ToLowerInvariant();

    if (!IsValidKubernetesName(instanceName))
    {
        return Results.BadRequest(new { error = "Instance name must use lowercase letters, digits, and '-', and must start/end with a letter or digit." });
    }

    var image = string.IsNullOrWhiteSpace(request?.Image) ? DefaultImage : request.Image.Trim();
    var namespaceName = string.IsNullOrWhiteSpace(request?.Namespace) ? "default" : request.Namespace.Trim();
    var serviceName = $"{instanceName}-svc";
    var deploymentName = $"{instanceName}-deploy";

    var labels = new Dictionary<string, string>
    {
        [InstanceLabelKey] = InstanceLabelValue,
        [ManagedByLabelKey] = ManagedByLabelValue,
        [InstanceNameLabelKey] = instanceName
    };

    try
    {
        await EnsureKindImageLoadedAsync(image, kubernetes);

        var deployment = new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = new V1ObjectMeta
            {
                Name = deploymentName,
                NamespaceProperty = namespaceName,
                Labels = labels
            },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector
                {
                    MatchLabels = labels
                },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = labels
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>
                        {
                            new()
                            {
                                Name = "game-instance",
                                Image = image,
                                ImagePullPolicy = "IfNotPresent",
                                Ports = new List<V1ContainerPort>
                                {
                                    new() { ContainerPort = ContainerPort }
                                }
                            }
                        }
                    }
                }
            }
        };

        await kubernetes.CreateNamespacedDeploymentAsync(deployment, namespaceName);

        var service = new V1Service
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = new V1ObjectMeta
            {
                Name = serviceName,
                NamespaceProperty = namespaceName,
                Labels = labels
            },
            Spec = new V1ServiceSpec
            {
                Type = "NodePort",
                Selector = labels,
                Ports = new List<V1ServicePort>
                {
                    new()
                    {
                        Port = ServicePort,
                        TargetPort = ContainerPort,
                        Protocol = "TCP"
                    }
                }
            }
        };

        var createdService = await kubernetes.CreateNamespacedServiceAsync(service, namespaceName);

        await WaitForPodReadyAsync(kubernetes, labels, namespaceName, TimeSpan.FromSeconds(60));

        var portForward = await StartPortForwardAsync(serviceName, namespaceName, ServicePort);
        PortForwards[instanceName] = portForward;

        return Results.Created($"/instances/{instanceName}", new InstanceResponse(instanceName, image, portForward.Port, $"http://localhost:{portForward.Port}/weatherforecast", namespaceName, serviceName, "Created"));
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.MapGet("/instances", async (Kubernetes kubernetes, string? @namespace) =>
{
    var namespaceName = string.IsNullOrWhiteSpace(@namespace) ? "default" : @namespace.Trim();
    var selector = $"{ManagedByLabelKey}={ManagedByLabelValue}";
    var services = await kubernetes.ListNamespacedServiceAsync(namespaceName, labelSelector: selector);

    var instances = services.Items.Select(service =>
    {
        var labels = service.Metadata?.Labels;
        var instanceName = labels is { } && labels.TryGetValue(InstanceNameLabelKey, out var name)
            ? name
            : service.Metadata?.Name?.Replace("-svc", "") ?? string.Empty;

        var localPort = PortForwards.TryGetValue(instanceName, out var info) ? info.Port : 0;
        return new InstanceResponse(
            instanceName,
            labels is { } && labels.TryGetValue(InstanceLabelKey, out var imageTag) ? imageTag : null,
            localPort,
            localPort > 0 ? $"http://localhost:{localPort}/weatherforecast" : null,
            namespaceName,
            service.Metadata?.Name ?? string.Empty,
            localPort > 0 ? "Available" : "Pending");
    });

    return Results.Ok(instances);
});

app.MapDelete("/instances/{name}", async (string name, Kubernetes kubernetes, string? @namespace) =>
{
    var namespaceName = string.IsNullOrWhiteSpace(@namespace) ? "default" : @namespace.Trim();
    var deploymentName = $"{name}-deploy";
    var serviceName = $"{name}-svc";

    try
    {
        StopPortForward(name);
        await kubernetes.DeleteNamespacedServiceAsync(serviceName, namespaceName);
        await kubernetes.DeleteNamespacedDeploymentAsync(deploymentName, namespaceName);
        return Results.Ok(new { message = $"Instance '{name}' deleted." });
    }
    catch (k8s.Autorest.HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return Results.NotFound(new { error = $"Instance '{name}' not found in namespace '{namespaceName}'." });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

app.Run();

static bool IsValidKubernetesName(string name)
{
    return Regex.IsMatch(name, "^[a-z0-9]([-a-z0-9]*[a-z0-9])?$");
}

async Task<PortForwardInfo> StartPortForwardAsync(string serviceName, string namespaceName, int targetPort)
{
    var localPort = GetFreeTcpPort();
    var args = $"port-forward svc/{serviceName} {localPort}:{targetPort} -n {namespaceName}";

    var startInfo = new ProcessStartInfo("kubectl", args)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    var process = Process.Start(startInfo);
    if (process == null)
    {
        throw new InvalidOperationException("Failed to start kubectl port-forward.");
    }

    await Task.Delay(1000);
    if (process.HasExited)
    {
        var error = await process.StandardError.ReadToEndAsync();
        process.Dispose();
        throw new InvalidOperationException($"kubectl port-forward failed to start: {error}");
    }

    return new PortForwardInfo(serviceName, namespaceName, localPort, process);
}

async Task WaitForPodReadyAsync(Kubernetes kubernetes, Dictionary<string, string> labels, string namespaceName, TimeSpan timeout)
{
    var selector = string.Join(",", labels.Select(kv => $"{kv.Key}={kv.Value}"));
    var deadline = DateTime.UtcNow + timeout;

    while (DateTime.UtcNow < deadline)
    {
        var pods = await kubernetes.ListNamespacedPodAsync(namespaceName, labelSelector: selector);
        var pod = pods.Items.FirstOrDefault();
        if (pod is not null)
        {
            var phase = pod.Status?.Phase;
            var containerReady = pod.Status?.ContainerStatuses?.All(cs => cs.Ready) == true;
            if (phase == "Running" && containerReady)
            {
                return;
            }

            if (phase == "Failed" || phase == "Unknown")
            {
                throw new InvalidOperationException($"Pod is not ready. Current phase={phase}. Pod={pod.Metadata?.Name}.");
            }
        }

        await Task.Delay(2000);
    }

    var currentPods = await kubernetes.ListNamespacedPodAsync(namespaceName, labelSelector: selector);
    var podStatus = currentPods.Items.Select(p => $"{p.Metadata?.Name}:{p.Status?.Phase}");
    throw new InvalidOperationException($"Timed out waiting for pod to be running. Pod statuses: {string.Join(",", podStatus)}");
}

void StopPortForward(string instanceName)
{
    if (!PortForwards.TryGetValue(instanceName, out var info))
    {
        return;
    }

    try
    {
        if (!info.Process.HasExited)
        {
            info.Process.Kill(true);
        }
    }
    catch
    {
    }

    info.Process.Dispose();
    PortForwards.Remove(instanceName);
}

int GetFreeTcpPort()
{
    using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return port;
}

async Task EnsureKindImageLoadedAsync(string image, Kubernetes kubernetes)
{
    if (!await IsKindClusterAsync(kubernetes))
    {
        return;
    }

    if (!LocalDockerImageExists(image))
    {
        throw new InvalidOperationException($"Local image '{image}' was not found. Build it locally and load it into kind before deploying.");
    }

    if (!KindLoadDockerImage(image, out var output, out var error))
    {
        throw new InvalidOperationException($"Failed to load image into kind cluster. Output:\n{output}\nError:\n{error}");
    }
}

async Task<bool> IsKindClusterAsync(Kubernetes kubernetes)
{
    try
    {
        var nodes = await kubernetes.ListNodeAsync();
        return nodes.Items.Any(node => node.Metadata?.Name?.StartsWith("kind-") == true);
    }
    catch
    {
        return false;
    }
}

bool LocalDockerImageExists(string image)
{
    try
    {
        var startInfo = new ProcessStartInfo("docker", $"image inspect {image}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process.WaitForExit();
        return process.ExitCode == 0;
    }
    catch
    {
        return false;
    }
}

bool KindLoadDockerImage(string image, out string output, out string error)
{
    output = string.Empty;
    error = string.Empty;

    try
    {
        var startInfo = new ProcessStartInfo("kind", $"load docker-image {image}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        output = process.StandardOutput.ReadToEnd();
        error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}

record PortForwardInfo(string ServiceName, string Namespace, int Port, Process Process);
record CreateInstanceRequest(string? Name, string? Image, string? Namespace);
record InstanceResponse(string Name, string? Image, int LocalPort, string? Url, string Namespace, string ServiceName, string Status);
