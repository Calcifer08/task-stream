using TaskStream.Logging.Worker.Interfaces;
using TaskStream.Logging.Worker.Services;

namespace TaskStream.Logging.Worker
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = Host.CreateApplicationBuilder(args);

      builder.Services.AddSingleton<IMongoDbService, MongoDbService>();
      builder.Services.AddHostedService<RabbitMQListener>();

      var host = builder.Build();
      host.Run();
    }
  }
}