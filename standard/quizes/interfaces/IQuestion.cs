public enum QuestionType
{
    SingleChoice,
    MultipleChoice,
    TrueFalse,
    ShortAnswer,
    LongAnswer
}

public interface IQuestion 
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int Index { get; set; }
    public QuestionType Type { get; set; }
    public DateTime CreatedAt { get; set; }  
    public Guid CreatedBy { get; set; }
    public string QuestionText { get; set; }
}
