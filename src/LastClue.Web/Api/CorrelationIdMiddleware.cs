namespace LastClue.Web.Api;

internal sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[ApiRequest.CorrelationHeader];
        var id = supplied.Count == 1 && (Guid.TryParseExact(supplied[0], "D", out var parsed)
            || Guid.TryParseExact(supplied[0], "N", out parsed)) ? parsed : Guid.NewGuid();
        var correlationId = id.ToString("D");
        ApiRequest.SetCorrelationId(context, correlationId);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[ApiRequest.CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });
        using (logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))
        {
            await next(context);
        }
    }
}
