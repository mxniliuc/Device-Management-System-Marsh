namespace DeviceManagement.Contracts.Auth;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    string UserId,
    string Email
);
