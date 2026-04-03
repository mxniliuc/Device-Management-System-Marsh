using DeviceManagement.Models;

namespace DeviceManagement.Repositories;

public interface IAuthUserRepository
{
    Task<AuthUser?> GetByEmailNormalizedAsync(string emailNormalized, CancellationToken ct);
    Task CreateAsync(AuthUser authUser, CancellationToken ct);
    /// <summary>Removes the credential row for this email (e.g. orphaned auth after profile deletion).</summary>
    Task<bool> DeleteByEmailNormalizedAsync(string emailNormalized, CancellationToken ct);
}
