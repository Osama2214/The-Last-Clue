using System.Net;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace LastClue.Web.IntegrationTests;

public sealed class ApiLoggingTests
{
    [Theory]
    [InlineData("Development", "games")]
    [InlineData("Development", "admin/cases")]
    [InlineData("Development", "auth")]
    [InlineData("Production", "games")]
    [InlineData("Production", "admin/cases")]
    [InlineData("Production", "auth")]
    public async Task Sensitive_values_never_enter_logs_scopes_or_failure_responses(string environment, string area)
    {
        var capture = new DiagnosticLogCapture();
        using var factory = new ApiTestFactory(environment).WithWebHostBuilder(builder =>
            builder.ConfigureLogging(logging => logging.AddProvider(capture)));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/v1/{area}/path-private-sentinel/diagnostics-test?token=query-private-sentinel");
        request.Headers.Add("Authorization", "Bearer authorization-private-sentinel");
        request.Headers.Add("X-Private-Clue", "header-private-sentinel");
        request.Headers.Add("Cookie", "private=cookie-private-sentinel");
        request.Content = new StringContent("body-private-sentinel", Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(request);
        using var body = await ApiContractTests.AssertError(response, 500, "INTERNAL_ERROR");
        var output = body.RootElement.GetRawText() + string.Join("\n", capture.Entries.Select(entry =>
            entry.Message + entry.Scopes + entry.Exception + string.Join(";", entry.Fields.Select(field => field.Value))));
        foreach (var value in new[] { "path-private-sentinel", "query-private-sentinel", "authorization-private-sentinel",
            "header-private-sentinel", "cookie-private-sentinel", "body-private-sentinel", "inner-private-sentinel", "data-private-sentinel" })
            Assert.DoesNotContain(value, output);

        var completed = Assert.Single(capture.Entries, entry => entry.Event == "ApiRequestCompleted");
        Assert.Equal(500, completed.Fields["StatusCode"]);
        Assert.Contains("{gameId}", (string)completed.Fields["Route"]!);
        Assert.True((double)completed.Fields["DurationMs"]! >= 0);
        Assert.Equal(response.Headers.GetValues("X-Correlation-Id").Single(), completed.Fields["CorrelationId"]);
        Assert.Contains((string)completed.Fields["CorrelationId"]!, completed.Scopes);
        var failure = Assert.Single(capture.Entries, entry => entry.Event == "ApiFailure");
        Assert.Equal("System.InvalidOperationException", failure.Fields["ExceptionType"]);
        Assert.Contains(nameof(FoundationApiController.SecretFailure), (string)failure.Fields["StackTrace"]!);
        Assert.Null(failure.Exception);
    }

    [Fact]
    public async Task Unknown_paths_and_queries_are_not_logged_as_route_text()
    {
        var capture = new DiagnosticLogCapture();
        using var factory = new ApiTestFactory().WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddProvider(capture)));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var response = await client.GetAsync("/api/v1/path-private-sentinel?password=query-private-sentinel");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var completed = Assert.Single(capture.Entries, entry => entry.Event == "ApiRequestCompleted");
        Assert.Equal("<unmatched>", completed.Fields["Route"]);
        var output = string.Join("\n", capture.Entries.Select(entry => entry.Message + entry.Scopes));
        Assert.DoesNotContain("path-private-sentinel", output);
        Assert.DoesNotContain("query-private-sentinel", output);
    }

    [Fact]
    public async Task Authenticated_logs_use_the_account_id_without_email_or_name()
    {
        var capture = new DiagnosticLogCapture();
        using var factory = new ApiTestFactory().WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddProvider(capture)));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var session = await client.PostAsync("/api/v1/foundationapi/session", null);
        Assert.Equal(HttpStatusCode.NoContent, session.StatusCode);
        using var response = await client.GetAsync("/api/v1/foundationapi/secured");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var completed = capture.Entries.Last(entry => entry.Event == "ApiRequestCompleted");
        Assert.Equal("ec27d53b-f67a-447e-9e3c-063ca54cf56c", completed.Fields["UserId"]);
        var output = string.Join("\n", capture.Entries.Select(entry => entry.Message + entry.Scopes));
        Assert.DoesNotContain("private-email-sentinel", output);
        Assert.DoesNotContain("private-name-sentinel", output);
    }

    [Fact]
    public async Task Parallel_requests_have_independent_response_and_log_correlation_ids()
    {
        var capture = new DiagnosticLogCapture();
        using var factory = new ApiTestFactory().WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddProvider(capture)));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var first = new HttpRequestMessage(HttpMethod.Get, "/api/v1/foundationapi/errors/validation");
        using var second = new HttpRequestMessage(HttpMethod.Get, "/api/v1/foundationapi/errors/not-found");
        first.Headers.Add("X-Correlation-Id", "0a2f6765-e2cc-4ee5-8350-ae9a2f3180e5");
        second.Headers.Add("X-Correlation-Id", "181b127e-6fc2-4de1-b5dc-9c087ec6730c");
        var responses = await Task.WhenAll(client.SendAsync(first), client.SendAsync(second));
        try
        {
            Assert.NotEqual(responses[0].Headers.GetValues("X-Correlation-Id").Single(), responses[1].Headers.GetValues("X-Correlation-Id").Single());
            var completed = capture.Entries.Where(entry => entry.Event == "ApiRequestCompleted").ToArray();
            Assert.Equal(2, completed.Length);
            Assert.Equal(2, completed.Select(entry => entry.Fields["CorrelationId"]).Distinct().Count());
        }
        finally { foreach (var response in responses) response.Dispose(); }
    }
}
