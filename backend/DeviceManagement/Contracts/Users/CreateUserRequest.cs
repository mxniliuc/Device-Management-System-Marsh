namespace DeviceManagement.Contracts.Users;

public sealed record CreateUserRequest(
    string Name,
    string Role,
    string Location
);

