using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LastClue.Web.Health;

internal static class HealthResponse
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.Headers.CacheControl = "no-store";
        return context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
    }
}
