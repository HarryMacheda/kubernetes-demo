namespace identity_server.Models
{
    public class ExternalLoginInfo
    {
        public string Provider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class OAuth2CallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? State { get; set; }
    }
}
