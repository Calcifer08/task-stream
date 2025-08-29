using System.ComponentModel.DataAnnotations;

namespace TaskStream.ApiGateway.Models.Tasks.Requests;

public record UpdateProjectRequest(
    [MaxLength(100)] string? Title,
    [MaxLength(500)] string? Description
);