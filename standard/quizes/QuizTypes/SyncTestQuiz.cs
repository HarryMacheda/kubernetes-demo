public class SyncTestQuiz : BaseQuiz
{
    public SyncTestQuiz(int id, string name, DateTime createdAt, Guid createdBy)
        : base(id, name, QuizType.SyncSpeedrun, createdAt, createdBy)
    {
    }
}