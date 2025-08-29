using TaskStream.Auth.Application.DTOs;

namespace TaskStream.Auth.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResultDto> LoginAsync(LoginDto loginDto);
    Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> LogoutAsync(string userId);
}