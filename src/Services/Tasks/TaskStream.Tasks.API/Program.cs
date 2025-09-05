using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using TaskStream.Tasks.API.GrpcServices;
using TaskStream.Tasks.Application.Validators;
using TaskStream.Tasks.Infrastructure.Data;
using TaskStream.Tasks.Infrastructure.Extensions;

namespace TaskStream.Tasks.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddValidatorsFromAssemblyContaining<CreateProjectDtoValidator>();

            builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

            builder.Services.AddGrpc();

            var isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            builder.WebHost.ConfigureKestrel(options =>
            {
                if (isRunningInContainer)
                {
                    options.Listen(IPAddress.Any, 80, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                }
            });

            var app = builder.Build();

            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

                    dbContext.Database.Migrate();
                }
                Console.WriteLine("Миграции для Auth.API успешно применены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при применении миграций для Auth.API: {ex.Message}");
            }

            app.MapGrpcService<TasksGrpcService>();

            app.MapGet("/", () => "Это сервис TaskStream.Tasks.API, использует gRPC");

            app.Run();
        }
    }
}