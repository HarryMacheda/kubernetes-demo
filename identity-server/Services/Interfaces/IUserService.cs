using Authentication;

public interface IUserService
{
    Task<User?> GetByEmail(string email);
    Task<User?> GetById(Guid id);

    Task<User?> ValidateUser(string email, string password);
    Task<User> CreateUser(string email, string password, string firstName, string surname);
}