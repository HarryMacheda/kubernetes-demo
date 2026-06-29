public enum QuizType
{
    Asynchronous, //A test given to a user that they can take at any time.
    SyncTest, //A test given to a user that they must take at a specific time.
    SyncLeaderboard, //A test given to a group at a specific time, a leaderboard is shown after each question.
    SyncSpeedrun, //A test given to a specific group, results shown at the end.
}

public interface IQuiz
{
    int Id { get; set; }
    string Name { get; set; }
    QuizType Type { get; set; }
    DateTime CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
}