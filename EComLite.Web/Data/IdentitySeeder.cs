using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EComLite.Web.Data
{
    /// <summary>
    /// Seeds role-based access data for the Version 2 admin workflows.
    /// Always ensures the "Admin" role exists. An admin user is only created when
    /// AdminUser:Email and AdminUser:Password are supplied via configuration
    /// (user-secrets or environment variables) - never hardcoded in appsettings.json.
    /// </summary>
    public static class IdentitySeeder
    {
        public const string AdminRole = "Admin";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

            // 1. Ensure the Admin role exists.
            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
                logger.LogInformation("Seeded role '{Role}'.", AdminRole);
            }

            // 2. Optionally seed an admin user from configuration.
            var adminEmail = config["AdminUser:Email"];
            var adminPassword = config["AdminUser:Password"];
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogInformation(
                    "No AdminUser:Email/Password configured; skipping admin user seeding. " +
                    "Set them via user-secrets or environment variables to seed an admin account.");
                return;
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var created = await userManager.CreateAsync(admin, adminPassword);
                if (!created.Succeeded)
                {
                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join("; ", created.Errors.Select(e => e.Description)));
                    return;
                }
                logger.LogInformation("Seeded admin user '{Email}'.", adminEmail);
            }

            if (!await userManager.IsInRoleAsync(admin, AdminRole))
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
                logger.LogInformation("Added '{Email}' to role '{Role}'.", adminEmail, AdminRole);
            }
        }
    }
}
