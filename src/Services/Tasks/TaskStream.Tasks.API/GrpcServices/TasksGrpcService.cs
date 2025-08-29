using AutoMapper;
using FluentValidation;
using Grpc.Core;
using TaskStream.Shared.Protos.Tasks;
using TaskStream.Tasks.Application.DTOs;
using TaskStream.Tasks.Application.Interfaces;

namespace TaskStream.Tasks.API.GrpcServices;

public class TasksGrpcService : TasksService.TasksServiceBase
{
    private readonly ITasksService _tasksService;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksGrpcService> _logger;
    private readonly IValidator<CreateProjectDto> _createProjectValidator;
    private readonly IValidator<UpdateProjectDto> _updateProjectValidator;
    private readonly IValidator<CreateTaskDto> _createTaskValidator;
    private readonly IValidator<UpdateTaskDto> _updateTaskValidator;


    public TasksGrpcService(
        ITasksService tasksService,
        IMapper mapper,
        ILogger<TasksGrpcService> logger,
        IValidator<CreateProjectDto> createProjectValidator,
        IValidator<UpdateProjectDto> updateProjectValidator,
        IValidator<CreateTaskDto> createTaskValidator,
        IValidator<UpdateTaskDto> updateTaskValidator)
    {
        _tasksService = tasksService;
        _mapper = mapper;
        _logger = logger;
        _createProjectValidator = createProjectValidator;
        _updateProjectValidator = updateProjectValidator;
        _createTaskValidator = createTaskValidator;
        _updateTaskValidator = updateTaskValidator;
    }

    private string GetUserId(ServerCallContext context)
    {
        var userId = context.RequestHeaders.GetValue("x-user-id");
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Неверный запрос: отсутствуют метаданные 'x-user-id'.");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Метаданные 'x-user-id' обязательны."));
        }
        return userId;
    }

    public override async Task<ProjectResponse> CreateProject(CreateProjectRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var dto = _mapper.Map<CreateProjectDto>(request);

        var validationResult = await _createProjectValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.ToString()));
        }

        var resultDto = await _tasksService.CreateProjectAsync(dto, userId);
        return _mapper.Map<ProjectResponse>(resultDto);
    }

    public override async Task<GetProjectsResponse> GetProjects(GetProjectsRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var resultDtos = await _tasksService.GetProjectsAsync(userId);

        var response = new GetProjectsResponse();
        response.Projects.AddRange(_mapper.Map<IEnumerable<ProjectResponse>>(resultDtos));
        return response;
    }

    public override async Task<ProjectResponse> GetProjectById(GetProjectByIdRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var projectId = Guid.Parse(request.ProjectId);
        var projectDto = await _tasksService.GetProjectByIdAsync(projectId, userId);

        if (projectDto is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Проект с идентификатором {request.ProjectId} не найден или в доступе отказано."));
        }

        return _mapper.Map<ProjectResponse>(projectDto);
    }

    public override async Task<ProjectResponse> UpdateProject(UpdateProjectRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var dto = _mapper.Map<UpdateProjectDto>(request);

        var validationResult = await _updateProjectValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.ToString()));
        }

        var updatedProjectDto = await _tasksService.UpdateProjectAsync(dto, userId);
        if (updatedProjectDto is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Проект с идентификатором {request.ProjectId} не найден или в доступе отказано."));
        }

        return _mapper.Map<ProjectResponse>(updatedProjectDto);
    }

    public override async Task<DeleteProjectResponse> DeleteProject(DeleteProjectRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var projectId = Guid.Parse(request.ProjectId);
        var success = await _tasksService.DeleteProjectAsync(projectId, userId);

        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Проект с идентификатором {request.ProjectId} не найден или в доступе отказано."));
        }

        return new DeleteProjectResponse { Succeeded = true };
    }



    public override async Task<TaskItemResponse> CreateTask(CreateTaskRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var dto = _mapper.Map<CreateTaskDto>(request);

        var validationResult = await _createTaskValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.ToString()));
        }

        var resultDto = await _tasksService.CreateTaskAsync(dto, userId);
        if (resultDto is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Проект с идентификатором {request.ProjectId} не найден или в доступе отказано."));
        }

        return _mapper.Map<TaskItemResponse>(resultDto);
    }

    public override async Task<GetTasksInProjectResponse> GetTasksInProject(GetTasksInProjectRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var projectId = Guid.Parse(request.ProjectId);
        var resultDtos = await _tasksService.GetTasksInProjectAsync(projectId, userId);

        var response = new GetTasksInProjectResponse();
        response.Tasks.AddRange(_mapper.Map<IEnumerable<TaskItemResponse>>(resultDtos));
        return response;
    }

    public override async Task<TaskItemResponse> GetTaskById(GetTaskByIdRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var taskId = Guid.Parse(request.TaskId);
        var taskDto = await _tasksService.GetTaskByIdAsync(taskId, userId);

        if (taskDto is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Задача с идентификатором {request.TaskId} не найдена или в доступе отказано."));
        }

        return _mapper.Map<TaskItemResponse>(taskDto);
    }

    public override async Task<TaskItemResponse> UpdateTask(UpdateTaskRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var dto = _mapper.Map<UpdateTaskDto>(request);

        var validationResult = await _updateTaskValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.ToString()));
        }

        var updatedTaskDto = await _tasksService.UpdateTaskAsync(dto, userId);
        if (updatedTaskDto is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Задача с идентификатором {request.TaskId} не найдена или в доступе отказано."));
        }

        return _mapper.Map<TaskItemResponse>(updatedTaskDto);
    }

    public override async Task<DeleteTaskResponse> DeleteTask(DeleteTaskRequest request, ServerCallContext context)
    {
        var userId = GetUserId(context);
        var taskId = Guid.Parse(request.TaskId);
        var success = await _tasksService.DeleteTaskAsync(taskId, userId);

        if (!success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Задача с идентификатором {request.TaskId} не найдена или в доступе отказано."));
        }

        return new DeleteTaskResponse { Succeeded = true };
    }
}