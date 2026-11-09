using LastClue.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure.Tests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(SqlServerRepositoryFixture.ConnectionVariable)))
            Skip = $"Set {SqlServerRepositoryFixture.ConnectionVariable} to run SQL Server integration tests.";
    }
}

public sealed class SqlServerRepositoryFixture : IAsyncLifetime
{
    public const string ConnectionVariable = "LAST_CLUE_TEST_SQLSERVER";
    private ServiceProvider? _services;

    public AsyncServiceScope CreateScope() =>
        (_services ?? throw new InvalidOperationException("SQL Server fixture is unavailable.")).CreateAsyncScope();

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var connection = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"LastClueRepositoryTests_{Guid.NewGuid():N}"
        };
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection.ConnectionString));
        services.AddInfrastructure();
        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        await using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_services is null)
            return;

        await using var services = _services;
        await using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
    }
}
