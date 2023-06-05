namespace QuizStep.Models.UserModels;

public class ProfileModel
{
    public Guid         Id                 { get; set; }
    public String       Login              { get; set; } = null!;
    public String       RealName           { get; set; } = null!;
    public String Role { get; set; }
    public String       Email              { get; set; } = null!;
    public String?      Avatar             { get; set; } = null!;
    public bool IsPersonal { get; set; }

    public int Students { get; set; }

    public ProfileModel(Data.Entity.User user)
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

    public ProfileModel()
    {
        Login = RealName = Email = null!;
    }
}