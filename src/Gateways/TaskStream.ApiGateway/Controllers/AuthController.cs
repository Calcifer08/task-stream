using System.Security.Claims;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskStream.ApiGateway.Models.Auth.Requests;
using GrpcContracts = TaskStream.Shared.Protos.Auth;

namespace TaskStream.ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GrpcContracts.AuthService.AuthServiceClient _authClient;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;

    public AuthController(GrpcContracts.AuthService.AuthServiceClient authClient, IMapper mapper, ILogger<AuthController> logger)
    {
        _authClient = authClient;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Регистрация пользователя {Email} через Гетвей", request.Email);
        try
        {
            var grpcRequest = _mapper.Map<GrpcContracts.RegisterRequest>(request);
            var result = await _authClient.RegisterAsync(grpcRequest);
            return Ok(_mapper.Map<AuthSuccessResponse>(result));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Вход пользователя {Email} через Гетвей", request.Email);

        try
        {
            var grpcRequest = _mapper.Map<GrpcContracts.LoginRequest>(request);
            var result = await _authClient.LoginAsync(grpcRequest);
            return Ok(_mapper.Map<AuthSuccessResponse>(result));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(TokenRefreshRequest request)
    {
        _logger.LogInformation("Обновление токенов через Гетвей");

        try
        {
            var grpcRequest = _mapper.Map<GrpcContracts.RefreshRequest>(request);
            var result = await _authClient.RefreshAsync(grpcRequest);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.ToList() });
            }

            return Ok(_mapper.Map<AuthSuccessResponse>(result));
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return BadRequest("UserID не найден в токене");
        }

        _logger.LogInformation("Выход пользователя {Email} через Гетвей", userId);

        var grpcRequest = new GrpcContracts.LogoutRequest();
        var metadata = new Metadata { { "x-user-id", userId } };

        try
        {
            var result = await _authClient.LogoutAsync(grpcRequest, metadata);

            if (!result.Succeeded)
            {
                return StatusCode(500, "Ошибка выхода на сервере");
            }

            return Ok();
        }
        catch (RpcException ex)
        {
            return HandleRpcException(ex);
        }
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
}