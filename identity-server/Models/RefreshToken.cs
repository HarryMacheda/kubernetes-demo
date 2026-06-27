public class RefreshToken
{
    public string Token { get; set; } = "";
    public Guid UserId { get; set; }
    public string ClientId { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
}