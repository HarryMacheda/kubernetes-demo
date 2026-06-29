public interface IQuestionRepository
{
    public Task<IQuestion> CreateQuestionAsync(IQuestion question);
    public Task<IQuestion?> GetQuestionByIdAsync(int id);
    public Task<IEnumerable<IQuestion>> GetAllQuestionsForQuizAsync(int quizId);
    public Task UpdateQuestionAsync(IQuestion question);
    public Task DeleteQuestionAsync(int id);
}