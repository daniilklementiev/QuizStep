namespace QuizStep.Data.Entity;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public Guid TestId { get; set; }
    
    public List<QuestionAnswer> Answers { get; set; }
    public Test Test { get; set; } = null!;
}