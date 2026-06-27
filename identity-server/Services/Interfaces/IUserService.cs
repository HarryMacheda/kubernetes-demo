using Authentication;

public interface IUserService
{
    Task<User?> GetByEmail(string email);
    Task<User?> GetById(Guid id);

    Task<User?> ValidateUser(string email, string password);
}