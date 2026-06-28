using Authentication;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokens;
    private readonly IUserService _users;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ITokenService tokens, IUserService users, ILogger<AuthService> logger)
    {
        _tokens = tokens;
        _users = users;
        _logger = logger;
    }

    public async Task<(string accessToken, RefreshToken refreshToken)?> Login(
        string username,
        string password,
        string clientId)
    {

        var client = ClientStore.Clients
            .FirstOrDefault(c => c.ClientId == clientId);

        if (client == null)
        {
            _logger.LogWarning(
                "Login failed. Client not found. ClientId: {ClientId}",
                clientId);

            return null;
        }

        var user = await _users.GetByEmail(username);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed. User not found. Username: {Username}",
                username);

            return null;
        }
        if (!user.VerifyPassword(password))
        {
            _logger.LogWarning(
                "Login failed. Invalid password. Username: {Username}, Plain Text: {PlainTextPassword}, PasswordHash: {PasswordHash}, HashedPassword: {HashedPassword}",
                username,
                password,
                user.PasswordHash,
                User.HashPassword(password));

            return null;
        }

        var access = _tokens.CreateAccessToken(user, clientId);
        var refresh = _tokens.CreateRefreshToken(user, clientId);

        return (access, refresh);
    }

    public async Task<AuthResponse> Register(
        string email,
        string password,
        string firstName,
        string surname,
        string clientId)
    {
        email = email.Trim().ToLowerInvariant();

        var existing = await _users.GetByEmail(email);
        if (existing != null)
            throw new Exception("User already exists");

        var user = await _users.CreateUser(
            email,
            password,
            firstName,
            surname);

        var accessToken = _tokens.CreateAccessToken(user, clientId);
        var refreshToken = _tokens.CreateRefreshToken(user, clientId);


        return new AuthResponse(
            accessToken,
            refreshToken.Token);
    }
}