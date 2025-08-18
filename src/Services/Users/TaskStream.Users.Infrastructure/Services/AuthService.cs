using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using TaskStream.Users.Application.DTOs;
using TaskStream.Users.Application.Interfaces.Security;
using TaskStream.Users.Application.Interfaces.Services;
using TaskStream.Users.Domain.Entities;

namespace TaskStream.Users.Infrastructure.Services;

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

        var accessToken = _tokenService.CreateAccessToken(user);
        (string refreshToken, int refreshTokenLifetime) = _tokenService.CreateRefreshToken();

        await _redisDb.StringSetAsync($"refreshToken:{user.Id}", refreshToken, TimeSpan.FromDays(refreshTokenLifetime));

        return new AuthResultDto(true, accessToken, refreshToken);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return new AuthResultDto(false, Errors: ["Недопустимый токен доступа: не удалось определить пользователя."]);
        }

        var savedRefreshToken = await _redisDb.StringGetAsync($"refreshToken:{userId}");

        if (savedRefreshToken.IsNullOrEmpty || savedRefreshToken != refreshTokenDto.RefreshToken)
        {
            return new AuthResultDto(false, Errors: ["Недопустимый токен обновления"]);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResultDto(false, Errors: ["Пользователь не найден"]);
        }

        var newAccessToken = _tokenService.CreateAccessToken(user);
        (string newRefreshToken, int refreshTokenLifetime) = _tokenService.CreateRefreshToken();

        await _redisDb.StringSetAsync($"refreshToken:{user.Id}", newRefreshToken, TimeSpan.FromDays(refreshTokenLifetime));

        return new AuthResultDto(true, newAccessToken, newRefreshToken);
    }

    public async Task LogoutAsync(string userId)
    {
        await _redisDb.KeyDeleteAsync($"refreshToken:{userId}");
    }
}