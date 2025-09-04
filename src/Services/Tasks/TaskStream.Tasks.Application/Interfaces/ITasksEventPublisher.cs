using TaskStream.Tasks.Domain.Entities;

namespace TaskStream.Tasks.Application.Interfaces;

public interface ITasksEventPublisher
{
    void PublishProjectCreated(Project project);
    void PublishProjectUpdated(Project project, Dictionary<string, object> changes);
    void PublishProjectDeleted(Guid projectId, string ownerId);

    void PublishTaskCreated(TaskItem task, Project project);
    void PublishTaskUpdated(TaskItem task, Dictionary<string, object> changes);
    void PublishTaskDeleted(TaskItem task);
}