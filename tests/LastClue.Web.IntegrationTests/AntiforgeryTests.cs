using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastClue.Web.IntegrationTests;

public sealed class AntiforgeryTests
{
    [Fact]
    public async Task Forged_write_is_rejected_and_anonymous_profile_challenges_with_json()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });
        using var forged = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { email = "forged@example.test", displayName = "Forged", password = "Test-pass8" });
        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
        Assert.Equal("application/json", forged.Content.Headers.ContentType?.MediaType);
        using var anonymous = await client.GetAsync("/api/v1/profiles/me");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal("application/json", anonymous.Content.Headers.ContentType?.MediaType);
        Assert.Null(anonymous.Headers.Location);
    }
}
