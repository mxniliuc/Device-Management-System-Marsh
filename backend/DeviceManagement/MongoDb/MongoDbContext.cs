using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DeviceManagement.MongoDb;

public sealed class MongoDbContext
{
    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.ConnectionString))
            throw new InvalidOperationException("MongoDb:ConnectionString is required.");
        if (string.IsNullOrWhiteSpace(o.DatabaseName))
            throw new InvalidOperationException("MongoDb:DatabaseName is required.");

        var client = new MongoClient(o.ConnectionString);
        Database = client.GetDatabase(o.DatabaseName);
    }

    public IMongoDatabase Database { get; }

    public IMongoCollection<Models.User> Users => Database.GetCollection<Models.User>("users");
    public IMongoCollection<Models.Device> Devices => Database.GetCollection<Models.Device>("devices");
}

