using LastClue.Infrastructure.Accounts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Web.Configuration;

internal static class IdentitySeedCommand
{
    public const string Name = "--seed-identity";

    public static async Task<int> RunAsync(IServiceProvider services, IConfiguration configuration, string[] args)
    {
        var email = configuration["Admin:Email"]?.Trim();
        var password = configuration["Admin:Password"];
        if (args.Length != 1 || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("Usage: --seed-identity with Admin:Email and Admin:Password set through secure configuration.");
            return 1;
        }

        try
        {
            await using var scope = services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync(email, password);
            Console.WriteLine("Identity roles and administrator are configured.");
            return 0;
        }
        catch (Exception exception) when (exception is SqlException or DbUpdateException or InvalidOperationException)
        {
            Console.Error.WriteLine("Could not seed Identity. Check the migration, database permissions and administrator configuration.");
            return 1;
        }
    }
}
