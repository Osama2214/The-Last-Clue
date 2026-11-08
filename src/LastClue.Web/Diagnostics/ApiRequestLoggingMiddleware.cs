using System.Diagnostics;
using LastClue.Web.Api;
using Microsoft.AspNetCore.Routing;

namespace LastClue.Web.Diagnostics;

internal sealed class ApiRequestLoggingMiddleware(RequestDelegate next, ILogger<ApiRequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "<unmatched>";
            var method = context.Request.Method is "GET" or "POST" or "PUT" or "PATCH" or "DELETE" or "HEAD" or "OPTIONS"
                ? context.Request.Method : "OTHER";
            logger.LogInformation(new EventId(1000, "ApiRequestCompleted"),
                "API request completed. Method: {Method}; Route: {Route}; StatusCode: {StatusCode}; DurationMs: {DurationMs}; CorrelationId: {CorrelationId}; UserId: {UserId}; Aborted: {Aborted}",
                method, route, context.Response.StatusCode, Math.Round(Stopwatch.GetElapsedTime(started).TotalMilliseconds, 2),
                ApiRequest.CorrelationId(context), ApiDiagnostics.UserId(context), ApiDiagnostics.WasAborted(context));
        }
    }
}
