namespace QuizStep.Models.QuizModels;

public class QuizTestModel
{
    public Guid Id { get; set; }
    public Guid MentorId { get; set; }
    public String MentorName { get; set; } = null!;
    public String? Icon { get; set; } = null!;
    public String Name { get; set; } = null!;
}