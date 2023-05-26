using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using QuizStep.Data;
using QuizStep.Data.Entity;
using QuizStep.Models;
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

    public AccountController(IValidationService validationService, DataContext dataContext, IHashService hashService, IKdfService kdfService, IRandomService randomService)
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
            else {
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
        if (!_validationService.Validate(userRegistrationModel.Login, ValidationTerms.Login))
        {
            validationResults.LoginMessage = "Login doesn't match the pattern";
            isModelValid = false;
        }
        if (_dataContext.Users.Any(u => u.Login.ToLower() == userRegistrationModel.Login))
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
            return View("Profile", userRegistrationModel);
        }
        else
        {
            ViewData["validationResults"] = validationResults;
            return View("Registration");
        }
    }
    
    [HttpPost]
    public String AuthUser()
    {
        StringValues loginValues = Request.Form["user-login"];
        if(loginValues.Count == 0)
        {
            return "No login";
        }
        String login = loginValues[0] ?? "";

        StringValues passwordValues = Request.Form["user-password"];
        if (passwordValues.Count == 0)
        {
            return "No password";
        }
        String password = passwordValues[0] ?? "";

        User? user = _dataContext.Users.Where(u => u.Login == login).FirstOrDefault();
        if (user is not null)   
        { 
            if(user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
            {
                HttpContext.Session.SetString("authUserId", user.Id.ToString());
                user.LastEnterDt = DateTime.Now;
                _dataContext.SaveChanges();
                return $"OK";
            }
        }

        return $"Автентифікацію віхилено";
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}