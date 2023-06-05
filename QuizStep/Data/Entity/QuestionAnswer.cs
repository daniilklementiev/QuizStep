namespace QuizStep.Data.Entity;

public class QuestionAnswer
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public String Text { get; set; } = null!;
    public Boolean IsRight { get; set; }
    
    public Question Question { get; set; }
}