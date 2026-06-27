public class OAuthClient
{
    public string ClientId { get; set; } = "";
    public string[] AllowedScopes { get; set; } = [];
}

public static class ClientStore
{
    public static readonly List<OAuthClient> Clients =
    [
        new OAuthClient
        {
            ClientId = "web",
            AllowedScopes = ["api"]
        }
    ];
}