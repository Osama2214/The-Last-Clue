using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastClue.Web.IntegrationTests;

public sealed class FoundationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FoundationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Home_page_is_available_and_identifies_the_game()
    {
        using var response = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The Last Clue", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Liveness_does_not_require_a_database()
    {
        using var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Readiness_is_unhealthy_without_database_configuration()
    {
        using var response = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("status").GetString());
        Assert.Single(body.RootElement.EnumerateObject());
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
    }
}
