using FluentValidation;
using TaskStream.Tasks.API.GrpcServices;
using TaskStream.Tasks.Application.Validators;
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

      var app = builder.Build();

      app.MapGrpcService<TasksGrpcService>();

      app.MapGet("/", () => "Это сервис TaskStream.Tasks.API, использует gRPC");

      app.Run();
    }
  }
}