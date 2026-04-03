using System.ComponentModel.DataAnnotations;

namespace DeviceManagement.Contracts.Auth;

/// <summary>Self-service signup: profile fields plus email/password (with confirmation).</summary>
public sealed record RegisterRequest(
    [param: Required]
    [param: EmailAddress]
    string Email,
    [param: Required]
    [param: StringLength(128, MinimumLength = 8)]
    string Password,
    [param: Required]
    [param: StringLength(128, MinimumLength = 8)]
    string ConfirmPassword,
    [param: Required]
    [param: StringLength(200, MinimumLength = 1)]
    string Name,
    [param: Required]
    [param: StringLength(100, MinimumLength = 1)]
    string Role,
    [param: Required]
    [param: StringLength(500, MinimumLength = 1)]
    string Location
);
