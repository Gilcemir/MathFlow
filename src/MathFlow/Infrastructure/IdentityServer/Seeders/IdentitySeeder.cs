using Microsoft.AspNetCore.Identity;
using MathFlow.Infrastructure.IdentityServer.Models;

namespace MathFlow.Infrastructure.IdentityServer.Seeders;

/// <summary>
/// Provides methods for seeding initial Identity data (roles and users).
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Seeds roles and master admin user if they do not exist.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
    /// <param name="configuration">The application configuration.</param>
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider.GetService<ILogger<Program>>();

        string[] roles = { "masterAdmin", "admin", "premium", "normal" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        logger?.LogInformation("Roles seeded successfully");

        var masterEmail = configuration["Identity:MasterAdmin:Email"];
        var masterPassword = configuration["Identity:MasterAdmin:Password"];

        if (string.IsNullOrEmpty(masterEmail) || string.IsNullOrEmpty(masterPassword))
        {
            throw new InvalidOperationException(
                "Master admin credentials not configured in appsettings. " +
                "Please set Identity:MasterAdmin:Email and Identity:MasterAdmin:Password");
        }

        var masterUser = await userManager.FindByEmailAsync(masterEmail);
        if (masterUser == null)
        {
            masterUser = new ApplicationUser
            {
                UserName = masterEmail,
                Email = masterEmail,
                EmailConfirmed = true,
                TwoFactorEnabled = false
            };

            var result = await userManager.CreateAsync(masterUser, masterPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(masterUser, "masterAdmin");
                logger?.LogInformation("Master admin user created: {Email}", masterEmail);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Failed to create master admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger?.LogInformation("Master admin user already exists: {Email}", masterEmail);
        }
    }
}
