using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using QuizStep.Data;
using QuizStep.Data.Entity;
using QuizStep.Models;
using QuizStep.Models.QuizModels;
using QuizStep.Models.UserModels;
using QuizStep.Services.Hash;
using QuizStep.Services.Kdf;
using QuizStep.Services.RandomService;
using QuizStep.Services.Validation;

namespace QuizStep.Controllers;

public class AccountController : Controller
{
    private readonly DataContext _dataContext;
    private readonly IValidationService _validationService;
    private readonly IHashService _hashService;
    private readonly IRandomService _randomService;
    private readonly IKdfService _kdfService;

    public AccountController(IValidationService validationService, DataContext dataContext, IHashService hashService,
        IKdfService kdfService, IRandomService randomService)
    {
        _validationService = validationService;
        _dataContext = dataContext;
        _hashService = hashService;
        _kdfService = kdfService;
        _randomService = randomService;
    }

    public IActionResult Registration()
    {
        return View();
    }

    public IActionResult Auth()
    {
        ViewData["results"] = TempData["results"];
        return View();
    }

    public IActionResult RegisterUser(UserRegistrationModel userRegistrationModel)
    {
        UserValidationModel validationResults = new();
        bool isModelValid = true;

        String patternEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$";
        String patternRealName = "^[a-zA-Zа-яА-Я]+([-'][a-zA-Zа-яА-Я]+)*$";

        #region Avatar Uploading

        string avatarFilename = null!;
        if (userRegistrationModel.Avatar is not null)
        {
            if (userRegistrationModel.Avatar.Length <= 1000)
            {
                validationResults.AvatarMessage = "Too small file. File must be larger than 1Kb";
                isModelValid = false;
            }
            else
            {
                String ext = Path.GetExtension(userRegistrationModel.Avatar.FileName);
                String hash = (_hashService.Hash(
                    userRegistrationModel.Avatar.FileName + Guid.NewGuid()))[..16];
                avatarFilename = hash + ext;
                string path = "wwwroot/avatars/" + avatarFilename;
                bool isWrongFile = System.IO.File.Exists(path);
                if (!isWrongFile)
                {
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        userRegistrationModel.Avatar.CopyTo(fileStream);
                    }
                }

                ViewData["avatarFilename"] = avatarFilename;
            }
        }

        #endregion

        #region Login valid

        if (String.IsNullOrEmpty(userRegistrationModel.Login))
        {
            validationResults.LoginMessage = "Login is required";
            isModelValid = false;
        }
        else if (!_validationService.Validate(userRegistrationModel.Login, ValidationTerms.Login))
        {
            validationResults.LoginMessage = "Login doesn't match the pattern";
            isModelValid = false;
        }
        else if (_dataContext.Users.Any(u => u.Login.ToLower() == userRegistrationModel.Login))
        {
            validationResults.LoginMessage = $"Login '{userRegistrationModel.Login}' is already taken";
            isModelValid = false;
        }

        #endregion

        #region password valid

        if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.NotEmpty))
        {
            validationResults.PasswordMessage = "Password is required";
            isModelValid = false;
        }
        else if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.Password))
        {
            validationResults.PasswordMessage = $"Password is too short. Min is 3 symbols";
            isModelValid = false;
        }

        if (!_validationService.Validate(userRegistrationModel.RepeatPassword, ValidationTerms.NotEmpty))
        {
            validationResults.RepeatPasswordMessage = "Repeat password is required";
            isModelValid = false;
        }
        else if (!userRegistrationModel.RepeatPassword.Equals(userRegistrationModel.Password))
        {
            validationResults.RepeatPasswordMessage = "Passwords do not match the pattern";
            isModelValid = false;
        }

        #endregion

        #region Email valid

        if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.NotEmpty))
        {
            validationResults.EmailMessage = "Email is empty";
            isModelValid = false;
        }
        else if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.Email))
        {
            validationResults.EmailMessage = "Email do not match the pattern";
            isModelValid = false;
        }
        else
        {
            if (!Regex.IsMatch(userRegistrationModel.Email, patternEmail, RegexOptions.IgnoreCase))
            {
                validationResults.EmailMessage = "Email is invalid";
                isModelValid = false;
            }
        }

        #endregion

        #region RealName valid

        if (String.IsNullOrEmpty(userRegistrationModel.RealName))
        {
            validationResults.RealNameMessage = "Real name is required";
            isModelValid = false;
        }
        else
        {
            if (!Regex.IsMatch(userRegistrationModel.RealName, patternRealName, RegexOptions.IgnoreCase))
            {
                validationResults.RealNameMessage = "Real name is invalid";
                isModelValid = false;
            }
        }

        #endregion

        #region IsAgree valid

        if (!userRegistrationModel.IsAgree)
        {
            validationResults.IsAgreeMessage = "You must agree to the terms";
            isModelValid = false;
        }

        #endregion


        if (isModelValid)
        {
            // формируем сущность пользователя и добавляем в контекст
            String salt = _randomService.RandomString(8);

            Data.Entity.User user = new()
            {
                Id = Guid.NewGuid(),
                Login = userRegistrationModel.Login,
                PasswordHash = _kdfService.GetDerivedKey(userRegistrationModel.Password, salt),
                PasswordSalt = salt,
                Role = "Student",
                Email = userRegistrationModel.Email,
                EmailCode = _randomService.ConfirmCode(6),
                RealName = userRegistrationModel.RealName,
                Avatar = avatarFilename,
                RegisterDt = DateTime.Now,
                LastEnterDt = null,
            };
            _dataContext.Users.Add(user);

            // генеруємо токен підтвердження пошти
            // var emailConfirmToken = _GenerateEmailToken(user);
            _dataContext.SaveChanges();

            // // надсилаємо код підтвердження пошти
            // _SendConfirmEmail(user, emailConfirmToken);
            return RedirectToAction("Main", "Home");
        }
        else
        {
            ViewData["validationResults"] = validationResults;
            return View("Registration");
        }
    }

    public RedirectToActionResult Logout()
    {
        HttpContext.Session.Remove("authUserId");
        return RedirectToAction("Auth", "Account");
    }

    public IActionResult Profile([FromRoute] String id)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
        // получаем общее количество пользователей в базе данных
        if (user is not null)
        {
            ProfileModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Students = _dataContext.Users.Count(u => u.Role == "Student")
            };
            if (String.IsNullOrEmpty(model.Avatar))
            {
                model.Avatar = "no_avatar.png";
            }

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return View(model);
        }
        else
        {
            return NotFound();
        }
    }

    public IActionResult ProfileStudents([FromRoute] String id)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
        // получаем общее количество пользователей в базе данных
        if (user is not null)
        {
            ProfileStudentsModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Students = _dataContext.Users.Select(s => new StudentModel()
                {
                    Id = s.Id,
                    Login = s.Login,
                    Realname = s.RealName,
                    Email = s.Email,
                    Role = s.Role,
                    Tests = null!
                }).ToList()
            };

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return View(model);
        }
        else
        {
            return NotFound();
        }
    }


    public IActionResult ProfileTests([FromRoute] String id)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
        if (user is not null)
        {
            var quizzes = _dataContext.Tests.ToList();
            var journals = _dataContext.Journals.Where(j => j.UserId == user.Id).ToList();
            var assignedTests = _dataContext.AssignedTests.Where(at => at.StudentId == user.Id).ToList();
            StudentTestModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Role = user.Role,
                Quizzes = quizzes
                    .Where(t => journals.Any(j => j.TestId == t.Id) || assignedTests.Any(at => at.TestId == t.Id))
                    .Select(t => new AllTestsModel()
                    {
                        Icon = t.Icon,
                        Id = t.Id,
                        MentorId = t.MentorId,
                        Name = t.Name,
                        IsPassed = journals.Any(j => j.IsPassed && j.TestId == t.Id),
                        MentorName = t.Mentor?.RealName ?? String.Empty,
                        QuestionsCount = _dataContext.Questions.Count(q => q.TestId == t.Id),
                        Result = journals.FirstOrDefault(j => j.TestId == t.Id)?.Result,
                    }).ToList()
            };

            return View("ProfileStudentTests", model);
        }
        else
        {
            return NotFound();
        }
    }

    public IActionResult ProfileTestsEdit([FromRoute] String id)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
        if (user is not null)
        {
            ProfileTestsModel model = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Tests = _dataContext.Tests.AsEnumerable().Select(t => new MentorTestModel()
                {
                    Icon = t.Icon,
                    Id = t.Id,
                    MentorId = t.MentorId,
                    TestTitle = t.Name,
                    Questions = null!
                }).ToList()
            };


            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }
            }

            return View("ProfileTests", model);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost]
    public IActionResult CreateTest(MentorTestModel model)
    {
        var user = _dataContext.Users.FirstOrDefault(u => u.Login == model.Login);
        if (user is not null)
        {
            if (!_dataContext.Tests.Any(t => t.Name == model.TestTitle))
            {
                _dataContext.Tests.Add(new Test()
                {
                    Id = Guid.NewGuid(),
                    Name = model.TestTitle,
                    MentorId = user.Id,
                    Icon =
                        "https://img.freepik.com/premium-vector/clipboard-with-checklist-flat-style_183665-74.jpg?w=1060", // стандартное изображение для теста
                    Journals = null!,
                    Mentor = user
                });
                _dataContext.SaveChanges();
            }

            ProfileTestsModel newModel = new()
            {
                Id = user.Id,
                Login = user.Login,
                RealName = user.RealName,
                Avatar = user.Avatar,
                Email = user.Email,
                Role = user.Role,
                Tests = _dataContext.Tests.AsEnumerable().Select(t => new MentorTestModel()
                {
                    Icon = t.Icon,
                    Id = t.Id,
                    MentorId = t.MentorId,
                    TestTitle = t.Name,
                    Questions = null!
                }).ToList()
            };

            return View("ProfileTests", newModel);
        }

        return NotFound();
    }

    public IActionResult EditMentorTest([FromRoute] string id)
    {
        var test = _dataContext.Tests.FirstOrDefault(t => t.Id == Guid.Parse(id));
        if (test is not null)
        {
            var user = _dataContext.Users.FirstOrDefault(u => u.Id == test.MentorId);
            if (user is not null)
            {
                MentorTestModel newModel = new()
                {
                    Id = user.Id,
                    Login = user.Login,
                    RealName = user.RealName,
                    Avatar = user.Avatar,
                    Role = user.Role,
                    TestTitle = test.Name,
                    Icon = test.Icon,
                    TestId = test.Id,
                    Questions = new List<QuestionModel>
                    {
                        new QuestionModel
                        {
                            Id = Guid.NewGuid(),
                            TestId = test.Id.ToString(),
                            Answers = new List<AnswerModel>
                            {
                                new AnswerModel { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid() },
                                new AnswerModel { Id = Guid.NewGuid(), QuestionId = Guid.NewGuid() }
                            }
                        }
                    }
                };

                return View(newModel);
            }
        }


        return NotFound();
    }

    [HttpPost]
    public IActionResult AddQuestions(MentorTestModel model)
    {
        // Сохранение вопросов и ответов в базу данных
        foreach (var question in model.Questions)
        {
            // Сохранение вопроса
            _dataContext.Questions.Add(new ()
            {
                Id = Guid.NewGuid(),
                Text = question.Text,
                TestId = model.TestId
            });

            foreach (var answer in question.Answers)
            {
                // Сохранение ответа
                _dataContext.Answers.Add(new()
                {
                    Id = Guid.NewGuid(),
                    IsRight = answer.IsRight,
                    QuestionId = question.Id,
                    Text = answer.Text
                });
            }
        }

        _dataContext.SaveChanges();

        return RedirectToAction("ProfileTests", "Account");
    }


    [HttpPost]
    public RedirectToActionResult AuthUser()
    {
        StringValues loginValues = Request.Form["user-login"];
        if (loginValues.Count == 0)
        {
            // no login
            TempData["results"] = "No login";
            return RedirectToAction("Auth", "Account");
        }

        String login = loginValues[0] ?? "";

        StringValues passwordValues = Request.Form["user-password"];
        if (passwordValues.Count == 0)
        {
            // no password
            TempData["results"] = "No password";
            return RedirectToAction("Auth", "Account");
        }

        String password = passwordValues[0] ?? "";

        User? user = _dataContext.Users.FirstOrDefault(u => u.Login == login);
        if (user is not null)
        {
            if (user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
            {
                HttpContext.Session.SetString("authUserId", user.Id.ToString());
                return RedirectToAction("Main", "Home");
            }
        }

        // no user
        TempData["results"] = "Wrong login or password";
        return RedirectToAction("Auth", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}