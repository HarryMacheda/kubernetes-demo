using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.Login(
            request.Username,
            request.Password,
            request.ClientId
            );

        if (result == null)
            return Unauthorized();

        return Ok(new
        {
            access_token = result.Value.accessToken,
            refresh_token = result.Value.refreshToken.Token
        });
    }
}

public record LoginRequest(
    string Username,
    string Password,
    string ClientId);