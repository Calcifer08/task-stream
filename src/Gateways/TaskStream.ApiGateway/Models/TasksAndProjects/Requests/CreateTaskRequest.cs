using System.ComponentModel.DataAnnotations;

namespace TaskStream.ApiGateway.Models.Tasks.Requests;

public record CreateTaskRequest(
    [Required, MaxLength(100)] string Title,
    string? Description,
    DateTime? DueDate
);