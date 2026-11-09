using LastClue.Infrastructure.Accounts;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LastClue.UI.Tests;

public sealed class AccountBrowserTests : IClassFixture<BrowserApplication>
{
    private readonly BrowserApplication _app;

    public AccountBrowserTests(BrowserApplication app) => _app = app;

    [SqlBrowserTheory]
    [InlineData(1280, 800)]
    [InlineData(360, 780)]
    public async Task Register_sign_in_show_initials_when_provider_is_blocked_and_sign_out(int width, int height)
    {
        var context = await _app.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height }
        });
        try
        {
            var page = await context.NewPageAsync();
            await page.RouteAsync("https://api.dicebear.com/**", route => route.AbortAsync());
            var email = $"{Guid.NewGuid():N}@example.test";
            page.SetDefaultTimeout(6000);
            var registration = await page.GotoAsync(_app.BaseUrl + "/Account/Register");
            Assert.True(registration?.Ok, $"Registration page HTTP status: {registration?.Status} at {page.Url}");
            await page.GetByLabel("Display name").FillAsync("Test Investigator");
            await page.GetByLabel("Email").FillAsync(email);
            await page.GetByLabel("Password", new() { Exact = true }).FillAsync(BrowserApplication.Password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Sign in" })).ToBeVisibleAsync();

            await page.GetByLabel("Email").FillAsync(email);
            await page.GetByLabel("Password").FillAsync("Incorrect8");
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
            await Expect(page.GetByText("Invalid email or password.", new() { Exact = true })).ToBeVisibleAsync();

            await page.GetByLabel("Password").FillAsync(BrowserApplication.Password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
            await Expect(page.GetByText("Test Investigator", new() { Exact = true })).ToBeVisibleAsync();
            await Expect(page.Locator(".avatar-initials")).ToHaveTextAsync("TI");
            await Expect(page.Locator(".avatar-initials")).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Create Room" })).ToBeVisibleAsync();
            Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= innerWidth"));

            await page.GetByRole(AriaRole.Button, new() { Name = "Sign out" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Sign in" })).ToBeVisibleAsync();
        }
        finally
        {
            await context.CloseAsync();
        }
    }
}

public sealed class SqlBrowserTheoryAttribute : TheoryAttribute
{
    public SqlBrowserTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(BrowserApplication.ConnectionVariable)))
            Skip = "Set LAST_CLUE_TEST_SQLSERVER and install Playwright Chromium to run the browser flow.";
    }
}

public sealed class SqlBrowserFactAttribute : FactAttribute
{
    public SqlBrowserFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(BrowserApplication.ConnectionVariable)))
            Skip = "Set LAST_CLUE_TEST_SQLSERVER and install Playwright Chromium to run the browser flow.";
    }
}

public sealed class BrowserApplication : IAsyncLifetime
{
    public const string ConnectionVariable = "LAST_CLUE_TEST_SQLSERVER";
    public const string Password = "Browser-test8";
    private WebApplicationFactory<Program>? _factory;
    private IPlaywright? _playwright;
    public IServiceProvider Services => _factory!.Services;
    private IBrowser? _browser;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser is unavailable.");
    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        var connection = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"LastClueBrowserTests_{Guid.NewGuid():N}"
        };
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Development").ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:Default"] = connection.ConnectionString })));
        _factory.UseKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 0));
        using var client = _factory.CreateClient();
        BaseUrl = client.BaseAddress!.GetLeftPart(UriPartial.Authority);

        await using var scope = _factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync("admin@example.test", Password);
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();
        _playwright?.Dispose();
        if (_factory is not null)
        {
            await using var factory = _factory;
            await using var scope = factory.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
        }
    }
}
