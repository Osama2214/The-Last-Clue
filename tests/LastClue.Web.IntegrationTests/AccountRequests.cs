using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace LastClue.Web.IntegrationTests;

internal static partial class AccountRequests
{
    public static async Task<HttpResponseMessage> PostAsync(
        HttpClient client, string action, object? data = null) =>
        await WriteAsync(client, HttpMethod.Post, $"/api/v1/auth/{action}", data);

    public static async Task<HttpResponseMessage> WriteAsync(
        HttpClient client, HttpMethod method, string path, object? data = null)
    {
        using var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var match = CsrfToken().Match(await response.Content.ReadAsStringAsync());
        Assert.True(match.Success);
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", WebUtility.HtmlDecode(match.Groups[1].Value));
        if (data is not null)
            request.Content = JsonContent.Create(data);

        return await client.SendAsync(request);
    }

    [GeneratedRegex("""<meta name="csrf-token" content="([^"]+)"\s*/>""")]
    private static partial Regex CsrfToken();
}
