using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuizStep.Models;

namespace QuizStep.Controllers;

public class AccountController : Controller
{

    public ViewResult Registration()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}