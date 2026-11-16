using System.ComponentModel.DataAnnotations;

namespace EsportPlatform.WEB.Models;

/// <summary>Payload sent to POST /api/auth/register.</summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Use between 2 and 80 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(32, MinimumLength = 3, ErrorMessage = "Use between 3 and 32 characters.")]
    [RegularExpression(
        @"^[a-zA-Z0-9_.-]+$",
        ErrorMessage = "Username may only contain letters, numbers, dots, underscores and hyphens.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Use at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(30, MinimumLength = 1)]
    public string? Role { get; set; }
}

/// <summary>Payload sent to POST /api/auth/login.</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>The session returned by the API after a successful register or login.</summary>
public class AuthSession
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    /// <summary>Primary role for convenience (derived from Roles or JWT when missing).</summary>
    public string? Role { get; set; }

    /// <summary>All roles returned by the API (login/register/me responses).</summary>
    public List<string> Roles { get; set; } = new();

    public bool IsInRole(string role) =>
        Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)) ||
        string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Result of an auth call, so pages can render errors without exceptions.</summary>
public sealed record AuthResult(bool Succeeded, string? Error, AuthSession? Session)
{
    public static AuthResult Success(AuthSession session) => new(true, null, session);
    public static AuthResult Failure(string error) => new(false, error, null);
}
