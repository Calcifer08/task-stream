using System.Text.Json;
using TaskStream.Auth.Application.Interfaces.Publishers;
using TaskStream.Auth.Domain.Entities;
using TaskStream.Shared.Messaging.Interfaces;

namespace TaskStream.Auth.Infrastructure.Services;

public class AuthEventPublisher : IAuthEventPublisher
{
    private readonly IMessageBusClient _messageBus;

    public AuthEventPublisher(IMessageBusClient messageBus)
    {
        _messageBus = messageBus;
    }

    public void PublishUserRegistered(ApplicationUser user)
    {
        var payload = new
        {
            UserId = user.Id,
            Email = user.Email
        };

        var payloadJson = JsonSerializer.Serialize(payload);

        _messageBus.Publish(
            producer: "Auth.API",
            payloadJson: payloadJson,
            routingKey: "auth.user.registered"
        );
    }
}