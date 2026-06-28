using System.Security.Claims;
using OpenIddict.Abstractions;

namespace Authentication
{
    public class UserContext
    {
        public User user { get; set; } = new User();

        public static UserContext FromClaimsPrincipal(ClaimsPrincipal principal)
        {
            var userContext = new UserContext();

            var idClaim = principal.FindFirst(OpenIddictConstants.Claims.Subject);
            if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
            {
                userContext.user.Id = userId;
            }

            var emailClaim = principal.FindFirst(OpenIddictConstants.Claims.Email);
            if (emailClaim != null)
            {
                userContext.user.Email = emailClaim.Value;
            }

            var nameClaim = principal.FindFirst(OpenIddictConstants.Claims.Name);
            if (nameClaim != null)
            {
                var nameParts = nameClaim.Value.Split(' ', 2);
                if (nameParts.Length == 2)
                {
                    userContext.user.FirstName = nameParts[0];
                    userContext.user.Surname = nameParts[1];
                }
                else
                {
                    userContext.user.FirstName = nameClaim.Value;
                }
            }

            return userContext;
        }
    }
}