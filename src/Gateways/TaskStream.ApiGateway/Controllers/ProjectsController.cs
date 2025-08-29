using System.Security.Claims;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskStream.ApiGateway.Models.Tasks.Requests;
using TaskStream.ApiGateway.Models.Tasks.Responses;
using TasksProtos = TaskStream.Shared.Protos.Tasks;

namespace TaskStream.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly TasksProtos.TasksService.TasksServiceClient _tasksClient;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksController> _logger;

    public ProjectsController(TasksProtos.TasksService.TasksServiceClient tasksClient, IMapper mapper, ILogger<TasksController> logger)
    {
        _tasksClient = tasksClient;
        _mapper = mapper;
        _logger = logger;
    }

    private IActionResult HandleRpcException(RpcException ex)
    {
        switch (ex.StatusCode)
        {
            case Grpc.Core.StatusCode.InvalidArgument:
                return BadRequest(new { Error = "Некорректные данные в запросе.", Details = ex.Status.Detail });
            case Grpc.Core.StatusCode.NotFound:
                return NotFound(new { Error = "Ресурс не найден.", Details = ex.Status.Detail });
            case Grpc.Core.StatusCode.Unauthenticated:
                return Unauthorized(new { Error = "Ошибка аутентификации.", Details = ex.Status.Detail });
            case Grpc.Core.StatusCode.PermissionDenied:
                return Forbid();
            default:
                return StatusCode(500, new { Error = "Произошла внутренняя ошибка сервера.", Details = ex.Status.Detail });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProjectAsync([FromBody] CreateProjectRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = _mapper.Map<TasksProtos.CreateProjectRequest>(request);

        try
        {
            var response = await _tasksClient.CreateProjectAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<ProjectResponse>(response));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectsAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };

        try
        {
            var response = await _tasksClient.GetProjectsAsync(new TasksProtos.GetProjectsRequest(), metadata);
            return Ok(_mapper.Map<IEnumerable<ProjectResponse>>(response.Projects));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> GetProjectByIdAsync(Guid projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = new TasksProtos.GetProjectByIdRequest { ProjectId = projectId.ToString() };

        try
        {
            var response = await _tasksClient.GetProjectByIdAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<ProjectResponse>(response));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpPut("{projectId:guid}")]
    public async Task<IActionResult> UpdateProjectAsync(Guid projectId, [FromBody] UpdateProjectRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = _mapper.Map<TasksProtos.UpdateProjectRequest>(request);
        grpcRequest.ProjectId = projectId.ToString();

        try
        {
            var response = await _tasksClient.UpdateProjectAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<ProjectResponse>(response));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> DeleteProjectAsync(Guid projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = new TasksProtos.DeleteProjectRequest { ProjectId = projectId.ToString() };

        try
        {
            await _tasksClient.DeleteProjectAsync(grpcRequest, metadata);
            return NoContent();
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpPost("{projectId:guid}/tasks")]
    public async Task<IActionResult> CreateTaskAsync(Guid projectId, [FromBody] CreateTaskRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = _mapper.Map<TasksProtos.CreateTaskRequest>(request);
        grpcRequest.ProjectId = projectId.ToString();

        try
        {
            var response = await _tasksClient.CreateTaskAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<TaskItemResponse>(response));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpGet("{projectId:guid}/tasks")]
    public async Task<IActionResult> GetTasksInProjectAsync(Guid projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = new TasksProtos.GetTasksInProjectRequest { ProjectId = projectId.ToString() };

        try
        {
            var response = await _tasksClient.GetTasksInProjectAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<IEnumerable<TaskItemResponse>>(response.Tasks));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }
}