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
        MainViewModel model = new()
        {
            Tests = _dataContext.Tests.
                Select(x => new QuizTestModel
            {
                Id = x.Id,
                MentorId = x.MentorId,
                Icon = x.Icon,
                Name = x.Name,
                MentorName = x.Mentor!.RealName ?? "No mentor"
            }).ToList()
        };
        return View(model);

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