using Newtonsoft.Json;
using QuizStep.Models.QuizModels;

namespace QuizStep.Services.Quiz;

public class QuizService : IQuizService
{
    public string SerializeQuestions(List<QuestionModel> questions)
    {
        if (questions is null)
        {
            return "Invalid input data";
        } 
        if (questions != null && questions.Count == 0)
        {
            return "Empty list";
        }
        return JsonConvert.SerializeObject(questions);
    }

    public List<QuestionModel> DeserializeQuestions(string serializedQuestions)
    {
        if (String.IsNullOrEmpty(serializedQuestions))
        {
            return new List<QuestionModel>();
        }
        return JsonConvert.DeserializeObject<List<QuestionModel>>(serializedQuestions);
    }
}