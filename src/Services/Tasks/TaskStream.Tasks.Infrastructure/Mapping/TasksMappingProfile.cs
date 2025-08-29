using AutoMapper;
using TaskStream.Tasks.Application.DTOs;
using TaskStream.Tasks.Domain.Entities;

namespace TaskStream.Tasks.Infrastructure.Mapping;

public class TasksMappingProfile : Profile
{
    public TasksMappingProfile()
    {
        CreateMap<Project, ProjectDto>();
        CreateMap<TaskItem, TaskItemDto>();
    }
}