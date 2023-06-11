namespace QuizStep.Models.UserModels;

public class StudentModel
{
    public Guid Id { get; set; }
    public String Realname { get; set; } = null!;
    public String Login { get; set; } = null!;
    public String Avatar { get; set; } = null!;
    public String Role { get; set; } = null!;
    public Guid? AssignTestId { get; set; } = Guid.Empty;
    public List<Data.Entity.Test> Tests { get; set; } = null!;
}