using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
      // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

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
        options.Address = new Uri(builder.Configuration["GrpcServices:AuthApi"]!);
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

      var isRunningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

      if (isRunningInContainer)
      {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
          ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
      }

      if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      if (!isRunningInContainer)
      {
        app.UseHttpsRedirection();
      }

      app.UseCors();
      app.UseAuthentication();
      app.UseAuthorization();

      app.MapGet("/", context =>
      {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
      });

      app.MapControllers();

      app.Run();
    }
  }
}
