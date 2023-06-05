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
        else
        {
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

        // проверка существует ли уже начатый тест исходя из журнала
        if (_dataContext.Journals.Any(j => j.UserId == studentId && j.TestId == testId))
        {
            // уже открыт тест
        }
        else
        {
            _dataContext.Journals.Add(new()
            {
                Id = Guid.NewGuid(),
                UserId = studentId,
                TestId = testId,
                IsPassed = false
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
                    .ToList(),
                CheckedAnswer = null!,
                Index = 0
            })
            .ToList();
        HttpContext.Session.SetString("questions", _quizService.SerializeQuestions(questions));
        ViewBag.TotalQuestions = questions.Count;
        /////////////////////////////////////////////////
        var currentQuestion = questions.First();
        ViewBag.CurrentQuestion = currentQuestion;
        ViewBag.QuestionIndex = 0;
        _dataContext.SaveChanges();
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

    public IActionResult UnifiedPagination(QuestionModel model, string button, int questionIndex)
    {
        if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
        {
            var questions = _quizService.DeserializeQuestions(HttpContext.Session.GetString("questions"));
            ViewBag.TotalQuestions = questions.Count;
            Guid selectedItem;
            try
            {
                selectedItem = Guid.Parse(model.CheckedAnswer);
            }
            catch
            {
                selectedItem = Guid.Empty;
            }


            var question = questions[questionIndex];
            var answer = _dataContext.Answers.FirstOrDefault(a => a.Id == selectedItem);
            if (question is not null && answer is not null)
            {
                var studentId = Guid.Parse(HttpContext.Session.GetString("authUserId"));
                // проверка на существование ответа на этот вопрос
                StudentAnswers studentAnswer = null!;
                try
                {
                    studentAnswer = _dataContext.StudentAnswers.FirstOrDefault(sa =>
                        sa.UserId == Guid.Parse(HttpContext.Session.GetString("authUserId")) &&
                        sa.TestId == Guid.Parse(question.TestId) &&
                        sa.QuestionId == question.Id);
                }
                catch
                {
                }

                if (studentAnswer is not null)
                {
                    _dataContext.StudentAnswers.Remove(studentAnswer);
                }

                _dataContext.StudentAnswers.Add(new()
                {
                    Id = Guid.NewGuid(),
                    UserId = studentId,
                    TestId = Guid.Parse(question.TestId),
                    QuestionId = question.Id,
                    AnswerId = answer.Id,
                    IsRight = answer.IsRight,
                });
            }

            switch (button)
            {
                case "previous":
                {
                    var prevQuestion = questions[questionIndex - 1];
                    ViewBag.CurrentQuestion = prevQuestion;
                    ViewBag.QuestionIndex = questionIndex - 1;
                    _dataContext.SaveChanges();
                    return View("DisplayTest", prevQuestion);
                }
                case "next":
                {
                    var nextQuestion = questions[questionIndex + 1];
                    ViewBag.CurrentQuestion = nextQuestion;
                    ViewBag.QuestionIndex = questionIndex + 1;
                    _dataContext.SaveChanges();
                    return View("DisplayTest", nextQuestion);
                }
                case "finish":
                    var testId = Guid.Parse(question.TestId);
                    var studentId = Guid.Parse(HttpContext.Session.GetString("authUserId"));
                    // создаем модель TestResultModel на основе Journal
                    var journal =
                        _dataContext.Journals.FirstOrDefault(j => j.TestId == testId && j.UserId == studentId);
                    if (journal is not null)
                    {
                        // высчитываем результат
                        var studentAnswers = _dataContext.StudentAnswers
                            .Where(sa => sa.UserId == studentId && sa.TestId == testId)
                            .ToList();
                        var totalRightAnswers = studentAnswers.Count(sa => sa.IsRight);
                        var result = (int)Math.Round((double)totalRightAnswers / studentAnswers.Count * 100);
                        if (totalRightAnswers > 0 && studentAnswers.Count > 0 && result >= 60)
                        {
                            journal.Result = $"{result}%";
                            journal.IsPassed = true;
                        }
                        else
                        {
                            journal.Result = $"{result}%";
                            journal.IsPassed = false;
                        }

                        _dataContext.SaveChanges();
                        return View("TestResult", new TestResultModel()
                        {
                            Result = journal.Result,
                            IsPassed = journal.IsPassed
                        });
                    }

                    break;
                default:
                    throw new Exception("Invalid button");
            }

            return NotFound();
        }

        return RedirectToAction("Auth", "Account");
    }
}