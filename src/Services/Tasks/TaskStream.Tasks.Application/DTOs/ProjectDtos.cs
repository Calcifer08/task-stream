namespace TaskStream.Tasks.Application.DTOs;

public record ProjectDto(Guid Id, string Title, string? Description);

public record CreateProjectDto(string Title, string? Description);

public record UpdateProjectDto(Guid ProjectId, string? NewTitle, string? NewDescription);