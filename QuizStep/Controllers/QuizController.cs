using Microsoft.AspNetCore.Mvc;
using QuizStep.Data;
using QuizStep.Data.Entity;
using QuizStep.Models.QuizModels;

namespace QuizStep.Controllers;

public class QuizController : Controller
{
    
    private readonly DataContext _dataContext;
    private Dictionary<Guid, int> _testQuestionCounts;
    public QuizController(DataContext dataContext)
    {
        _dataContext = dataContext;
        _testQuestionCounts = _dataContext.Tests
        .Select(t => new
        {
            TestId = t.Id,
            QuestionCount = _dataContext.Questions.Count(q => q.TestId == t.Id)
        })
        .ToDictionary(x => x.TestId, x => x.QuestionCount);
    }

    public IActionResult DisplayTest([FromRoute] string id)
    {
        // try
        // {
        //     var testId = Guid.Parse(id);
        //     List<QuestionModel> questions = _dataContext.Questions
        //         .Where(q => q.TestId == testId)
        //         .Select(qs => new QuestionModel()
        //         {
        //             TestId = qs.TestId.ToString(),
        //             Text = qs.Text,
        //             Answers = _dataContext.Answers
        //                 .Where(a => a.QuestionId == qs.Id)
        //                 .Select(a => new AnswerModel()
        //                 {
        //                     Id = a.Id,
        //                     QuestionId = a.QuestionId,
        //                     Text= a.Text,
        //                     IsRight = a.IsRight
        //                 })
        //                 .ToList()
        //         })
        //         .ToList();
        // }
        // catch
        // {
        //     throw new Exception("Invalid id");
        // }


        Guid questionId = new Guid("1999918b-46ae-4499-b61e-b42a92e0c06f");
        var question = _dataContext.Questions.FirstOrDefault(q => q.Id == questionId);

        if (question != null)
        {
            var answers = _dataContext.Answers
                .Where(a => a.QuestionId == questionId)
                .Select(a => new AnswerModel()
                {
                    Id = a.Id,
                    QuestionId = a.QuestionId,
                    Text= a.Text,
                    IsRight = a.IsRight
                })
                .ToList();

            var questionViewModel = new QuestionModel()
            {
                Text = question.Text,
                Answers = answers
            };
            ViewData["pagesCount"] = _testQuestionCounts[Guid.Parse(id)];
            return View(questionViewModel);
        }
        else
        {
            // Обработка случая, когда вопрос с указанным QuestionId не найден
            return NotFound();
        }
    }
    
    
}