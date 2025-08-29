namespace TaskStream.Tasks.Domain.Entities;

public enum TaskItemStatus
{
    ToDo,
    InProgress,
    Done
}

public class TaskItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
    public DateTime? DueDate { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}