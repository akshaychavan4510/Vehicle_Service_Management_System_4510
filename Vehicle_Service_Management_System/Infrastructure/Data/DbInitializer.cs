using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Infrastructure.Identity;

namespace Vehicle_Service_Management_System.Infrastructure.Data
{
    // ============================================================
    // Application Roles
    // ============================================================
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Staff = "Staff";

        public static readonly string[] All = new[]
        {
            Admin,
            Manager,
            Staff
        };
    }

    // ============================================================
    // Database Initializer
    // ============================================================
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var serviceProvider = scope.ServiceProvider;

            var context =
                serviceProvider.GetRequiredService<ApplicationDbContext>();

            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();


            // =====================================================
            // 1. Apply Pending Migrations
            // =====================================================

            await context.Database.MigrateAsync();


            // =====================================================
            // 2. Create Roles
            // =====================================================

            foreach (var role in ApplicationRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            $"Role '{role}' creation failed: " +
                            string.Join(
                                ", ",
                                roleResult.Errors.Select(
                                    e => e.Description)));
                    }
                }
            }


            // =====================================================
            // 3. Admin Account
            // =====================================================

            const string adminEmail =
                "admin@vehicleservice.local";

            const string adminPassword =
                "Admin@45";


            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            bool isNewAdmin = false;


            // =====================================================
            // 4. Create Admin If Not Exists
            // =====================================================

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };


                var createResult =
                    await userManager.CreateAsync(
                        adminUser,
                        adminPassword);


                if (!createResult.Succeeded)
                {
                    throw new Exception(
                        "Admin user creation failed: " +
                        string.Join(
                            ", ",
                            createResult.Errors.Select(
                                e => e.Description)));
                }

                isNewAdmin = true;
            }


            // =====================================================
            // 5. Ensure Admin Role
            // =====================================================

            if (!await userManager.IsInRoleAsync(
                    adminUser,
                    ApplicationRoles.Admin))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        ApplicationRoles.Admin);


                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Admin role assignment failed: " +
                        string.Join(
                            ", ",
                            roleResult.Errors.Select(
                                e => e.Description)));
                }
            }


            // =====================================================
            // 6. Reset Password (only for newly created admin)
            // =====================================================

            if (isNewAdmin)
            {
                var resetToken =
                    await userManager
                        .GeneratePasswordResetTokenAsync(
                            adminUser);

                var passwordResult =
                    await userManager.ResetPasswordAsync(
                        adminUser,
                        resetToken,
                        adminPassword);

                if (!passwordResult.Succeeded)
                {
                    throw new Exception(
                        "Admin password reset failed: " +
                        string.Join(
                            ", ",
                            passwordResult.Errors.Select(
                                e => e.Description)));
                }
            }


            // =====================================================
            // 7. Final Success Message
            // =====================================================

            Console.WriteLine(
                "=================================================");

            Console.WriteLine(
                "Database seeding completed successfully.");

            Console.WriteLine(
                $"Admin Email: {adminEmail}");

            Console.WriteLine(
                "Admin Password: Admin@45");

            Console.WriteLine(
                "=================================================");
        }
    }
}