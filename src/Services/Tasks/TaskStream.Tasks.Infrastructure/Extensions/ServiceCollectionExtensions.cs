using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskStream.Shared.Messaging;
using TaskStream.Shared.Messaging.Interfaces;
using TaskStream.Tasks.API.Extensions;
using TaskStream.Tasks.Application.Interfaces;
using TaskStream.Tasks.Infrastructure.Data;
using TaskStream.Tasks.Infrastructure.Mapping;
using TaskStream.Tasks.Infrastructure.Services;

namespace TaskStream.Tasks.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TasksDbContext>(options =>
            options.UseNpgsql(configuration.GetRequired("ConnectionStrings:Postgres"), npgsqlOptions =>
            {
                npgsqlOptions.MapEnum<Domain.Entities.TaskItemStatus>();
            }));

        services.AddSingleton<IMessageBusClient, RabbitMQClient>();

        services.AddScoped<ITasksService, TasksService>();
        services.AddScoped<ITasksEventPublisher, TasksEventPublisher>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<TasksMappingProfile>();
        });

        return services;
    }
}