using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Contracts.Auth;

public sealed record LoginRequest(
    [param: Required]
    [param: EmailAddress]
    string Email,
    [param: Required]
    string Password
);
