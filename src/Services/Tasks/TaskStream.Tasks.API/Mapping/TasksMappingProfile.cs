using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using TaskStream.Shared.Protos.Tasks;
using TaskStream.Tasks.Application.DTOs;
using TaskStream.Tasks.Domain.Entities;

namespace TaskStream.Tasks.API.Mapping;

public class TasksMappingProfile : Profile
{
    public TasksMappingProfile()
    {
        CreateMap<Timestamp, DateTime?>()
            .ConvertUsing(src => src == null ? (DateTime?)null : src.ToDateTime());
        CreateMap<DateTime?, Timestamp>().
            ConvertUsing(src => src.HasValue ? Timestamp.FromDateTime(src.Value.ToUniversalTime()) : null!);

        // gRPC -> DTO
        CreateMap<CreateProjectRequest, CreateProjectDto>()
            .ConvertUsing(src => new CreateProjectDto(
                src.Title,
                src.HasDescription ? src.Description : null
            ));

        CreateMap<UpdateProjectRequest, UpdateProjectDto>()
            .ConvertUsing(src => new UpdateProjectDto(
                Guid.Parse(src.ProjectId),
                src.HasNewTitle ? src.NewTitle : null,
                src.HasNewDescription ? src.NewDescription : null
            ));

        CreateMap<CreateTaskRequest, CreateTaskDto>()
            .ConvertUsing(src => new CreateTaskDto(
                Guid.Parse(src.ProjectId),
                src.Title,
                src.HasDescription ? src.Description : null,
                src.DueDate != null ? src.DueDate.ToDateTime() : (DateTime?)null
            ));

        CreateMap<UpdateTaskRequest, UpdateTaskDto>()
            .ConvertUsing(src => new UpdateTaskDto(
                Guid.Parse(src.TaskId),
                src.HasTitle ? src.Title : null,
                src.HasDescription ? src.Description : null,
                src.HasStatus ? (TaskItemStatus?)src.Status : null,
                src.DueDate != null ? src.DueDate.ToDateTime() : (DateTime?)null
            ));


        // DTO -> gRPC
        CreateMap<ProjectDto, ProjectResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Description, opt =>
            {
                opt.Condition(src => src.Description is not null);
                opt.MapFrom(src => src.Description);
            });


        CreateMap<TaskItemDto, TaskItemResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Shared.Protos.Tasks.TaskStatus)src.Status))
            .ForMember(dest => dest.Description, opt =>
            {
                opt.Condition(src => src.Description is not null);
                opt.MapFrom(src => src.Description);
            })
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate));
    }
}