using LastClue.Application.Accounts;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Accounts;

internal sealed class IdentityAuthService(
    AppDbContext database, UserManager<AppUser> users, SignInManager<AppUser> signIn) : IAuthService
{
    public async Task<RegistrationResult> RegisterAsync(string email, string displayName, string password)
    {
        email = email.Trim();
        displayName = displayName.Trim();
        if (displayName.Length == 0)
            return Invalid("displayName", "Enter a display name.");

        await using var transaction = await database.Database.BeginTransactionAsync();
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            AvatarSeed = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
                return created.Errors.Any(error => error.Code is "DuplicateEmail" or "DuplicateUserName")
                    ? EmailInUse()
                    : created.Errors.Any(error => error.Code.StartsWith("Password", StringComparison.Ordinal))
                        ? Invalid("password", "Use at least 8 characters and include a digit.")
                        : Invalid("email", "Enter a valid email address.");

            var assigned = await users.AddToRoleAsync(user, AccountIdentity.Player);
            if (!assigned.Succeeded)
                throw new InvalidOperationException("Could not assign the player role.");

            await transaction.CommitAsync();
            return new(RegistrationStatus.Created);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            return EmailInUse();
        }
    }

    private static RegistrationResult EmailInUse() =>
        new(RegistrationStatus.EmailInUse, new Dictionary<string, string>
        {
            ["email"] = "That email is already registered."
        });

    private static RegistrationResult Invalid(string field, string message) =>
        new(RegistrationStatus.InvalidAccount, new Dictionary<string, string> { [field] = message });

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || user.IsBlocked)
            return false;

        return (await signIn.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true))
            .Succeeded;
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is not null && !(await users.UpdateSecurityStampAsync(user)).Succeeded)
            throw new InvalidOperationException("Could not invalidate the account session.");

        await signIn.SignOutAsync();
    }
}
