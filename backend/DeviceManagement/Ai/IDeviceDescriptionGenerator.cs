using DeviceManagement.Contracts.Devices;

namespace DeviceManagement.Ai;

public interface IDeviceDescriptionGenerator
{
    Task<string> GenerateAsync(GenerateDeviceDescriptionRequest specs, CancellationToken cancellationToken);
}
