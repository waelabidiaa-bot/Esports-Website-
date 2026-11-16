using EsportPlatform.API.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EsportPlatform.API.Data.Seeders;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var logger = services.GetService<ILoggerFactory>()?.CreateLogger("AdminSeeder");

        var username = configuration["Admin:UserName"];
        var password = configuration["Admin:Password"];
        var fullName = configuration["Admin:FullName"] ?? "Administrator";

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole("Admin"));

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        // No hardcoded credentials: if admin seed config is not provided, do not create/modify accounts.
        // Existing administrators (if any) remain intact.
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger?.LogInformation("Admin seed skipped because Admin:UserName/Admin:Password are not configured.");
            return;
        }

        username = username.Trim();
        fullName = fullName.Trim();

        var admin = await userManager.FindByNameAsync(username);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = username,
                FullName = string.IsNullOrWhiteSpace(fullName) ? "Administrator" : fullName
            };

            var createResult = await userManager.CreateAsync(admin, password);

            if (!createResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}