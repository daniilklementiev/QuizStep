namespace QuizStep.Models.QuizModels;

public class AnswerModel
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Text { get; set; }
    public bool IsRight { get; set; }
}