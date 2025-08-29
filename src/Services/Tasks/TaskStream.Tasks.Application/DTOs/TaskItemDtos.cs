using TaskStream.Tasks.Domain.Entities;

public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDate,
    Guid ProjectId);

public record CreateTaskDto(
    Guid ProjectId,
    string Title,
    string? Description,
    DateTime? DueDate);

public record UpdateTaskDto(
    Guid TaskId,
    string? Title,
    string? Description,
    TaskItemStatus? Status,
    DateTime? DueDate);