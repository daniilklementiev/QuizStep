namespace QuizStep.Data.Entity;

public class Journal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TestId { get; set; }
    public Boolean IsPassed { get; set; }
}