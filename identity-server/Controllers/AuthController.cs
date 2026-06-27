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
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
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

    [HttpPost("register")]
public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
{
    try
    {
        var user = await _auth.Register(
            request.Username,
            request.Password,
            request.FirstName,
            request.Surname,
            "web");

        return Ok(user);
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