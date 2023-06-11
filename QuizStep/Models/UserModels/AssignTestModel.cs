namespace QuizStep.Models.UserModels;

public class AssignTestModel
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public Guid UserId { get; set; }
}