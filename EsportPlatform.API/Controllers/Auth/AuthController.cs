using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EsportPlatform.API.Data.DTOs.Auth;
using EsportPlatform.API.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string DefaultUserRole = "User";
    private const string DefaultAdminRole = "Admin"; 

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSettings = jwtOptions.Value;
    }

    /// <summary>
    /// Creates an account from a full name, username, password and confirmation,
    /// then signs the new user in by returning a JWT.
    /// Public registration always assigns the User role.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest("Passwords do not match.");
        }

        var userName = dto.UserName.Trim();
        var fullName = dto.FullName.Trim();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest("Full name and username are required.");
        }

        if (await _userManager.FindByNameAsync(userName) is not null)
        {
            return Conflict("That username is already taken.");
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            FullName = fullName
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors);
        }

        // Public registration must never grant elevated roles.
        const string roleName = DefaultUserRole;

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(roleResult.Errors);
            }
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(addToRoleResult.Errors);
        }

        var response = await CreateAuthResponseAsync(user);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Signs a user in with their username and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName.Trim());
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized("Invalid username or password.");
        }

        var response = await CreateAuthResponseAsync(user);
        return Ok(response);
    }

    /// <summary>
    /// Returns the identity encoded in the current JWT. Useful for validating a
    /// stored token after a page refresh.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            userId = user.Id,
            userName = user.UserName,
            fullName = user.FullName,
            roles
        });
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = GenerateJwtToken(user, roles);

        return new AuthResponseDto(
            user.Id,
            user.UserName!,
            user.FullName,
            token,
            expiresAt,
            roles.ToList());
    }

    private (string Token, DateTime ExpiresAt) GenerateJwtToken(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var userIdString = user.Id.ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userIdString),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userIdString),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expiryMinutes = _jwtSettings.ExpiryMinutes > 0 ? _jwtSettings.ExpiryMinutes : 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}