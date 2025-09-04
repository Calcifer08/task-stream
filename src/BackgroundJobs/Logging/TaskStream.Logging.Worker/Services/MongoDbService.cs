using MongoDB.Bson;
using MongoDB.Driver;
using TaskStream.Logging.Worker.Interfaces;

namespace TaskStream.Logging.Worker.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoCollection<BsonDocument> _eventsCollection;

    public MongoDbService(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDb:ConnectionString"];
        var databaseName = configuration["MongoDb:DatabaseName"];
        var collectionName = configuration["MongoDb:CollectionName"];

        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);
        _eventsCollection = mongoDatabase.GetCollection<BsonDocument>(collectionName);
    }

    public async Task SaveEventAsync(string eventJson)
    {
        var document = BsonDocument.Parse(eventJson);
        await _eventsCollection.InsertOneAsync(document);
    }
}