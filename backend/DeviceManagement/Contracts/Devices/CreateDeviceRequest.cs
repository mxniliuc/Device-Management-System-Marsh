using DeviceManagement.Models;

namespace DeviceManagement.Contracts.Devices;

public sealed record CreateDeviceRequest(
    string Name,
    string Manufacturer,
    DeviceType Type,
    string Os,
    string OsVersion,
    string Processor,
    int RamGb,
    string Description,
    string Location,
    string? AssignedToUserId
);

