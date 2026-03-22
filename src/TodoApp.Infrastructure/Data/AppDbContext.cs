using Microsoft.EntityFrameworkCore;
using TodoApp.Core;

namespace TodoApp.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public required DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>()
            .Property(todo => todo.Title)
            .IsRequired()
            .HasMaxLength(200);
    }
}
