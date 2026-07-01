using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/settings")]
public class SettingsController : StandardController
{
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(Context.user);
    }

    [HttpGet("quiz")]
    [Authorize]
    public IActionResult Quiz()
    {
        return Ok(QuizSettingsHelper.GetQuizSettings(Context));
    }
}