using TaskStream.Auth.Domain.Entities;

namespace TaskStream.Auth.Application.Interfaces.Publishers;

public interface IAuthEventPublisher
{
    void PublishUserRegistered(ApplicationUser user);
}