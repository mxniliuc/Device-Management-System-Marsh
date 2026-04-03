using DeviceManagement.Models;
using DeviceManagement.Search;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DeviceManagement.MongoDb;

/// <summary>
/// Ensures a weighted text index on device fields and backfills <see cref="Device.RamSearch"/> for existing documents.
/// </summary>
public sealed class DeviceTextSearchBootstrapper : IHostedService
{
    public const string TextIndexName = "devices_text_search";

    private readonly MongoDbContext _db;

    public DeviceTextSearchBootstrapper(MongoDbContext db)
    {
        _db = db;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var collection = _db.Devices;
        await EnsureTextIndexAsync(collection, cancellationToken);
        await BackfillRamSearchAsync(collection, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task EnsureTextIndexAsync(IMongoCollection<Device> collection, CancellationToken ct)
    {
        using var cursor = await collection.Indexes.ListAsync(ct);
        var indexes = await cursor.ToListAsync(ct);
        foreach (var doc in indexes)
        {
            if (doc.TryGetValue("name", out var name) && name.IsString && name.AsString == TextIndexName)
                return;
        }

        var keys = Builders<Device>.IndexKeys
            .Text(d => d.Name)
            .Text(d => d.Manufacturer)
            .Text(d => d.Processor)
            .Text(d => d.RamSearch);

        var options = new CreateIndexOptions
        {
            Name = TextIndexName,
            DefaultLanguage = "english",
            Weights = new BsonDocument
            {
                ["name"] = 10,
                ["manufacturer"] = 5,
                ["processor"] = 3,
                ["ramSearch"] = 1
            }
        };

        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Device>(keys, options), cancellationToken: ct);
    }

    private static async Task BackfillRamSearchAsync(IMongoCollection<Device> collection, CancellationToken ct)
    {
        var devices = await collection.Find(Builders<Device>.Filter.Empty).ToListAsync(ct);
        if (devices.Count == 0)
            return;

        var models = new List<WriteModel<Device>>();
        foreach (var d in devices)
        {
            var expected = DeviceRamSearchText.Build(d.RamGb);
            if (d.RamSearch == expected)
                continue;

            models.Add(new UpdateOneModel<Device>(
                Builders<Device>.Filter.Eq(x => x.Id, d.Id),
                Builders<Device>.Update.Set(x => x.RamSearch, expected)));
        }

        if (models.Count > 0)
            await collection.BulkWriteAsync(models, cancellationToken: ct);
    }
}
