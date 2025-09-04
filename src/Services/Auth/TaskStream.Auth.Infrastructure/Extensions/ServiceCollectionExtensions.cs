using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TaskStream.Auth.Application.Interfaces.Publishers;
using TaskStream.Auth.Application.Interfaces.Security;
using TaskStream.Auth.Application.Interfaces.Services;
using TaskStream.Auth.Domain.Entities;
using TaskStream.Auth.Infrastructure.Data;
using TaskStream.Auth.Infrastructure.Services;
using TaskStream.Shared.Messaging;
using TaskStream.Shared.Messaging.Interfaces;

namespace TaskStream.Auth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(configuration.GetRequired("ConnectionStrings:Postgres")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<UsersDbContext>()
        .AddDefaultTokenProviders();

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration.GetRequired("ConnectionStrings:Redis")));

        services.AddSingleton<IMessageBusClient, RabbitMQClient>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddScoped<IAuthEventPublisher, AuthEventPublisher>();

        return services;
    }
}