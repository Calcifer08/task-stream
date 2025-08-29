using AutoMapper;
using FluentValidation;
using Grpc.Core;
using TaskStream.Shared.Protos.Auth;
using TaskStream.Auth.Application.DTOs;
using TaskStream.Auth.Application.Interfaces.Services;

namespace TaskStream.Auth.API.GrpcServices;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthGrpcService> _logger;
    private readonly IValidator<RegisterDto> _registerValidator;

    public AuthGrpcService(IAuthService authService, IMapper mapper, ILogger<AuthGrpcService> logger, IValidator<RegisterDto> registerValidator)
    {
        _authService = authService;
        _mapper = mapper;
        _logger = logger;
        _registerValidator = registerValidator;
    }

    public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Запрос на регистрацию по gRPC для {Email}", request.Email);
        var dto = _mapper.Map<RegisterDto>(request);

        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, validationResult.ToString()));
        }

        var result = await _authService.RegisterAsync(dto);
        if (!result.Succeeded)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", result.Errors!)));
        }

        return _mapper.Map<AuthResponse>(result);
    }

    public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Запрос на вход по gRPC для {Email}", request.Email);
        var dto = _mapper.Map<LoginDto>(request);
        var result = await _authService.LoginAsync(dto);

        if (!result.Succeeded)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Неверные учетные данные."));
        }
        return _mapper.Map<AuthResponse>(result);
    }

    public override async Task<AuthResponse> Refresh(RefreshRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Запрос на обновление токена обновления по gRPC");
        var dto = _mapper.Map<RefreshTokenDto>(request);
        var result = await _authService.RefreshTokenAsync(dto);
        if (!result.Succeeded)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(", ", result.Errors!)));
        }

        return _mapper.Map<AuthResponse>(result);
    }

    public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context)
    {
        var userId = context.RequestHeaders.GetValue("x-user-id");

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Ошибка выхода: отсутствует метаданные 'x-user-id'.");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Метаданные 'x-user-id' обязательны."));
        }

        _logger.LogInformation("Запрос на выход по gRPC для User {UserId}", userId);

        var succeeded = await _authService.LogoutAsync(userId);
        if (!succeeded)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Не удалось выполнить выход из системы."));
        }

        return new LogoutResponse { Succeeded = true };
    }
}