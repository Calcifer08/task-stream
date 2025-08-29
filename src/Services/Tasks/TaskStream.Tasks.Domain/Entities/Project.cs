namespace TaskStream.Tasks.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string OwnerId { get; set; }

    public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
}