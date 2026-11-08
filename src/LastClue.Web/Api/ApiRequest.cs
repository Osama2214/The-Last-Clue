namespace LastClue.Web.Api;

internal static class ApiRequest
{
    public const string CorrelationHeader = "X-Correlation-Id";
    private static readonly object CorrelationKey = new();

    public static bool IsApi(HttpContext context) => context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    public static string CorrelationId(HttpContext context) => (string)context.Items[CorrelationKey]!;
    public static void SetCorrelationId(HttpContext context, string id)
    {
        context.Items[CorrelationKey] = id;
        context.TraceIdentifier = id;
    }
}
