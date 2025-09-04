using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TaskStream.Logging.Worker.Interfaces;

namespace TaskStream.Logging.Worker;

public class RabbitMQListener : BackgroundService
{
  private readonly ILogger<RabbitMQListener> _logger;
  private readonly IServiceProvider _serviceProvider;
  private IConnection _connection = null!;
  private IModel _channel = null!;

  public RabbitMQListener(IConfiguration configuration, ILogger<RabbitMQListener> logger, IServiceProvider serviceProvider)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;

    var factory = new ConnectionFactory()
    {
      HostName = configuration["RabbitMQ:Host"],
      Port = int.Parse(configuration["RabbitMQ:Port"]!)
    };

    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();

    _logger.LogInformation("Подключено к RabbitMQ.");
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var exchangeName = "taskstream_events_topic";
    _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);

    var queueName = _channel.QueueDeclare().QueueName;

    _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "#");

    _logger.LogInformation("Ожидание событий...");

    var consumer = new EventingBasicConsumer(_channel);

    consumer.Received += async (model, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = Encoding.UTF8.GetString(body);
      _logger.LogInformation("Получено событие: {Message}", message);

      try
      {
        using (var scope = _serviceProvider.CreateScope())
        {
          var mongoService = scope.ServiceProvider.GetRequiredService<IMongoDbService>();
          await mongoService.SaveEventAsync(message);
        }

        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Ошибка при обработке сообщения: {Message}", message);

        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
      }
    };

    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

    return Task.CompletedTask;
  }

  public override void Dispose()
  {
    _logger.LogInformation("Закрытие соединения с RabbitMQ...");

    if (_channel?.IsOpen ?? false) _channel.Close();
    if (_connection?.IsOpen ?? false) _connection.Close();
  }
}