using LastClue.Application.Exceptions;
using LastClue.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Web.Configuration;

internal static class SettingsSeedCommand
{
    public const string Name = "--seed-settings";

    public static async Task<int> RunAsync(IServiceProvider services, string[] args)
    {
        if (args.Length != 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            Console.Error.WriteLine("Usage: --seed-settings <existing-user-id>");
            return 1;
        }

        try
        {
            await using var scope = services.CreateAsyncScope();
            var seeded = await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync(args[1]);
            Console.WriteLine(seeded ? "Settings version 1 was created." : "Settings version 1 already exists.");
            return 0;
        }
        catch (ValidationException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        catch (Exception exception) when (exception is SqlException or DbUpdateException or InvalidOperationException)
        {
            Console.Error.WriteLine("Could not seed settings. Check the database connection, permissions and migration.");
            return 1;
        }
    }
}
