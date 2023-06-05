namespace QuizStep.Data.Entity;

public class Test
{
    public Guid Id { get; set; }
    public Guid MentorId { get; set; }
    public String Icon { get; set; } = null!;
    public String Name { get; set; } = null!;
    
    // навигационное свойство
    public User? Mentor { get; set; }
    public ICollection<Journal> Journals { get; set; }
}