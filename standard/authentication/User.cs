namespace Authentication
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void SetPassword(string plainPassword)
        {
            PasswordHash = HashPassword(plainPassword);
        }
        
        public bool VerifyPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, PasswordHash);
        }

        public static string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
        }
    }
}