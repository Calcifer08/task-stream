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
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        _logger.LogInformation("Регистрация пользователя {Email} через Гетвей", request.Email);
        var grpcRequest = _mapper.Map<GrpcContracts.RegisterRequest>(request);
        var result = await _authClient.RegisterAsync(grpcRequest);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.ToList() });
        }

        return Ok(_mapper.Map<AuthSuccessResponse>(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        _logger.LogInformation("Вход пользователя {Email} через Гетвей", request.Email);
        var grpcRequest = _mapper.Map<GrpcContracts.LoginRequest>(request);
        var result = await _authClient.LoginAsync(grpcRequest);

        if (!result.Succeeded)
        {
            return Unauthorized(new { Errors = result.Errors.ToList() });
        }

        return Ok(_mapper.Map<AuthSuccessResponse>(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRefreshRequest request)
    {
        _logger.LogInformation("Обновление токенов через Гетвей");
        var grpcRequest = _mapper.Map<GrpcContracts.RefreshRequest>(request);
        var result = await _authClient.RefreshAsync(grpcRequest);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.ToList() });
        }

        return Ok(_mapper.Map<AuthSuccessResponse>(result));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return BadRequest("UserID не найден в токене");
        }

        _logger.LogInformation("Выход пользователя {Email} через Гетвей", userId);

        var grpcRequest = new GrpcContracts.LogoutRequest();
        var metadata = new Metadata { { "x-user-id", userId } };

        var result = await _authClient.LogoutAsync(grpcRequest, metadata);

        if (!result.Succeeded)
        {
            return StatusCode(500, "Ошибка выхода на сервере");
        }

        return Ok();
    }
}