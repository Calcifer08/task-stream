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
public class TasksController : ControllerBase
{
    private readonly TasksProtos.TasksService.TasksServiceClient _tasksClient;
    private readonly IMapper _mapper;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TasksProtos.TasksService.TasksServiceClient tasksClient, IMapper mapper, ILogger<TasksController> logger)
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

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTaskByIdAsync(Guid taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = new TasksProtos.GetTaskByIdRequest { TaskId = taskId.ToString() };

        try
        {
            var response = await _tasksClient.GetTaskByIdAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<TaskItemResponse>(response));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> UpdateTaskAsync(Guid taskId, [FromBody] UpdateTaskRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };

        var grpcRequest = _mapper.Map<TasksProtos.UpdateTaskRequest>(request);
        grpcRequest.TaskId = taskId.ToString();

        try
        {
            var response = await _tasksClient.UpdateTaskAsync(grpcRequest, metadata);
            return Ok(_mapper.Map<TaskItemResponse>(response));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTaskAsync(Guid taskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized("UserID не найден в токене");
        var metadata = new Metadata { { "x-user-id", userId } };
        var grpcRequest = new TasksProtos.DeleteTaskRequest { TaskId = taskId.ToString() };

        try
        {
            await _tasksClient.DeleteTaskAsync(grpcRequest, metadata);
            return NoContent();
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }
}