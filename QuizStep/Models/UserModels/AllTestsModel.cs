namespace QuizStep.Models.UserModels;

public class AllTestsModel
{
    public Guid Id { get; set; }
    public Guid MentorId { get; set; }
    public String MentorName { get; set; } = null!;
    public String? Icon { get; set; } = null!;
    public String Name { get; set; } = null!;
    public bool IsPassed { get; set; }
    public String? Result { get; set; } = null!;
    public int QuestionsCount { get; set; }
}