namespace TaskStream.ApiGateway.Models.Tasks.Requests;

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    string? Status,
    DateTime? DueDate
);