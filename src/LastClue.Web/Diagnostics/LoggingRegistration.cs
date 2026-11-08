namespace LastClue.Web.Diagnostics;

internal static class LoggingRegistration
{
    public static ILoggingBuilder AddSafeRequestLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddJsonConsole(options => options.IncludeScopes = true);
        // Framework HTTP logs can include query strings, route values or raw exception messages.
        // The application emits its own safe request and failure events instead.
        logging.AddFilter("Microsoft.AspNetCore", LogLevel.None);
        return logging;
    }
}
