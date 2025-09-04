namespace TaskStream.Logging.Worker.Interfaces;

public interface IMongoDbService
{
    Task SaveEventAsync(string eventJson);
}