using MongoDB.Driver;
using Viora.Domain.Archives;

namespace Viora.Infrastructure.Archives;

internal class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        MongoDbConventions.RegisterConventions();
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Archive> Archives => _database.GetCollection<Archive>("archives");
    public IMongoCollection<Folder> Folders => _database.GetCollection<Folder>("folders");
    public IMongoCollection<Record> Records => _database.GetCollection<Record>("records");
    public IMongoCollection<Template> Templates => _database.GetCollection<Template>("templates");
}

