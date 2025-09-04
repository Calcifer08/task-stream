using FluentValidation;
using TaskStream.Auth.API.GrpcServices;
using TaskStream.Auth.Application.Validators;
using TaskStream.Auth.Infrastructure.Extensions;

namespace TaskStream.Auth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddInfrastructureServices(builder.Configuration);

            builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

            builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

            builder.Services.AddGrpc();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.MapGrpcService<AuthGrpcService>();

            app.MapGet("/", () => "Это сервис TaskStream.Auth.API, использует gRPC");

            app.Run();
        }
    }
}
