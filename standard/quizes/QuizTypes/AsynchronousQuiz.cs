public class AsynchronousQuiz : BaseQuiz
{
    public AsynchronousQuiz(int id, string name, DateTime createdAt, Guid createdBy)
        : base(id, name, QuizType.SyncSpeedrun, createdAt, createdBy)
    {
    }
}