using Authentication;

public interface IAuthService
{
    Task<User?> Login(
        string username,
        string password,
        string clientId
        );

    Task<User> Register(string email, string password, string firstName, string surname, string clientId);
}