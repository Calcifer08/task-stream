using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskStream.Tasks.Application.DTOs;
using TaskStream.Tasks.Application.Interfaces;
using TaskStream.Tasks.Domain.Entities;
using TaskStream.Tasks.Infrastructure.Data;

namespace TaskStream.Tasks.Infrastructure.Services;

public class TasksService : ITasksService
{
    private readonly TasksDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksService> _logger;
    private readonly ITasksEventPublisher _eventPublisher;

    public TasksService(TasksDbContext context, IMapper mapper, ILogger<TasksService> logger, ITasksEventPublisher eventPublisher)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _eventPublisher = eventPublisher;
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

        PublishSafely(() => _eventPublisher.PublishProjectCreated(project), nameof(_eventPublisher.PublishProjectCreated));

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

        var changes = new Dictionary<string, object>();
        if (dto.NewTitle is not null && dto.NewTitle != project.Title)
        {
            changes["title"] = new { oldValue = project.Title, newValue = dto.NewTitle };
            project.Title = dto.NewTitle;
        }
        if (dto.NewDescription is not null && dto.NewDescription != project.Description)
        {
            changes["description"] = new { oldValue = project.Description, newValue = dto.NewDescription };
            project.Description = dto.NewDescription;
        }

        if (changes.Count > 0)
        {
            await _context.SaveChangesAsync();
            PublishSafely(() => _eventPublisher.PublishProjectUpdated(project, changes), nameof(_eventPublisher.PublishProjectUpdated));
        }

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, string userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

        if (project is null) return false;

        _context.Projects.Remove(project);
        var affectedRows = await _context.SaveChangesAsync();

        if (affectedRows > 0)
        {
            PublishSafely(() => _eventPublisher.PublishProjectDeleted(projectId, userId), nameof(_eventPublisher.PublishProjectDeleted));
        }

        return affectedRows > 0;
    }


    public async Task<TaskItemDto?> CreateTaskAsync(CreateTaskDto dto, string userId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.OwnerId == userId);

        if (project is null) return null;

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

        PublishSafely(() => _eventPublisher.PublishTaskCreated(taskItem, project), nameof(_eventPublisher.PublishTaskCreated));

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

        var changes = new Dictionary<string, object>();
        if (dto.Title is not null && dto.Title != taskItem.Title)
        {
            var oldValue = taskItem.Title;
            taskItem.Title = dto.Title;
            changes["title"] = new { oldValue, newValue = dto.Title };
        }
        if (dto.Description != taskItem.Description)
        {
            var oldValue = taskItem.Description;
            taskItem.Description = dto.Description;
            changes["description"] = new { oldValue, newValue = dto.Description };
        }
        if (dto.Status.HasValue && dto.Status.Value != taskItem.Status)
        {
            var oldValue = taskItem.Status;
            taskItem.Status = dto.Status.Value;
            changes["status"] = new { oldValue = oldValue.ToString(), newValue = dto.Status.Value.ToString() };
        }
        if (dto.DueDate.HasValue && dto.DueDate.Value != taskItem.DueDate)
        {
            var oldValue = taskItem.DueDate;
            taskItem.DueDate = dto.DueDate.Value;
            changes["dueDate"] = new { oldValue, newValue = dto.DueDate.Value };
        }

        if (changes.Count > 0)
        {
            await _context.SaveChangesAsync();
            PublishSafely(() => _eventPublisher.PublishTaskUpdated(taskItem, changes), nameof(_eventPublisher.PublishTaskUpdated));
        }
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

        if (affectedRows > 0)
        {
            PublishSafely(() => _eventPublisher.PublishTaskDeleted(taskItem), nameof(_eventPublisher.PublishTaskDeleted));
        }

        return affectedRows > 0;
    }


    private void PublishSafely(Action publishAction, string eventName)
    {
        try
        {
            publishAction();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось опубликовать событие {EventName}", eventName);
        }
    }
}
