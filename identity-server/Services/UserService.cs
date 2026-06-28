using Authentication;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public Task<User?> GetByEmail(string email)
        => _repo.GetUserByEmailAsync(email);

    public Task<User?> GetById(Guid id)
        => _repo.GetUserByIdAsync(id);

    public async Task<User?> ValidateUser(string email, string password)
    {
        var user = await _repo.GetUserByEmailAsync(email);

        if (user == null)
            return null;

        return user.VerifyPassword(password) ? user : null;
    }

    public async Task<User> CreateUser(string email, string password, string firstName, string surname)
    {
        var existing = await _repo.GetUserByEmailAsync(email);

        if (existing != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Email = email,
            FirstName = firstName,
            Surname = surname,
            CreatedAt = DateTime.UtcNow
        };

        return await _repo.CreateUserAsync(
            user.Email,
            password,
            user.FirstName,
            user.Surname);
    }
}