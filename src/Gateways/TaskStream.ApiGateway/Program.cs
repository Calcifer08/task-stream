using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskStream.ApiGateway.Extensions;
using TaskStream.ApiGateway.Swagger;
using TaskStream.Shared.Protos.Auth;
using TaskStream.Shared.Protos.Tasks;

namespace TaskStream.ApiGateway
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration.GetRequired("Jwt:Issuer"),
          ValidAudience = builder.Configuration.GetRequired("Jwt:Audience"),
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetRequired("Jwt:Key"))),
          ClockSkew = TimeSpan.Zero
        };
      });

      builder.Services.AddAuthorization();

      builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(options =>
      {
        options.Address = new Uri(builder.Configuration["GrpcServices:UsersApi"]!);
      });
      builder.Services.AddGrpcClient<TasksService.TasksServiceClient>(options =>
      {
        options.Address = new Uri(builder.Configuration["GrpcServices:TasksApi"]!);
      });

      builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
          options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen(options =>
      {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "TasksStream", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          In = ParameterLocation.Header,
          Description = "Пожалуйста, введите JWT токен",
          Name = "Authorization",
          Type = SecuritySchemeType.Http,
          BearerFormat = "JWT",
          Scheme = "bearer"
        });
        options.OperationFilter<SecurityRequirementsOperationFilter>();
      });

      builder.Services.AddCors(options =>
      {
        options.AddDefaultPolicy(policy =>
          {
            policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
          });
      });

      builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

      var app = builder.Build();

      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();
      app.UseCors();
      app.UseAuthentication();
      app.UseAuthorization();
      app.MapControllers();

      app.Run();
    }
  }
}
