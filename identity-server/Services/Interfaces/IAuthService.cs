public interface IAuthService
{
    Task<(string accessToken, RefreshToken refreshToken)?> Login(
        string username,
        string password,
        string clientId
        );
}