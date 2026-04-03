using DeviceManagement.Models;
using DeviceManagement.MongoDb;
using MongoDB.Driver;

namespace DeviceManagement.Repositories;

public sealed class AuthUserRepository : IAuthUserRepository
{
    private readonly MongoDbContext _db;

    public AuthUserRepository(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<AuthUser?> GetByEmailNormalizedAsync(string emailNormalized, CancellationToken ct)
    {
        return await _db.AuthUsers.Find(a => a.EmailNormalized == emailNormalized).FirstOrDefaultAsync(ct);
    }

    public async Task CreateAsync(AuthUser authUser, CancellationToken ct)
    {
        await _db.AuthUsers.InsertOneAsync(authUser, cancellationToken: ct);
    }

    public async Task<bool> DeleteByEmailNormalizedAsync(string emailNormalized, CancellationToken ct)
    {
        var result = await _db.AuthUsers.DeleteOneAsync(
            a => a.EmailNormalized == emailNormalized,
            cancellationToken: ct);
        return result.DeletedCount > 0;
    }
}
