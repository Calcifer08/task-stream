using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskStream.Tasks.API.Extensions;
using TaskStream.Tasks.API.GrpcServices;
using TaskStream.Tasks.Application.Interfaces;
using TaskStream.Tasks.Application.Validators;
using TaskStream.Tasks.Infrastructure.Data;
using TaskStream.Tasks.Infrastructure.Services;

namespace TaskStream.Tasks.API
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddDbContext<TasksDbContext>(options =>
          options.UseNpgsql(builder.Configuration.GetRequired("ConnectionStrings:Postgres"), npgsqlOptions =>
          {
            npgsqlOptions.MapEnum<Domain.Entities.TaskItemStatus>();
          }));

      builder.Services.AddScoped<ITasksService, TasksService>();

      builder.Services.AddValidatorsFromAssemblyContaining<CreateProjectDtoValidator>();

      builder.Services.AddAutoMapper(cfg =>
      {
        cfg.AddProfile<API.Mapping.TasksMappingProfile>();
        cfg.AddProfile<Infrastructure.Mapping.TasksMappingProfile>();
      });

      builder.Services.AddGrpc();

      var app = builder.Build();

      app.MapGrpcService<TasksGrpcService>();

      app.MapGet("/", () => "Это сервис TaskStream.Tasks.API, использует gRPC");

      app.Run();
    }
  }
}