using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TaskStream.Auth.API.GrpcServices;
using TaskStream.Auth.Application.Interfaces.Security;
using TaskStream.Auth.Application.Interfaces.Services;
using TaskStream.Auth.Application.Validators;
using TaskStream.Auth.Domain.Entities;
using TaskStream.Auth.Infrastructure.Data;
using TaskStream.Auth.Infrastructure.Extensions;
using TaskStream.Auth.Infrastructure.Services;

namespace TaskStream.Auth.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<UsersDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetRequired("ConnectionStrings:Postgres"));
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(builder.Configuration.GetRequired("ConnectionStrings:Redis")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

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
