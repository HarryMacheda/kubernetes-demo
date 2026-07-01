using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/quiz/{quizId}/question")]
public class QuestionController : StandardController
{
    private readonly IQuizRepository _quizRepo;
    private readonly IQuestionRepository _questionRepo;

    public QuestionController(IQuizRepository quiz, IQuestionRepository repo)
    {
        _quizRepo = quiz;
        _questionRepo = repo;
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetQuestions(int quizId)
    {    
        IEnumerable<IQuestion> questions = await _questionRepo.GetAllQuestionsForQuizAsync(quizId);

        return Ok(questions);
    }

    public class CreateQuestionRequest
    {
        public string QuestionText {get; set;} = string.Empty;
        public QuestionType Type {get; set;}
        public int Index {get; set;}
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateQuestion(int quizId, [FromBody] CreateQuestionRequest request)
    {    
        IQuestion question = new Question
        {
            QuestionText = request.QuestionText,
            Type = request.Type,
            Index = request.Index,
            CreatedAt = DateTime.Now,
            CreatedBy = Context.user.Id,
            QuizId = quizId
        };

        await _questionRepo.CreateQuestionAsync(question);

        return Ok(question);
    }

    public class UpdateQuestionRequest
    {
        public string QuestionText {get; set;} = string.Empty;
    }

    [HttpPut("{questionId}")]
    [Authorize]
    public async Task<IActionResult> UpdateQuestion(int quizId, int questionId, [FromBody] UpdateQuestionRequest request)
    {
        IQuestion question = await _questionRepo.GetQuestionByIdAsync(questionId);

        if (question == null || question.QuizId != quizId)
        {
            return NotFound("Question not found for the specified quiz.");
        }

        question.QuestionText = request.QuestionText;

        await _questionRepo.UpdateQuestionAsync(question);

        return Ok(question);
    }
}