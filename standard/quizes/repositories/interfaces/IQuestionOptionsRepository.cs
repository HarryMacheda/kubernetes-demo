public interface IQuestionOptionsRepository
{
    public Task<IQuestionOption> CreateQuestionOptionAsync(IQuestionOption questionOption);
    public Task<IQuestionOption?> GetQuestionOptionByIdAsync(int id);
    public Task<IEnumerable<IQuestionOption>> GetAllQuestionOptionsForQuestionAsync(int questionId);
    public Task<IEnumerable<IQuestionOption>> GetAllQuestionOptionsForQuizAsync(int quizzId);
    public Task UpdateQuestionOptionAsync(IQuestionOption questionOption);
    public Task DeleteQuestionOptionAsync(int id);
}