using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace LastClue.Infrastructure.Accounts;

public sealed class IdentitySeeder(
    AppDbContext database, RoleManager<IdentityRole> roles, UserManager<AppUser> users)
{
    public async Task SeedAsync(string email, string password)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        foreach (var name in new[] { AccountIdentity.Player, AccountIdentity.Admin })
        {
            if (!await roles.RoleExistsAsync(name))
                EnsureSuccess(await roles.CreateAsync(new IdentityRole(name)));
        }

        var admin = await users.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Administrator",
                AvatarSeed = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };
            EnsureSuccess(await users.CreateAsync(admin, password));
            EnsureSuccess(await users.AddToRoleAsync(admin, AccountIdentity.Admin));
        }
        else if (!await users.IsInRoleAsync(admin, AccountIdentity.Admin))
        {
            throw new InvalidOperationException("The configured account already exists without the admin role.");
        }

        await transaction.CommitAsync();
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Could not seed Identity roles or the administrator.");
    }
}
