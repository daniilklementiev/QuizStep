using QuizStep.Data.Entity;
using QuizStep.Models.QuizModels;

namespace QuizStep.Models.UserModels;

public class StudentTestModel
{
    public Guid Id { get; set; }
    public Guid MentorId { get; set; }
    public String? Icon { get; set; }
    public String TestTitle { get; set; } = null!;
    public String       Login              { get; set; } = null!;
    public String       RealName           { get; set; } = null!;
    public bool IsPersonal { get; set; }
    public String Role { get; set; }
    public String?      Avatar             { get; set; } = null!;
    
    public List<AllTestsModel> Quizzes { get; set; } = null!;
    


    public StudentTestModel(Data.Entity.User user)
    {
        // object mapping 
        var userProps = user.GetType().GetProperties();
        var thisProps = this.GetType().GetProperties();
        foreach (var thisProp in thisProps)
        {
            var prop = userProps.FirstOrDefault(
                userProp => userProp.Name == thisProp.Name && 
                            userProp.PropertyType.IsAssignableTo(thisProp.PropertyType)
            );
            if (prop is not null)
            {
                thisProp.SetValue(this, prop.GetValue(user));
            }
        }
    }

    public StudentTestModel()
    {
        Login = RealName = null!;
    }
}