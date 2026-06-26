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

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
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
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Password must be at least 6 characters long"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.Surname))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "First name and surname are required"
                    });
                }

                // Check if user already exists
                var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    });
                }

                // Create new user
                var newUser = await _userRepository.CreateUserAsync(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.Surname
                );

                var token = _tokenService.GenerateToken(newUser);

                _logger.LogInformation($"New user registered: {request.Email}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    Token = token,
                    User = new UserDto
                    {
                        Id = newUser.Id,
                        Email = newUser.Email,
                        FirstName = newUser.FirstName,
                        Surname = newUser.Surname,
                        CreatedAt = newUser.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }


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
