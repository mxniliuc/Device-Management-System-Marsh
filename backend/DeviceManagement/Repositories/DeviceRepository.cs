using DeviceManagement.Models;
using DeviceManagement.MongoDb;
using MongoDB.Driver;

namespace DeviceManagement.Repositories;

public sealed class DeviceRepository : IDeviceRepository
{
    private readonly MongoDbContext _db;

    public DeviceRepository(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<Device>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Devices.Find(Builders<Device>.Filter.Empty).ToListAsync(ct);
    }

    public async Task<Device?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _db.Devices.Find(d => d.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<Device> CreateAsync(Device device, CancellationToken ct)
    {
        await _db.Devices.InsertOneAsync(device, cancellationToken: ct);
        return device;
    }

    public async Task<bool> UpdateAsync(string id, Device device, CancellationToken ct)
    {
        device.Id = id;
        var res = await _db.Devices.ReplaceOneAsync(d => d.Id == id, device, cancellationToken: ct);
        return res.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct)
    {
        var res = await _db.Devices.DeleteOneAsync(d => d.Id == id, ct);
        return res.DeletedCount == 1;
    }
}

