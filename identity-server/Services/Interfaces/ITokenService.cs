using Authentication;

public interface ITokenService
{
    string CreateAccessToken(User user, string clientId);
    RefreshToken CreateRefreshToken(User user, string clientId);
}