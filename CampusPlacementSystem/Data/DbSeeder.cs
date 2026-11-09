using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementSystem.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndSuperAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // 1. Seed Roles
        string[] roles = [AppRoles.SuperAdmin, AppRoles.PlacementOfficer, AppRoles.CompanyHR, AppRoles.Student];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role created: {Role}", role);
            }
        }

        // 2. Seed Initial SuperAdmin
        var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@campusplacement.com";
        var adminPassword = configuration["DefaultAdmin:Password"] ?? "Admin@123456";
        var adminFullName = configuration["DefaultAdmin:FullName"] ?? "System Administrator";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                AccountStatus = AccountStatus.Active,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AppRoles.SuperAdmin);
                logger.LogInformation("SuperAdmin user created successfully with email: {Email}", adminEmail);
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to create SuperAdmin user: {Errors}", errors);
            }
        }
    }
}
