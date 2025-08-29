using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskStream.Tasks.Application.DTOs;
using TaskStream.Tasks.Application.Interfaces;
using TaskStream.Tasks.Domain.Entities;
using TaskStream.Tasks.Infrastructure.Data;

namespace TaskStream.Tasks.Infrastructure.Services;

public class TasksService : ITasksService
{
    private readonly TasksDbContext _context;
    private readonly IMapper _mapper;

    public TasksService(TasksDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string userId)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            OwnerId = userId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(string userId)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == userId)
            .AsNoTracking()
            .ProjectTo<ProjectDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId, string userId)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto?> UpdateProjectAsync(UpdateProjectDto dto, string userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.OwnerId == userId);

        if (project is null) return null;

        if (dto.NewTitle is not null) project.Title = dto.NewTitle;
        if (dto.NewDescription is not null) project.Description = dto.NewDescription;

        await _context.SaveChangesAsync();

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, string userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

        if (project is null) return false;

        _context.Projects.Remove(project);
        var affectedRows = await _context.SaveChangesAsync();

        return affectedRows > 0;
    }


    public async Task<TaskItemDto?> CreateTaskAsync(CreateTaskDto dto, string userId)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId && p.OwnerId == userId);

        if (!projectExists) return null;

        var taskItem = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            ProjectId = dto.ProjectId,
            Status = TaskItemStatus.ToDo
        };

        _context.TaskItems.Add(taskItem);
        await _context.SaveChangesAsync();

        return _mapper.Map<TaskItemDto>(taskItem);
    }

    public async Task<IEnumerable<TaskItemDto>> GetTasksInProjectAsync(Guid projectId, string userId)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        if (!projectExists) return Enumerable.Empty<TaskItemDto>();

        return await _context.TaskItems
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking()
            .ProjectTo<TaskItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TaskItemDto?> GetTaskByIdAsync(Guid taskId, string userId)
    {
        var taskItem = await _context.TaskItems
            .Include(t => t.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (taskItem is null || taskItem.Project.OwnerId != userId) return null;

        return _mapper.Map<TaskItemDto>(taskItem);
    }

    public async Task<TaskItemDto?> UpdateTaskAsync(UpdateTaskDto dto, string userId)
    {
        var taskItem = await _context.TaskItems
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == dto.TaskId);

        if (taskItem is null || taskItem.Project.OwnerId != userId) return null;

        if (dto.Title is not null) taskItem.Title = dto.Title;
        if (dto.Description is not null) taskItem.Description = dto.Description;
        if (dto.Status is not null) taskItem.Status = dto.Status.Value;
        if (dto.DueDate is not null) taskItem.DueDate = dto.DueDate.Value;

        await _context.SaveChangesAsync();

        return _mapper.Map<TaskItemDto>(taskItem);
    }

    public async Task<bool> DeleteTaskAsync(Guid taskId, string userId)
    {
        var taskItem = await _context.TaskItems
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (taskItem is null || taskItem.Project.OwnerId != userId) return false;

        _context.TaskItems.Remove(taskItem);
        var affectedRows = await _context.SaveChangesAsync();

        return affectedRows > 0;
    }
}
