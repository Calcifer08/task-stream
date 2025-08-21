using System.Text.Json.Serialization;

namespace TaskStream.ApiGateway.Models.Auth.Requests;

public record AuthSuccessResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }
}