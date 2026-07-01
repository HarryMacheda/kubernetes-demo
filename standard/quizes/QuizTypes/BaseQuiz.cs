public abstract class BaseQuiz : IQuiz
{
    public int Id { get; set; }
    public string Name { get; set; }
    public QuizType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    protected BaseQuiz(int id, string name, QuizType type, DateTime createdAt, Guid createdBy)
    {
        Id = id;
        Name = name;
        Type = type;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public static IQuiz QuizFromType(int id, string name, QuizType type, DateTime createdAt, Guid createdBy)
    {
        return type switch
        {
            QuizType.Asynchronous => new AsynchronousQuiz(id, name, createdAt, createdBy),
            QuizType.SyncTest => new SyncTestQuiz(id, name, createdAt, createdBy),
            QuizType.SyncLeaderboard => new SyncLeaderboardQuiz(id, name, createdAt, createdBy),
            QuizType.SyncSpeedrun => new SyncSpeedrunQuiz(id, name, createdAt, createdBy),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported quiz type: {type}")
        };
    }
}