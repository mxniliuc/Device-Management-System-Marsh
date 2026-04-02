using DeviceManagement.Models;

namespace DeviceManagement.Repositories;

public interface IDeviceRepository
{
    Task<List<Device>> GetAllAsync(CancellationToken ct);
    Task<Device?> GetByIdAsync(string id, CancellationToken ct);
    Task<Device> CreateAsync(Device device, CancellationToken ct);
    Task<bool> UpdateAsync(string id, Device device, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
}

