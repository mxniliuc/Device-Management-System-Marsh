using DeviceManagement.Models;
using DeviceManagement.MongoDb;
using MongoDB.Driver;

namespace DeviceManagement.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly MongoDbContext _db;

    public UserRepository(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Users.Find(Builders<User>.Filter.Empty).ToListAsync(ct);
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct)
    {
        await _db.Users.InsertOneAsync(user, cancellationToken: ct);
        return user;
    }

    public async Task<bool> UpdateAsync(string id, User user, CancellationToken ct)
    {
        user.Id = id;
        var res = await _db.Users.ReplaceOneAsync(u => u.Id == id, user, cancellationToken: ct);
        return res.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct)
    {
        var res = await _db.Users.DeleteOneAsync(u => u.Id == id, ct);
        return res.DeletedCount == 1;
    }
}

