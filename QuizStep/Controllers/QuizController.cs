using Microsoft.AspNetCore.Mvc;
using QuizStep.Data;
using QuizStep.Data.Entity;
using QuizStep.Models.QuizModels;
using QuizStep.Services.Quiz;

namespace QuizStep.Controllers;

public class QuizController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IQuizService _quizService;

    public QuizController(DataContext dataContext, IQuizService quizService)
    {
        _dataContext = dataContext; 
        _quizService = quizService;
    }

    public IActionResult StartTest([FromRoute] string id)
    {
        Guid studentId;
        try
        {
            studentId = Guid.Parse(HttpContext.Session.GetString("authUserId"));
        }
        catch
        {
            return RedirectToAction("Auth", "Account");
        }

        Guid testId;
        try
        {
            testId = Guid.Parse(id);
        }
        catch
        {
            return RedirectToAction("Main", "Home");
        }

        if (_dataContext.Journals.Any(j => j.UserId == studentId && j.TestId == testId))
        {
            // уже открыт тест
        }
        else {
            _dataContext.Journals.Add(new()
            {
                Id = Guid.NewGuid(),
                UserId = studentId,
                TestId = testId,
                IsPassed = false,
            });
        }
        
        List<QuestionModel> questions = _dataContext.Questions
            .Where(q => q.TestId == testId)
            .Select(qs => new QuestionModel()
            {
                Id = qs.Id,
                TestId = qs.TestId.ToString(),
                Text = qs.Text,
                Answers = _dataContext.Answers
                    .Where(a => a.QuestionId == qs.Id)
                    .Select(a => new AnswerModel()
                    {
                        Id = a.Id,
                        QuestionId = a.QuestionId,
                        Text = a.Text,
                        IsRight = a.IsRight
                    })
                    .ToList()
            })
            .ToList();
        HttpContext.Session.SetString("questions", _quizService.SerializeQuestions(questions));
        ViewBag.TotalQuestions = questions.Count;
        return View("DisplayTest");
    }

    public IActionResult DisplayTest([FromRoute] String id)
    {
        Guid studentId;
        try
        {
            studentId = Guid.Parse(HttpContext.Session.GetString("authUserId"));
        }
        catch
        {
            return RedirectToAction("Auth", "Account");
        }

        Guid testId;
        try
        {
            testId = Guid.Parse(id);
        }
        catch
        {
            return RedirectToAction("Main", "Home");
        }

        _dataContext.Journals.Add(new()
        {
            Id = Guid.NewGuid(),
            UserId = studentId,
            TestId = testId,
            IsPassed = false,
        });
        List<QuestionModel> questions = _dataContext.Questions
            .Where(q => q.TestId == testId)
            .Select(qs => new QuestionModel()
            {
                Id = qs.Id,
                TestId = qs.TestId.ToString(),
                Text = qs.Text,
                Answers = _dataContext.Answers
                    .Where(a => a.QuestionId == qs.Id)
                    .Select(a => new AnswerModel()
                    {
                        Id = a.Id,
                        QuestionId = a.QuestionId,
                        Text = a.Text,
                        IsRight = a.IsRight
                    })
                    .ToList()
            })
            .ToList();
        HttpContext.Session.SetString("questions", _quizService.SerializeQuestions(questions));
        ViewBag.TotalQuestions = questions.Count;
        /////////////////////////////////////////////////
        var currentQuestion = questions.First();
        ViewBag.CurrentQuestion = currentQuestion;
        ViewBag.QuestionIndex = 0;
        
        return View(currentQuestion);
    }

    public IActionResult NextQuestion(int questionIndex)
    {
        var questions = _quizService.DeserializeQuestions(HttpContext.Session.GetString("questions"));
        ViewBag.TotalQuestions = questions.Count;
        var nextQuestion = questions[questionIndex];
        ViewBag.CurrentQuestion = nextQuestion;
        ViewBag.QuestionIndex = questionIndex;
        return View("DisplayTest", nextQuestion);
    }
    
    public IActionResult PreviousQuestion(int questionIndex)
    {
        var questions = _quizService.DeserializeQuestions(HttpContext.Session.GetString("questions"));
        ViewBag.TotalQuestions = questions.Count;
        var prevQuestion = questions[questionIndex];
        ViewBag.CurrentQuestion = prevQuestion;
        ViewBag.QuestionIndex = questionIndex;
        return View("DisplayTest", prevQuestion);
    }


    // public IActionResult DisplayTest([FromRoute] string id)
    // {
    //     Guid testId;
    //     List<QuestionModel> questions;
    //     try
    //     {
    //         testId = Guid.Parse(id);
    //         questions = _dataContext.Questions
    //             .Where(q => q.TestId == testId)
    //             .Select(qs => new QuestionModel()
    //             {
    //                 Id = qs.Id,
    //                 TestId = qs.TestId.ToString(),
    //                 Text = qs.Text,
    //                 Answers = _dataContext.Answers
    //                     .Where(a => a.QuestionId == qs.Id)
    //                     .Select(a => new AnswerModel()
    //                     {
    //                         Id = a.Id,
    //                         QuestionId = a.QuestionId,
    //                         Text= a.Text,
    //                         IsRight = a.IsRight
    //                     })
    //                     .ToList()
    //             })
    //             .ToList();
    //     }
    //     catch
    //     {
    //         throw new Exception("Invalid id");
    //     }
    //
    //     if (questions.Count == 0)
    //     {
    //         throw new Exception("Empty list");
    //     }
    //     
    //     Guid questionId = new Guid("1999918b-46ae-4499-b61e-b42a92e0c06f");
    //     var question = _dataContext.Questions.FirstOrDefault(q => q.Id == questionId);
    //
    //     if (question != null)
    //     {
    //         var answers = _dataContext.Answers
    //             .Where(a => a.QuestionId == questionId)
    //             .Select(a => new AnswerModel()
    //             {
    //                 Id = a.Id,
    //                 QuestionId = a.QuestionId,
    //                 Text= a.Text,
    //                 IsRight = a.IsRight
    //             })
    //             .ToList();
    //
    //         var questionViewModel = new QuestionModel()
    //         {
    //             Id = question.Id,
    //             Text = question.Text,
    //             Answers = answers
    //         };
    //         return View(questionViewModel);
    //     }
    //     else
    //     {
    //         // Обработка случая, когда вопрос с указанным QuestionId не найден
    //         return NotFound();
    //     }
    // }
}