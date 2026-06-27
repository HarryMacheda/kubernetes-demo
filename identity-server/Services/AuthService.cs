public class AuthService : IAuthService
{
    private readonly ITokenService _tokens;
    private readonly IUserService _users;

    public AuthService(ITokenService tokens, IUserService users)
    {
        _tokens = tokens;
        _users = users;
    }

    public async Task<(string accessToken, RefreshToken refreshToken)?> Login(
        string username,
        string password,
        string clientId
        )
    {
        var client = ClientStore.Clients
            .FirstOrDefault(c => c.ClientId == clientId);

        if (client == null )
            return null;

        var user = await _users.GetByEmail(username);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var access = _tokens.CreateAccessToken(user, clientId);
        var refresh = _tokens.CreateRefreshToken(user, clientId);

        return (access, refresh);
    }
}