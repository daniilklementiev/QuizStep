namespace QuizStep.Models.QuizModels;

public class QuestionModel
{
    public Guid Id { get; set; }
    public String TestId { get; set; }
    public string Text { get; set; } = null!;
    public List<AnswerModel> Answers { get; set; }
}