public interface IQuizRepository
{
    Task<IQuiz> CreateQuizAsync(IQuiz quiz);
    Task<IQuiz?> GetQuizByIdAsync(int id);
    Task<IEnumerable<IQuiz>> GetAllQuizzesAsync();
    Task UpdateQuizAsync(IQuiz quiz);
    Task DeleteQuizAsync(int id);
}