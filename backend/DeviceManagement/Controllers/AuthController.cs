using System.Security.Claims;
using DeviceManagement.Contracts.Auth;
using DeviceManagement.Auth;
using DeviceManagement.Models;
using DeviceManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManagement.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthUserRepository _authUsers;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<AuthUser> _passwordHasher;
    private readonly IJwtTokenService _tokens;

    public AuthController(
        IAuthUserRepository authUsers,
        IUserRepository users,
        IPasswordHasher<AuthUser> passwordHasher,
        IJwtTokenService tokens)
    {
        _authUsers = authUsers;
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = "Password and confirmation do not match."
            });
        }

        var email = request.Email.Trim();
        var normalized = NormalizeEmail(email);
        if (await _authUsers.GetByEmailNormalizedAsync(normalized, ct) is { } existingAuth)
        {
            var linkedUser = await _users.GetByIdAsync(existingAuth.UserId, ct);
            if (linkedUser is null)
            {
                await _authUsers.DeleteByEmailNormalizedAsync(normalized, ct);
            }
            else
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = "An account with this email already exists."
                });
            }
        }

        var companyRole = request.Role.Trim();
        if (string.IsNullOrWhiteSpace(companyRole))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = "Role is required."
            });
        }

        var profile = new User
        {
            Name = request.Name.Trim(),
            Role = companyRole,
            Location = request.Location.Trim()
        };
        var createdProfile = await _users.CreateAsync(profile, ct);

        var auth = new AuthUser
        {
            EmailNormalized = normalized,
            UserId = createdProfile.Id
        };
        auth.PasswordHash = _passwordHasher.HashPassword(auth, request.Password);
        await _authUsers.CreateAsync(auth, ct);

        var (token, expires) = _tokens.CreateToken(createdProfile.Id, email);
        return Ok(new LoginResponse(token, expires, createdProfile.Id, email));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var normalized = NormalizeEmail(request.Email);
        var auth = await _authUsers.GetByEmailNormalizedAsync(normalized, ct);
        if (auth is null)
            return Unauthorized();

        var profile = await _users.GetByIdAsync(auth.UserId, ct);
        if (profile is null)
        {
            await _authUsers.DeleteByEmailNormalizedAsync(normalized, ct);
            return Unauthorized();
        }

        var verify = _passwordHasher.VerifyHashedPassword(auth, auth.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized();

        var email = request.Email.Trim();
        var (token, expires) = _tokens.CreateToken(auth.UserId, email);
        return Ok(new LoginResponse(token, expires, auth.UserId, email));
    }

    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new { userId = id, email });
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
