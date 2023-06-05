namespace QuizStep.Models.QuizModels;

public class QuestionModel
{
    public Guid Id { get; set; }
    public String TestId { get; set; }
    public String Text { get; set; } = null!;
    public List<AnswerModel> Answers { get; set; }
    
    public String CheckedAnswer { get; set; }

    public int Index { get; set; }

}