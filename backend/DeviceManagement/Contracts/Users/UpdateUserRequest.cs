namespace DeviceManagement.Contracts.Users;

public sealed record UpdateUserRequest(
    string Name,
    string Role,
    string Location
);

