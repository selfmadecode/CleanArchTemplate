using Domain.Entities;
using Domain.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Domain.Helper.PermissionProvider;
using static System.Net.Mime.MediaTypeNames;

namespace Infrastructure.Persistence;

public static class SeedData
{
    public static async Task EnsureSeedData(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {        
        // apply any pending migrations
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedRolesWithPermissionsAsync(roleManager);
        await SeedAdminUserAsync(userManager, roleManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = PermissionProvider.GetAllRoles();

        foreach (var roleEnum in roles)
        {
            var roleName = roleEnum;

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var newRole = new ApplicationRole { Name = roleName };
                await roleManager.CreateAsync(newRole);
            }
        }
    }

    private static async Task SeedRolesWithPermissionsAsync(RoleManager<ApplicationRole> roleManager)
    {
        var allRoles = PermissionProvider.GetAllRoles();

        foreach (var roleEnum in allRoles)
        {
            var roleName = roleEnum;
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                role = new ApplicationRole { Name = roleName };
                await roleManager.CreateAsync(role);
            }

            var permissions = PermissionProvider.GetPermissionsForRole(roleEnum);
            var existingClaims = await roleManager.GetClaimsAsync(role);

            foreach (var permission in permissions)
            {
                if (!existingClaims.Any(c => c.Type == nameof(Permission) && c.Value == permission.ToString()))
                {
                    await roleManager.AddClaimAsync(role, new Claim(nameof(Permission), permission.ToString()));
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        await EnsureUserAsync(userManager, roleManager,
            email: "superadmin@cleanarchtemplate.com",
            firstName: "Super",
            lastName: "Administrator",
            password: "cjAuto@12345",
            roleName: Role.SUPERADMIN.ToString());

        await EnsureUserAsync(userManager, roleManager,
            email: "admin@cleanarchtemplate.com",
            firstName: "Admin",
            lastName: "Administrator",
            password: "cjAuto@12345",
            roleName: Role.ADMIN.ToString());

        await EnsureUserAsync(userManager, roleManager,
            email: "appuser@cleanarchtemplate.com",
            firstName: "App",
            lastName: "User",
            password: "cjAuto@12345",
            roleName: Role.USER.ToString());
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
    string email, string firstName, string lastName, string password, string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
            return;

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }

        user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user '{email}': {errors}");
        }
    }
}
