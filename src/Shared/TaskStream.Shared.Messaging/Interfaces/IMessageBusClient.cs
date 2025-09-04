namespace TaskStream.Shared.Messaging.Interfaces;

public interface IMessageBusClient
{
    void Publish(string producer, string payloadJson, string routingKey);
}