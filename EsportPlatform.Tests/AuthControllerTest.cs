using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EsportPlatform.API.Controllers;
using EsportPlatform.API.Data;
using EsportPlatform.API.Data.DTOs.Auth;
using EsportPlatform.API.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace EsportPlatform.Tests;

public class AuthControllerTest
{
    private static readonly JwtSettings TestJwtSettings = new()
    {
        Issuer = "esport-api-tests",
        Audience = "client-tests",
        SecretKey = "unit-test-secret-key-that-is-at-least-32-characters",
        ExpiryMinutes = 60
    };

    // ==========================================
    // REUSABLE HELPER
    // ==========================================
    private static (AuthController Controller, ServiceProvider Provider) CreateController()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        var provider = services.BuildServiceProvider();

        var controller = new AuthController(
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>(),
            Options.Create(TestJwtSettings));

        return (controller, provider);
    }

    private static RegisterDto NewRegisterDto(string userName = "nova") => new()
    {
        FullName = "Alex Turner",
        UserName = userName,
        Password = "Str0ngPassword!",
        ConfirmPassword = "Str0ngPassword!"
    };

    // ==========================================
    // REGISTER
    // ==========================================
    [Fact]
    public async Task Register_ShouldCreateUserWithRoleAndReturnJwt()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        var result = await controller.Register(NewRegisterDto());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);

        var response = Assert.IsType<AuthResponseDto>(objectResult.Value);
        Assert.Equal("nova", response.UserName);
        Assert.Equal("Alex Turner", response.FullName);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.True(response.ExpiresAt > DateTime.UtcNow);

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByNameAsync("nova");

        Assert.NotNull(user);
        Assert.Equal("Alex Turner", user!.FullName);
        Assert.True(await userManager.IsInRoleAsync(user, "User"));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        Assert.Contains(token.Claims, claim =>
            claim.Type == ClaimTypes.Name && claim.Value == "nova");
        Assert.Contains(token.Claims, claim =>
            claim.Type == ClaimTypes.Role && claim.Value == "User");
        Assert.Contains(token.Claims, claim =>
            claim.Type == "fullName" && claim.Value == "Alex Turner");
    }

    [Fact]
    public async Task Register_ShouldRejectMismatchedPasswords()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        var dto = NewRegisterDto();
        dto.ConfirmPassword = "DifferentPassword!";

        var result = await controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Passwords do not match.", badRequest.Value);

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await userManager.FindByNameAsync("nova"));
    }

    [Fact]
    public async Task Register_ShouldRejectDuplicateUsername()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        await controller.Register(NewRegisterDto());
        var result = await controller.Register(NewRegisterDto());

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("That username is already taken.", conflict.Value);
    }

    [Fact]
    public async Task Register_ShouldRejectWeakPassword()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        var dto = NewRegisterDto();
        dto.Password = "weak";
        dto.ConfirmPassword = "weak";

        var result = await controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==========================================
    // LOGIN
    // ==========================================
    [Fact]
    public async Task Login_ShouldReturnJwt_WhenCredentialsAreValid()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        await controller.Register(NewRegisterDto());

        var result = await controller.Login(new LoginDto("nova", "Str0ngPassword!"));

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);

        Assert.Equal("nova", response.UserName);
        Assert.Equal("Alex Turner", response.FullName);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        await controller.Register(NewRegisterDto());

        var result = await controller.Login(new LoginDto("nova", "WrongPassword!"));

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password.", unauthorized.Value);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        var result = await controller.Login(new LoginDto("ghost", "Str0ngPassword!"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ShouldBeCaseInsensitive_ForUserName()
    {
        var (controller, provider) = CreateController();
        using var _ = provider;

        await controller.Register(NewRegisterDto("Nova"));

        var result = await controller.Login(new LoginDto("nova", "Str0ngPassword!"));

        Assert.IsType<OkObjectResult>(result);
    }
}
