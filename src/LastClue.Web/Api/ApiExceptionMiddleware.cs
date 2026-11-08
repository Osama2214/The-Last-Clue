using LastClue.Web.Diagnostics;

namespace LastClue.Web.Api;

internal sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            ApiDiagnostics.MarkAborted(context);
            context.Abort();
        }
        catch (Exception exception)
        {
            ApiDiagnostics.LogFailure(logger, context, exception);
            if (context.Response.HasStarted)
            {
                ApiDiagnostics.MarkAborted(context);
                context.Abort();
                return;
            }
            context.Response.Clear();
            await ApiErrors.WriteExceptionAsync(context, exception);
        }
    }
}
