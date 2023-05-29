namespace QuizStep.Data.Entity;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public Guid TestId { get; set; }
}