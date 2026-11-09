using System.Collections.Concurrent;
using System.Net;
using LastClue.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LastClue.Web.IntegrationTests;

public sealed class ConfigurationTests
{
    private const string ValidConnection = "Server=tcp:127.0.0.1,1;Database=LastClue;Integrated Security=true;Connect Timeout=1;ConnectRetryCount=0";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Password=validation-password;invalid-entry")]
    [InlineData("Server=database.test")]
    [InlineData("Database=LastClue")]
    [InlineData("Server=;Database=LastClue")]
    public async Task Missing_or_malformed_database_configuration_fails_configuration_readiness(string? connection)
    {
        using var factory = CreateFactory(new() { ["ConnectionStrings:Default"] = connection });
        using var client = factory.CreateClient();
        var report = await ConfigurationReport(factory);
        Assert.Equal(HealthStatus.Unhealthy, report.Status);
        Assert.Equal("Missing or invalid core configuration: ConnectionStrings:Default", report.Entries["configuration"].Description);
        using var live = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
    }

    [Theory]
    [InlineData(false, false, null, null, false, "disabled")]
    [InlineData(true, true, "validation-key", "validation-model", false, "known-outage")]
    [InlineData(true, false, null, "validation-model", false, "credentials-missing")]
    [InlineData(true, false, "  ", "validation-model", false, "credentials-missing")]
    [InlineData(true, false, "validation-key", null, false, "model-missing")]
    [InlineData(true, false, "validation-key", "validation-model", true, null)]
    public async Task Ai_configuration_selects_attempt_or_fallback_without_affecting_core_readiness(
        bool enabled, bool outage, string? key, string? model, bool canAttempt, string? reason)
    {
        using var factory = CreateFactory(new()
        {
            ["ConnectionStrings:Default"] = ValidConnection,
            ["AiProvider:Enabled"] = enabled.ToString(),
            ["AiProvider:KnownOutage"] = outage.ToString(),
            ["AiProvider:ApiKey"] = key,
            ["AiProvider:Model"] = model
        });
        using var client = factory.CreateClient();
        var options = factory.Services.GetRequiredService<IOptions<AiProviderOptions>>().Value;
        Assert.Equal(canAttempt, options.CanAttemptGeneration);
        Assert.Equal(reason, options.FallbackReason);
        Assert.Equal(HealthStatus.Healthy, (await ConfigurationReport(factory)).Status);
        // Valid configuration alone does not prove database connectivity.
        using var ready = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ready.StatusCode);
    }

    [Theory]
    [InlineData("Server=database.test;Database=LastClue")]
    [InlineData("Data Source=database.test;Initial Catalog=LastClue")]
    public async Task Database_aliases_are_accepted_without_attempting_a_connection(string connection)
    {
        using var factory = CreateFactory(new() { ["ConnectionStrings:Default"] = connection });
        Assert.Equal(HealthStatus.Healthy, (await ConfigurationReport(factory)).Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("mailto:player@example.test")]
    [InlineData("https://example.test/game")]
    [InlineData("https://example.test?key=origin-validation-sentinel")]
    [InlineData("https://name:origin-validation-sentinel@example.test")]
    [InlineData("https://example.test#game")]
    public void Invalid_application_origin_stops_startup_without_echoing_its_value(string origin)
    {
        using var factory = CreateFactory(new() { ["AppOptions:AllowedOrigin"] = origin });
        var error = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.Contains("AppOptions:AllowedOrigin", error.ToString());
        Assert.DoesNotContain("origin-validation-sentinel", error.ToString());
    }

    [Fact]
    public async Task Invalid_database_and_ai_secrets_do_not_appear_in_logs_or_probe_responses()
    {
        const string databaseSecret = "database-validation-sentinel";
        const string aiSecret = "provider-validation-sentinel";
        var logs = new CapturedLogs();
        using var factory = CreateFactory(new()
        {
            ["ConnectionStrings:Default"] = "Password=" + databaseSecret + ";invalid-entry",
            ["AiProvider:Enabled"] = "true",
            ["AiProvider:KnownOutage"] = "true",
            ["AiProvider:ApiKey"] = aiSecret,
            ["AiProvider:Model"] = "validation-model"
        }, logs);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var output = await response.Content.ReadAsStringAsync() + string.Join("\n", logs.Messages);
        Assert.DoesNotContain(databaseSecret, output);
        Assert.DoesNotContain(aiSecret, output);
        Assert.Contains("ConnectionStrings:Default", string.Join("\n", logs.Messages));
        Assert.DoesNotContain(databaseSecret, factory.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value.ToString()!);
        Assert.DoesNotContain(aiSecret, factory.Services.GetRequiredService<IOptions<AiProviderOptions>>().Value.ToString()!);
    }

    [Fact]
    public void Invalid_log_level_stops_startup()
    {
        using var factory = CreateFactory(new() { ["Logging:LogLevel:Default"] = "UnsupportedLevel" });
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }

    [SqlServerFact]
    public async Task Readiness_is_healthy_when_sql_server_is_reachable_even_without_ai_credentials()
    {
        using var factory = CreateFactory(new()
        {
            ["ConnectionStrings:Default"] = Environment.GetEnvironmentVariable("LAST_CLUE_TEST_SQLSERVER"),
            ["AiProvider:Enabled"] = "true",
            ["AiProvider:ApiKey"] = null
        });
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"status\":\"Healthy\"}", await response.Content.ReadAsStringAsync());
    }

    private sealed class SqlServerFactAttribute : FactAttribute
    {
        public SqlServerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LAST_CLUE_TEST_SQLSERVER")))
                Skip = "Set LAST_CLUE_TEST_SQLSERVER to run against SQL Server.";
        }
    }

    private static WebApplicationFactory<Program> CreateFactory(
        Dictionary<string, string?> settings, CapturedLogs? logs = null)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(settings));
            if (logs is not null)
            {
                builder.ConfigureLogging(logging => logging.AddProvider(logs));
            }
        });
    }

    private static Task<HealthReport> ConfigurationReport(WebApplicationFactory<Program> factory)
    {
        return factory.Services.GetRequiredService<HealthCheckService>()
            .CheckHealthAsync(registration => registration.Name == "configuration");
    }

    private sealed class CapturedLogs : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CapturedLogger(Messages);
        public void Dispose() { }

        private sealed class CapturedLogger(ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                messages.Enqueue(formatter(state, exception) + exception?.ToString());
            }
        }
    }
}
