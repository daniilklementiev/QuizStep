namespace QuizStep.Data.Entity;

public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public string? Answer { get; set; } = null!;
    public string Options { get; set; } = null!;
}