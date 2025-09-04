using System.Text.Json;
using TaskStream.Shared.Messaging.Interfaces;
using TaskStream.Tasks.Application.Interfaces;
using TaskStream.Tasks.Domain.Entities;

namespace TaskStream.Tasks.Infrastructure.Services;

public class TasksEventPublisher : ITasksEventPublisher
{
    private readonly IMessageBusClient _messageBus;
    private readonly string _producer = "Tasks.API";

    public TasksEventPublisher(IMessageBusClient messageBus)
    {
        _messageBus = messageBus;
    }

    public void PublishProjectCreated(Project project)
    {
        var payload = new
        {
            ProjectId = project.Id,
            Title = project.Title,
            Description = project.Description,
            UserId = project.OwnerId,
        };
        Publish(payload, "tasks.project.created");
    }

    public void PublishProjectUpdated(Project project, Dictionary<string, object> changes)
    {
        var payload = new
        {
            ProjectId = project.Id,
            UserId = project.OwnerId,
            Changes = changes
        };
        Publish(payload, "tasks.project.updated");
    }

    public void PublishProjectDeleted(Guid projectId, string ownerId)
    {
        var payload = new { ProjectId = projectId, UserId = ownerId };
        Publish(payload, "tasks.project.deleted");
    }

    public void PublishTaskCreated(TaskItem task, Project project)
    {
        var payload = new
        {
            TaskId = task.Id,
            Title = task.Title,
            ProjectId = task.ProjectId,
            UserId = project.OwnerId
        };
        Publish(payload, "tasks.task.created");
    }

    public void PublishTaskUpdated(TaskItem task, Dictionary<string, object> changes)
    {
        var payload = new
        {
            TaskId = task.Id,
            ProjectId = task.ProjectId,
            UserId = task.Project.OwnerId,
            Changes = changes
        };
        Publish(payload, "tasks.task.updated");
    }

    public void PublishTaskDeleted(TaskItem task)
    {
        var payload = new
        {
            TaskId = task.Id,
            ProjectId = task.ProjectId,
            UserId = task.Project.OwnerId
        };
        Publish(payload, "tasks.task.deleted");
    }

    private void Publish(object payload, string routingKey)
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        _messageBus.Publish(
            producer: _producer,
            payloadJson: payloadJson,
            routingKey: routingKey
        );
    }
}