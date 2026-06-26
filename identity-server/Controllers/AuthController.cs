using Authentication;
using identity_server.Models;
using identity_server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace identity_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IExternalLoginService _externalLoginService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserRepository userRepository,
            ITokenService tokenService,
            IExternalLoginService externalLoginService,
            ILogger<AuthController> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _externalLoginService = externalLoginService;
            _logger = logger;
        }

        /// <summary>
        /// Sign in with email and password to receive a JWT token.
        /// </summary>
        [HttpPost("signin")]
        public async Task<ActionResult<LoginResponse>> SignIn([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            try
            {
                var user = await _userRepository.VerifyUserPasswordAsync(request.Email, request.Password);

                if (user == null)
                {
                    _logger.LogWarning($"Failed login attempt for email: {request.Email}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                var token = _tokenService.GenerateToken(user);

                _logger.LogInformation($"User {request.Email} successfully signed in");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Sign in successful",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        Surname = user.Surname,
                        CreatedAt = user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign in");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during sign in"
                });
            }
        }

        /// <summary>
        /// Initiates OAuth2 sign-in with the specified provider.
        /// </summary>
        [HttpGet("signin/{provider}")]
        public IActionResult SignInWithProvider(string provider)
        {
            var validProviders = new[] { "google", "microsoft" };
            if (!validProviders.Contains(provider.ToLower()))
            {
                return BadRequest(new { error = "Invalid provider" });
            }

            var redirectUrl = Url.Action("OAuth2Callback", "Auth", new { provider }, protocol: Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, provider);
        }

        /// <summary>
        /// Handles OAuth2 provider callback.
        /// </summary>
        [HttpGet("oauth2-callback/{provider}")]
        public async Task<ActionResult<LoginResponse>> OAuth2Callback(string provider)
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(provider);

                if (!result.Succeeded)
                {
                    _logger.LogWarning($"OAuth2 authentication failed for provider: {provider}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Failed to authenticate with provider"
                    });
                }

                var externalLogin = ExtractExternalLoginInfo(result, provider);
                var user = await _externalLoginService.LinkOrCreateUserAsync(externalLogin);
                var token = _tokenService.GenerateToken(user);

                _logger.LogInformation($"User {user.Email} signed in via {provider}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = $"Sign in with {provider} successful",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        Surname = user.Surname,
                        CreatedAt = user.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during OAuth2 callback for {provider}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during OAuth2 authentication"
                });
            }
        }

        private ExternalLoginInfo ExtractExternalLoginInfo(AuthenticateResult result, string provider)
        {
            var claims = result.Principal?.Claims.ToList() ?? new List<System.Security.Claims.Claim>();

            var externalLogin = new ExternalLoginInfo
            {
                Provider = provider,
                ProviderKey = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString(),
                Email = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty,
                FirstName = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value,
                LastName = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Surname)?.Value
            };

            return externalLogin;
        }
    }
}
