using QuizStep.Data.Entity;
using QuizStep.Models.QuizModels;

namespace QuizStep.Models.UserModels;

public class QuestionEditingModel
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<QuestionAnswer> Answers { get; set; } = new();
}