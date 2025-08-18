namespace TaskStream.Users.Application.DTOs;

public record AuthResultDto(
    bool Succeeded,
    string AccessToken = "",
    string RefreshToken = "",
    IEnumerable<string>? Errors = null);