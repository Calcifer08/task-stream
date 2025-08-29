using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using TaskStream.ApiGateway.Models.Tasks.Requests;
using TaskStream.ApiGateway.Models.Tasks.Responses;
using TasksProtos = TaskStream.Shared.Protos.Tasks;

namespace TaskStream.ApiGateway.Mapping;

public class TasksAndProjectsMappingProfile : Profile
{
    public TasksAndProjectsMappingProfile()
    {
        CreateMap<Timestamp, DateTime?>()
            .ConvertUsing(src => src != null ? src.ToDateTime() : (DateTime?)null);
        CreateMap<DateTime?, Timestamp>()
            .ConvertUsing(src => src.HasValue ? Timestamp.FromDateTime(src.Value.ToUniversalTime()) : null!);

        // REST -> gRPC
        CreateMap<CreateProjectRequest, TasksProtos.CreateProjectRequest>()
            .ForMember(dest => dest.Description, opt =>
            {
                opt.Condition(src => src.Description is not null);
                opt.MapFrom(src => src.Description);
            });

        CreateMap<CreateTaskRequest, TasksProtos.CreateTaskRequest>()
            .ForMember(dest => dest.Description, opt =>
            {
                opt.Condition(src => src.Description is not null);
                opt.MapFrom(src => src.Description);
            })
            .ForMember(dest => dest.DueDate, opt =>
            {
                opt.Condition(src => src.DueDate.HasValue);
                opt.MapFrom(src => src.DueDate);
            });

        CreateMap<UpdateProjectRequest, TasksProtos.UpdateProjectRequest>()
            .ForMember(dest => dest.NewTitle, opt =>
            {
                opt.Condition(src => src.Title is not null);
                opt.MapFrom(src => src.Title);
            })
            .ForMember(dest => dest.NewDescription, opt =>
            {
                opt.Condition(src => src.Description is not null);
                opt.MapFrom(src => src.Description);
            });

        CreateMap<UpdateTaskRequest, TasksProtos.UpdateTaskRequest>()
            .ForMember(dest => dest.Title, opt =>
            {
                opt.Condition(src => src.Title is not null);
                opt.MapFrom(src => src.Title);
            })
            .ForMember(dest => dest.Description, opt =>
            {
                opt.Condition(src => src.Description is not null);
                opt.MapFrom(src => src.Description);
            })
            .ForMember(dest => dest.Status, opt =>
            {
                opt.Condition(src => !string.IsNullOrEmpty(src.Status));
                opt.MapFrom(src => ConvertStatus(src.Status));
            })
            .ForMember(dest => dest.DueDate, opt =>
            {
                opt.Condition(src => src.DueDate.HasValue);
                opt.MapFrom(src => src.DueDate);
            });


        // gRPC -> REST
        CreateMap<TasksProtos.ProjectResponse, ProjectResponse>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.HasDescription ? src.Description : null));

        CreateMap<TasksProtos.TaskItemResponse, TaskItemResponse>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.HasDescription ? src.Description : null))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TaskStatus)src.Status));
    }

    private static TasksProtos.TaskStatus? ConvertStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return null;

        return status.ToUpperInvariant() switch
        {
            "TO_DO" => TasksProtos.TaskStatus.ToDo,
            "IN_PROGRESS" => TasksProtos.TaskStatus.InProgress,
            "DONE" => TasksProtos.TaskStatus.Done,
            _ => throw new AutoMapperMappingException($"Неподдерживаемый статус: '{status}'")
        };
    }
}