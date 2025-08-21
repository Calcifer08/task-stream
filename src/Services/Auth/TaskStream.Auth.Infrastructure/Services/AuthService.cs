using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using TaskStream.Auth.Application.DTOs;
using TaskStream.Auth.Application.Interfaces.Security;
using TaskStream.Auth.Application.Interfaces.Services;
using TaskStream.Auth.Domain.Entities;

namespace TaskStream.Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IDatabase _redisDb;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConnectionMultiplexer redis)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _redisDb = redis.GetDatabase();
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
    {
        var user = new ApplicationUser { Email = registerDto.Email, UserName = registerDto.Email };
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new AuthResultDto(false, Errors: result.Errors.Select(e => e.Description));
        }

        return await LoginAsync(new LoginDto(registerDto.Email, registerDto.Password));
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return new AuthResultDto(false, Errors: ["Неверные учетные данные"]);
        }

        var oldRefreshToken = await _redisDb.StringGetAsync($"user:{user.Id}:refreshToken");
        if (!oldRefreshToken.IsNullOrEmpty)
        {
            await _redisDb.KeyDeleteAsync($"user:{user.Id}:refreshToken");
            await _redisDb.KeyDeleteAsync($"refreshToken:{oldRefreshToken}");
        }

        var accessToken = _tokenService.CreateAccessToken(user);
        (string newRefreshToken, int refreshTokenLifetime) = _tokenService.CreateRefreshToken();

        var expiryTime = TimeSpan.FromDays(refreshTokenLifetime);

        await _redisDb.StringSetAsync($"user:{user.Id}:refreshToken", newRefreshToken, expiryTime);
        await _redisDb.StringSetAsync($"refreshToken:{newRefreshToken}", user.Id, expiryTime);

        return new AuthResultDto(true, accessToken, newRefreshToken);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var userId = await _redisDb.StringGetAsync($"refreshToken:{refreshTokenDto.RefreshToken}");

        if (userId.IsNullOrEmpty)
        {
            return new AuthResultDto(false, Errors: ["Недопустимый или просроченный токен обновления"]);
        }

        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            return new AuthResultDto(false, Errors: ["Пользователь, связанный с токеном, не найден"]);
        }

        await _redisDb.KeyDeleteAsync($"user:{userId}:refreshToken");
        await _redisDb.KeyDeleteAsync($"refreshToken:{refreshTokenDto.RefreshToken}");

        var newAccessToken = _tokenService.CreateAccessToken(user);
        (string newRefreshToken, int refreshTokenLifetime) = _tokenService.CreateRefreshToken();

        var expiryTime = TimeSpan.FromDays(refreshTokenLifetime);
        await _redisDb.StringSetAsync($"user:{user.Id}:refreshToken", newRefreshToken, expiryTime);
        await _redisDb.StringSetAsync($"refreshToken:{newRefreshToken}", user.Id, expiryTime);

        return new AuthResultDto(true, newAccessToken, newRefreshToken);
    }

    public async Task LogoutAsync(string userId)
    {
        var refreshToken = await _redisDb.StringGetAsync($"user:{userId}:refreshToken");

        await _redisDb.KeyDeleteAsync($"user:{userId}:refreshToken");

        if (!refreshToken.IsNullOrEmpty)
        {
            await _redisDb.KeyDeleteAsync($"refreshToken:{refreshToken}");
        }
    }
}