using System.ComponentModel.DataAnnotations;

namespace EsportPlatform.API.Data.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(32, MinimumLength = 3)]
    [RegularExpression(
        @"^[a-zA-Z0-9_.-]+$",
        ErrorMessage = "Username may only contain letters, numbers, dots, underscores and hyphens.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(30, MinimumLength = 1)]
    public string? Role { get; set; }
}

public record LoginDto(
    [Required]
    string UserName,

    [Required]
    string Password
);

public record AuthResponseDto(
    string UserId,
    string UserName,
    string FullName,
    string Token,
    DateTime ExpiresAt,
    IReadOnlyList<string> Roles
);
