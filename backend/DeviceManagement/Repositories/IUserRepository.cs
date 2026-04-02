using DeviceManagement.Models;

namespace DeviceManagement.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken ct);
    Task<User?> GetByIdAsync(string id, CancellationToken ct);
    Task<User> CreateAsync(User user, CancellationToken ct);
    Task<bool> UpdateAsync(string id, User user, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
}

