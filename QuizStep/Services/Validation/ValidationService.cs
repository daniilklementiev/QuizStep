using System.Text.RegularExpressions;

namespace QuizStep.Services.Validation;

public class ValidationService : IValidationService
{
    public bool Validate(string source, params ValidationTerms[] terms)
    {
        if (terms.Length == 0)
        {
            throw new ArgumentException("No terms for validation");
        }
        if(terms.Length == 1 && terms[0] == ValidationTerms.None)
        {
            return true;
        }
        bool result = true;
        if(terms.Contains(ValidationTerms.NotEmpty))
        {
            result &= !String.IsNullOrEmpty(source);
            // result = result && x
        }
        if(terms.Contains(ValidationTerms.Email))
        {
            result &= ValidateEmail(source);
        }
        if(terms.Contains(ValidationTerms.Login))
        {
            result &= ValidateLogin(source);
        }
        if(terms.Contains(ValidationTerms.Realname))
        {
            result &= ValidateRealname(source);
        }
        if(terms.Contains(ValidationTerms.Password))
        {
            result &= ValidatePassword(source);
        }

        return result;
    }
    
    private static bool ValidateRegex(String source, String pattern)
    {
        if (source is null)
            return false;
        return Regex.IsMatch(source, pattern);
    }
    private static Boolean ValidateEmail(String source)
    {
        return ValidateRegex(source, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$");
    }
    
    private static Boolean ValidateRealname(String source)
    {
        return ValidateRegex(source, "^[a-zA-Zа-яА-Я]+([-' ][a-zA-Zа-яА-Я]+)*$");
    }

    private static Boolean ValidateLogin(String source)
    {
        String loginRegex = @"^\w{3,}$";
        return ValidateRegex(source, loginRegex);
    }

    private static Boolean ValidatePassword(String source)
    {
        return source.Length >= 3;
    }
}