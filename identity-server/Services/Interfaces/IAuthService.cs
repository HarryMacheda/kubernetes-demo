using Authentication;

public interface IAuthService
{
    Task<(string accessToken, RefreshToken refreshToken)?> Login(
        string username,
        string password,
        string clientId
        );

    Task<AuthResponse> Register(string email, string password, string firstName, string surname, string clientId);
}