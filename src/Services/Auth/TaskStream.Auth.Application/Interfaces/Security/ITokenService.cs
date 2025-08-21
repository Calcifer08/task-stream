using System.Security.Claims;
using TaskStream.Auth.Domain.Entities;

namespace TaskStream.Auth.Application.Interfaces.Security;

public interface ITokenService
{
    string CreateAccessToken(ApplicationUser user);
    (string, int) CreateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}