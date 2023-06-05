namespace QuizStep.Data.Entity;

public class Journal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TestId { get; set; }
    public String? Result { get; set; } = null!;
    public Boolean IsPassed { get; set; }

    public Test? Test { get; set; }
}