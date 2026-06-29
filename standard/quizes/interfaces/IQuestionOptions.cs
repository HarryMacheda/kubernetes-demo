public interface IQuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int Index { get; set; }
    public bool IsCorrect { get; set; }
    public string OptionText { get; set; }
    public string OptionAnswerText { get; set; }
}