using Authentication;
using Microsoft.AspNetCore.Mvc;

public class StandardController : ControllerBase
{
    public UserContext Context => UserContext.FromClaimsPrincipal(User);
}