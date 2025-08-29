using System.ComponentModel.DataAnnotations;

namespace TaskStream.ApiGateway.Models.Tasks.Requests;

public record CreateProjectRequest(
    [Required, MaxLength(100)] string Title,
    [MaxLength(500)] string? Description
);