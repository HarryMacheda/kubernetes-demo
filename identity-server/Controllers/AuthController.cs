using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request == null)
            return BadRequest();

        var user = await _auth.Login(
            request.Username!,
            request.Password!,
            request.ClientId ?? "web");

        if (user == null)
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
        identity.SetClaim(OpenIddictConstants.Claims.Name,
            $"{user.FirstName} {user.Surname}");

        identity.SetClaim("client_id", request.ClientId ?? "web");

        var principal = new ClaimsPrincipal(identity);

        principal.SetScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OfflineAccess);

        principal.SetResources("api");

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var user = await _auth.Register(
                request.Username,
                request.Password,
                request.FirstName,
                request.Surname,
                "web");

            if (user == null)
                return BadRequest(new { error = "User creation failed" });

            // ❌ DO NOT call SignIn here
            return Ok(new
            {
                message = "User created successfully",
                userId = user.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record RegisterRequest(
    string Username,
    string Password,
    string FirstName,
    string Surname);

public record LoginRequest(
    string Username,
    string Password,
    string ClientId);