using TaskStream.Tasks.Application.DTOs;

namespace TaskStream.Tasks.Application.Interfaces;

public interface ITasksService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string userId);
    Task<IEnumerable<ProjectDto>> GetProjectsAsync(string userId);
    Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, string userId);
    Task<ProjectDto?> UpdateProjectAsync(UpdateProjectDto dto, string userId);
    Task<bool> DeleteProjectAsync(Guid projectId, string userId);


    Task<TaskItemDto?> CreateTaskAsync(CreateTaskDto dto, string userId);
    Task<IEnumerable<TaskItemDto>> GetTasksInProjectAsync(Guid projectId, string userId);
    Task<TaskItemDto?> GetTaskByIdAsync(Guid taskId, string userId);
    Task<TaskItemDto?> UpdateTaskAsync(UpdateTaskDto updateDto, string userId);
    Task<bool> DeleteTaskAsync(Guid taskId, string userId);
}