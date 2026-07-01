using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/quiz")]
public class QuizController : StandardController
{
    private readonly IQuizRepository _quizRepo;

    public QuizController(IQuizRepository repo)
    {
        _quizRepo = repo;
    }

    public class CreateQuizRequest
    {
        public string Name {get; set;} = string.Empty;
        public QuizType Type {get; set;}
    }


    [HttpPost("new")]
    [Authorize]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
    {
        QuizSettings quizSettings = QuizSettingsHelper.GetQuizSettings(Context);
        if (!quizSettings.IsQuizTypeAvailable(request.Type))
        {
            return BadRequest("Selected quiz type is not available.");
        }

        IQuiz quiz = BaseQuiz.QuizFromType(
            0,
            request.Name,
            request.Type,
            DateTime.Now,
            Context.user.Id
        );
        
        await _quizRepo.CreateQuizAsync(quiz);

        return Ok(quiz);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetQuiz(int id)
    {    
        IQuiz quiz = await _quizRepo.GetQuizByIdAsync(id);

        return Ok(quiz);
    }
}