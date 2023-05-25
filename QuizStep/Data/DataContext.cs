using Microsoft.EntityFrameworkCore;
using QuizStep.Data.Entity;

namespace QuizStep.Data;

public class DataContext : DbContext
{
    public DbSet<Entity.Question> Questions { get; set; }
    public DbSet<Entity.User>     Users     { get; set; }
    
    public DataContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
    
}