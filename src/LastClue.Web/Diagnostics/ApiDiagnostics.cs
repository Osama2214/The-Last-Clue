using System.Security.Claims;
using LastClue.Application.Exceptions;
using LastClue.Web.Api;

namespace LastClue.Web.Diagnostics;

internal static class ApiDiagnostics
{
    private static readonly object AbortedKey = new();
    public static void MarkAborted(HttpContext context) => context.Items[AbortedKey] = true;
    public static bool WasAborted(HttpContext context) => context.Items.ContainsKey(AbortedKey);

    public static string? UserId(HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated == true
            && Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id.ToString("D") : null;
    }

    public static void LogFailure(ILogger logger, HttpContext context, Exception exception)
    {
        if (exception is DomainException or BadHttpRequestException) return;
        // Exception messages, Data and inner exceptions may contain submitted secrets.
        logger.LogError(new EventId(1001, "ApiFailure"),
            "API failure. CorrelationId: {CorrelationId}; UserId: {UserId}; ExceptionType: {ExceptionType}; StackTrace: {StackTrace}",
            ApiRequest.CorrelationId(context), UserId(context), exception.GetType().FullName, exception.StackTrace);
    }
}
