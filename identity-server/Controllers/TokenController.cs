using identity_server.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace identity_server.Controllers
{
    [ApiController]
    [Route("api/token")]
    public class TokenController : ControllerBase
    {
        public class ValidationResponse
        {
            public bool IsValid { get; set; }
            public IEnumerable<object> Claims { get; set; } = [];
            public string Error { get; set; } = string.Empty;
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ValidationResponse>> Validate([FromServices] IConfiguration configuration)
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);
            var issuer = configuration["Jwt:Issuer"];
            var validAudiences = configuration["Jwt:Audience"].Split(",");

            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized();
            }

            var token = authHeader.ToString()["Bearer ".Length..].Trim();
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudiences = validAudiences,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Ok(new
                {
                    IsValid = true,
                    Claims = principal.Claims.Select(claim => new { claim.Type, claim.Value })
                });
            }
            catch (SecurityTokenException)
            {
                return Ok(new { IsValid = false, Error = "Token validation failed" });
            }
        }
    }
}