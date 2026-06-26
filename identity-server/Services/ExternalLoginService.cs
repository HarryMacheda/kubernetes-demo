using Authentication;
using identity_server.Models;

namespace identity_server.Services
{
    public interface IExternalLoginService
    {
        Task<User> LinkOrCreateUserAsync(ExternalLoginInfo externalLogin);
        Task<User?> GetUserByExternalLoginAsync(string provider, string providerKey);
    }

    public class ExternalLoginService : IExternalLoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ExternalLoginService> _logger;

        public ExternalLoginService(IUserRepository userRepository, ILogger<ExternalLoginService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> LinkOrCreateUserAsync(ExternalLoginInfo externalLogin)
        {
            if (string.IsNullOrWhiteSpace(externalLogin.Email))
                throw new ArgumentException("Email from external provider cannot be empty.");

            // Try to find existing user by email
            var existingUser = await _userRepository.GetUserByEmailAsync(externalLogin.Email);

            if (existingUser != null)
            {
                _logger.LogInformation($"Linked external login {externalLogin.Provider} to existing user: {externalLogin.Email}");
                return existingUser;
            }

            // Create new user from external login
            var firstName = externalLogin.FirstName ?? externalLogin.Email.Split('@')[0];
            var surname = externalLogin.LastName ?? "User";
            
            // Generate a random password since this is OAuth2 login
            var randomPassword = Guid.NewGuid().ToString("N")[..16];

            var newUser = await _userRepository.CreateUserAsync(
                externalLogin.Email,
                randomPassword,
                firstName,
                surname
            );

            _logger.LogInformation($"Created new user from {externalLogin.Provider} login: {externalLogin.Email}");
            return newUser;
        }

        public async Task<User?> GetUserByExternalLoginAsync(string provider, string providerKey)
        {
            // This would require storing external login info in a separate table
            // For now, we return null as this requires schema changes
            // You can extend the User table to include provider info
            return null;
        }
    }
}
