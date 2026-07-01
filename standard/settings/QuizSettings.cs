using Authentication;

public static class QuizSettingsHelper
{
    public static QuizSettings GetQuizSettings(UserContext userContext)
    {
        return new QuizSettings
        {
            AvailableQuizTypes = GetAvailableQuizTypes(userContext)
        };
    }

    public static List<QuizTypesAvailablity> GetAvailableQuizTypes(UserContext userContext)
    {
        return new List<QuizTypesAvailablity>
        {
            new QuizTypesAvailablity { QuizType = QuizType.Asynchronous, IsAvailable = false, IsLocked = true },
            new QuizTypesAvailablity { QuizType = QuizType.SyncTest, IsAvailable = false, IsComingSoon = true },
            new QuizTypesAvailablity { QuizType = QuizType.SyncLeaderboard, IsAvailable = true },
            new QuizTypesAvailablity { QuizType = QuizType.SyncSpeedrun, IsAvailable = false, IsComingSoon = true }
        };
    }
}

public class QuizSettings
{
    public List<QuizTypesAvailablity> AvailableQuizTypes { get; init; } = new List<QuizTypesAvailablity>();

    public bool IsQuizTypeAvailable(QuizType type)
    {
        var quizTypeAvailability = AvailableQuizTypes.FirstOrDefault(q => q.QuizType == type);
        return quizTypeAvailability != null && quizTypeAvailability.IsAvailable;
    }
}

public record QuizTypesAvailablity
{
    public QuizType QuizType { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsComingSoon { get; set; } = false;
    public bool IsLocked { get; set; } = false;
}