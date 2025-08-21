using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskStream.ApiGateway.Models.Auth.Requests;

public record TokenRefreshRequest
{
    [Required]
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }
}