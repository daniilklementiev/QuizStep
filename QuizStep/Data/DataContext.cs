using Microsoft.EntityFrameworkCore;
using QuizStep.Data.Entity;

namespace QuizStep.Data;

public class DataContext : DbContext
{
    public DbSet<Entity.Question> Questions { get; set; }
    public DbSet<Entity.User> Users { get; set; }
    public DbSet<Entity.QuestionAnswer> Answers { get; set; }
    public DbSet<Entity.StudentAnswers> StudentAnswers { get; set; }
    public DbSet<Entity.Journal> Journals { get; set; }
    public DbSet<Entity.Test> Tests { get; set; }
    public DbSet<Entity.AssignedTest> AssignedTests { get; set; }

    public DataContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>()
            .HasOne(t => t.Mentor)
            .WithMany(u => u.Tests)
            .HasForeignKey(t => t.MentorId);

        modelBuilder.Entity<Journal>().HasOne(j => j.Test)
            .WithMany(t => t.Journals)
            .HasForeignKey(j => j.TestId);
        
        modelBuilder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId);

        base.OnModelCreating(modelBuilder);
    }
}