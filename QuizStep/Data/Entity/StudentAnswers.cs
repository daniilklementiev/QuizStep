namespace QuizStep.Data.Entity;

public class StudentAnswers
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid AnswerId { get; set; }
    public Guid TestId { get; set; }
    public bool IsRight { get; set; }
}