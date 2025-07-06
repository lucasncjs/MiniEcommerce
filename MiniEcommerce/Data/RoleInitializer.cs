using Microsoft.AspNetCore.Identity;
using MiniEcommerce.Models;

public class RoleInitializer
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
    {
        string adminRole = "Admin";
        string adminEmail = "admin@ecommerce.com";
        string adminPassword = "Admin123!";

        // 1️⃣ Crear rol si no existe
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // 2️⃣ Crear usuario admin si no existe
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // ⚠️ Importante si tenés confirmación activada
            };

            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}