using LastClue.Infrastructure.Accounts;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Web.IntegrationTests;

public sealed class AccountTestDatabase : IAsyncLifetime
{
    public const string AdminEmail = "admin@example.test";
    public const string Password = "Test-password8";
    private WebApplicationFactory<Program>? _factory;

    public WebApplicationFactory<Program> Factory =>
        _factory ?? throw new InvalidOperationException("SQL test database is unavailable.");

    public HttpClient CreateClient(bool handleCookies = true) => Factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = handleCookies
    });

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable(SqlServerFactAttribute.ConnectionVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var connection = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"LastClueAccountTests_{Guid.NewGuid():N}"
        };

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:Default"] = connection.ConnectionString })));

        await using var scope = _factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync(AdminEmail, Password);
    }

    public async Task DisposeAsync()
    {
        if (_factory is null)
            return;

        await using var factory = _factory;
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
    }
}
