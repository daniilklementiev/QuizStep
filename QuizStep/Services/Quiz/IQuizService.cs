namespace QuizStep.Services.Quiz;

public interface IQuizService
{
    public String SerializeQuestions(List<Models.QuizModels.QuestionModel> questions);
    public List<Models.QuizModels.QuestionModel> DeserializeQuestions(string serializedQuestions);
}