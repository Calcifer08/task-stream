using System.Security.Claims;
using TaskStream.Users.Domain.Entities;

namespace TaskStream.Users.Application.Interfaces.Security;

public interface ITokenService
{
    string CreateAccessToken(ApplicationUser user);
    (string, int) CreateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}