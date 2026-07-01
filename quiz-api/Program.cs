using OpenIddict.Validation.AspNetCore;
using OpenIddict.Validation.SystemNetHttp;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is not configured");
builder.Services.AddScoped<IQuizRepository>(_ => new PostgresQuizRepository(connectionString));
builder.Services.AddScoped<IQuestionRepository>(_ => new PostgresQuestionRepository(connectionString));

builder.Services.AddOpenApi();

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("http://identity-server");
        options.AddAudiences("api");

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(
    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
    
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
