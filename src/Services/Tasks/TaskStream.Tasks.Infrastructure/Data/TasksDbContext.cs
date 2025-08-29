using Microsoft.EntityFrameworkCore;
using TaskStream.Tasks.Domain.Entities;

namespace TaskStream.Tasks.Infrastructure.Data;

public class TasksDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }

    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<TaskItemStatus>();

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => p.OwnerId);
            entity.HasMany(p => p.TaskItems)
                  .WithOne(t => t.Project)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(200);
        });
    }
}