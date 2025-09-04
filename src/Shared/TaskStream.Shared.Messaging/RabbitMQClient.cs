using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskStream.Shared.Messaging.Interfaces;

namespace TaskStream.Shared.Messaging;

public class RabbitMQClient : IMessageBusClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQClient> _logger;
    private readonly string _exchangeName = "taskstream_events_topic";

    public RabbitMQClient(IConfiguration configuration, ILogger<RabbitMQClient> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:Host"],
            Port = int.Parse(configuration["RabbitMQ:Port"]!)
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Topic);
            _logger.LogInformation("Подключено к RabbitMQ Message Bus");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось подключиться к RabbitMQ");
            throw;
        }
    }

    public void Publish(string producer, string payloadJson, string routingKey)
    {
        var envelope = new JsonObject
        {
            ["eventId"] = Guid.NewGuid().ToString(),
            ["eventType"] = routingKey,
            ["occurredAt"] = DateTime.UtcNow,
            ["producer"] = producer,
            ["payload"] = JsonNode.Parse(payloadJson)
        };

        var message = envelope.ToJsonString();
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        _logger.LogInformation($"Опубликовано событие с ключом '{routingKey}' от {producer}");
    }

    public void Dispose()
    {
        if (_channel?.IsOpen ?? false) _channel.Close();
        if (_connection?.IsOpen ?? false) _connection.Close();
    }
}