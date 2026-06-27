namespace Authentication
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(string email, string plainPassword, string firstName, string surname);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> VerifyUserPasswordAsync(string email, string plainPassword);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
    }
}
