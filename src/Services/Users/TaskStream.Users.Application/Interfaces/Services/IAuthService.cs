using TaskStream.Users.Application.DTOs;

namespace TaskStream.Users.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResultDto> LoginAsync(LoginDto loginDto);
    Task<AuthResultDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task LogoutAsync(string userId);
}