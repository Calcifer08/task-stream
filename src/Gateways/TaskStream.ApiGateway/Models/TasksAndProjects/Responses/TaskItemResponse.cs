using TaskStatus = TaskStream.Shared.Protos.Tasks.TaskStatus;

namespace TaskStream.ApiGateway.Models.Tasks.Responses;

public record TaskItemResponse(
    string Id,
    string Title,
    string? Description,
    TaskStatus Status,
    DateTime? DueDate,
    string ProjectId
);