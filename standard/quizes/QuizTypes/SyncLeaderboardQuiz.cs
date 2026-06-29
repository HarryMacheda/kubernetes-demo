public class SyncLeaderboardQuiz : BaseQuiz
{
    public SyncLeaderboardQuiz(int id, string name, DateTime createdAt, Guid createdBy)
        : base(id, name, QuizType.SyncSpeedrun, createdAt, createdBy)
    {
    }
}