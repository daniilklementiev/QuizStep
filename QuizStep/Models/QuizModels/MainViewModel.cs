using QuizStep.Data.Entity;

namespace QuizStep.Models.QuizModels;

public class MainViewModel
{
    public List<QuizTestModel> Tests { get; set; } = null!;
}