using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LastClue.Web.IntegrationTests;

public sealed class ApiContractTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Known_and_unknown_failures_have_safe_json_in_each_environment(string environment)
    {
        using var factory = new ApiTestFactory(environment);
        using var client = factory.ApiClient();
        foreach (var (kind, status, code) in new[]
        {
            ("validation", 400, "INVALID_INPUT"), ("forbidden", 403, "FORBIDDEN"),
            ("not-found", 404, "NOT_FOUND"), ("phase", 409, "PHASE_CHANGED"),
            ("room-full", 409, "ROOM_FULL"), ("already-in-room", 409, "ALREADY_IN_ROOM"),
            ("replay", 409, "REPLAY_MISMATCH"), ("rate-limit", 429, "RATE_LIMITED"),
            ("unexpected", 500, "INTERNAL_ERROR")
        })
        {
            using var response = await client.GetAsync("/api/v1/foundationapi/errors/" + kind);
            using var body = await AssertError(response, status, code);
            Assert.DoesNotContain("private-error-sentinel", body.RootElement.GetRawText());
            Assert.DoesNotContain("Exception", body.RootElement.GetRawText());
            if (kind == "phase") Assert.Equal(42, body.RootElement.GetProperty("currentStateVersion").GetInt64());
            else Assert.False(body.RootElement.TryGetProperty("currentStateVersion", out _));
            if (kind == "rate-limit") Assert.Equal(30, response.Headers.RetryAfter!.Delta!.Value.TotalSeconds);
        }
    }

    [Theory]
    [InlineData(400, "INVALID_INPUT")]
    [InlineData(401, "AUTHENTICATION_REQUIRED")]
    [InlineData(403, "FORBIDDEN")]
    [InlineData(404, "NOT_FOUND")]
    [InlineData(405, "METHOD_NOT_ALLOWED")]
    [InlineData(409, "CONFLICT")]
    [InlineData(413, "PAYLOAD_TOO_LARGE")]
    [InlineData(415, "UNSUPPORTED_MEDIA_TYPE")]
    [InlineData(429, "RATE_LIMITED")]
    [InlineData(500, "INTERNAL_ERROR")]
    [InlineData(503, "SERVICE_UNAVAILABLE")]
    public async Task Empty_error_results_use_the_same_contract(int status, string code)
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var response = await client.GetAsync($"/api/v1/foundationapi/status/{status}");
        using var body = await AssertError(response, status, code);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{broken-json")]
    [InlineData("{\"text\":\"private-validation-sentinel\"}")]
    public async Task Invalid_json_or_model_does_not_echo_submitted_values(string input)
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var response = await client.PostAsync("/api/v1/foundationapi/validate", new StringContent(input, Encoding.UTF8, "application/json"));
        using var body = await AssertError(response, 400, "INVALID_INPUT");
        Assert.DoesNotContain("private-validation-sentinel", body.RootElement.GetRawText());
    }

    [Fact]
    public async Task Valid_models_succeed_and_unmapped_methods_return_json()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var valid = await client.PostAsJsonAsync("/api/v1/foundationapi/validate", new { text = "valid" });
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        using var invalidMethod = await client.GetAsync("/api/v1/foundationapi/validate");
        using var body = await AssertError(invalidMethod, 404, "NOT_FOUND");
    }

    [Fact]
    public async Task Cookie_challenge_and_forbid_return_json_without_login_redirects()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var anonymous = await client.GetAsync("/api/v1/foundationapi/secured");
        using var anonymousBody = await AssertError(anonymous, 401, "AUTHENTICATION_REQUIRED");
        using var signIn = await client.PostAsync("/api/v1/foundationapi/session", null);
        Assert.Equal(HttpStatusCode.NoContent, signIn.StatusCode);
        using var secured = await client.GetAsync("/api/v1/foundationapi/secured");
        Assert.Equal(HttpStatusCode.OK, secured.StatusCode);
        using var forbidden = await client.GetAsync("/api/v1/foundationapi/admin");
        using var forbiddenBody = await AssertError(forbidden, 403, "FORBIDDEN");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid-correlation-sentinel")]
    [InlineData("20ea771d-d14c-42ce-9a08-0bcedd1e2b2b")]
    public async Task Correlation_id_is_validated_and_matches_the_header(string? supplied)
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/missing");
        if (supplied is not null) request.Headers.Add("X-Correlation-Id", supplied);
        using var response = await client.SendAsync(request);
        using var body = await AssertError(response, 404, "NOT_FOUND");
        var id = body.RootElement.GetProperty("correlationId").GetString();
        if (Guid.TryParse(supplied, out var expected)) Assert.Equal(expected.ToString("D"), id);
        else Assert.NotEqual(supplied, id);
    }

    [Fact]
    public async Task Api_prefix_is_segment_scoped_and_health_responses_keep_their_contract()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var notApi = await client.GetAsync("/apiary/missing");
        Assert.Equal(HttpStatusCode.NotFound, notApi.StatusCode);
        Assert.NotEqual("application/json", notApi.Content.Headers.ContentType?.MediaType);
        using var live = await client.GetAsync("/health");
        using var health = JsonDocument.Parse(await live.Content.ReadAsStringAsync());
        Assert.Single(health.RootElement.EnumerateObject());
        Assert.Equal("Healthy", health.RootElement.GetProperty("status").GetString());
        Assert.True(Guid.TryParse(live.Headers.GetValues("X-Correlation-Id").Single(), out _));
    }

    [Theory]
    [InlineData(400, "INVALID_INPUT")]
    [InlineData(422, "HTTP_ERROR")]
    [InlineData(500, "INTERNAL_ERROR")]
    public async Task Custom_mvc_error_results_do_not_leak_their_original_content(int status, string code)
    {
        using var factory = new ApiTestFactory();
        using var client = factory.ApiClient();
        using var response = await client.GetAsync($"/api/v1/foundationapi/custom-error/{status}");
        using var body = await AssertError(response, status, code);
        Assert.DoesNotContain("private-result-sentinel", body.RootElement.GetRawText());
    }

    internal static async Task<JsonDocument> AssertError(HttpResponseMessage response, int status, string code)
    {
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Headers.Location);
        Assert.Contains("no-store", response.Headers.CacheControl!.ToString());
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(code, body.RootElement.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("message").GetString()));
        var correlation = body.RootElement.GetProperty("correlationId").GetString();
        Assert.True(Guid.TryParseExact(correlation, "D", out _));
        Assert.Equal(response.Headers.GetValues("X-Correlation-Id").Single(), correlation);
        return body;
    }
}
