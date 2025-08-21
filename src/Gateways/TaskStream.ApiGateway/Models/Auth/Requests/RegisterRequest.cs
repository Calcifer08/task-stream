using System.ComponentModel.DataAnnotations;

namespace TaskStream.ApiGateway.Models.Auth.Requests;

public record RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }
    [Required]
    public required string Password { get; init; }
}