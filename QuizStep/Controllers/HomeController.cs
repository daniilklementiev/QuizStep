using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizStep.Data;
using QuizStep.Models;
using QuizStep.Models.QuizModels;

namespace QuizStep.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _dataContext;
    public HomeController(ILogger<HomeController> logger, DataContext dataContext)
    {
        _logger = logger;
        _dataContext = dataContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Main()
    {
        var userIdString = HttpContext.Session.GetString("authUserId");
        try
        { 
            var userId = Guid.Parse(userIdString);
            // var quizzes = _dataContext.Tests.ToList();
            // var assignedTests = _dataContext.AssignedTests.Where(at => at.StudentId == userId).ToList();
            // var filteredQuizzes = quizzes
            //     .Where(t => journals.Any(j => j.TestId == t.Id) || assignedTests.Any(at => at.TestId == t.Id))
            //     .AsEnumerable();
            MainViewModel model = new()
            {
                Tests = _dataContext.Tests.Select(t => new QuizTestModel
                    {
                        Id = t.Id,
                        MentorId = t.MentorId,
                        Icon = t.Icon,
                        Name = t.Name,
                        Description = t.Description,
                        MentorName = _dataContext.Users.FirstOrDefault(u=>u.Id == t.MentorId).RealName ?? String.Empty,
                        QuestionsCount = _dataContext.Questions.Count(q => q.TestId == t.Id)
                    }).ToList()
            };
            return View(model);
        }
        catch (Exception)
        {
            return RedirectToAction("Auth", "Account");
        }

        return NotFound();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statuscode)
    {
        if (statuscode == 404)
        {
            return View("404");
        }
        else
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}