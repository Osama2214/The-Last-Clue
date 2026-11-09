using LastClue.Application.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LastClue.Web.Api;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotencyFilterAttribute : ActionFilterAttribute
{
    public const string HeaderName = "Idempotency-Key";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var values = context.HttpContext.Request.Headers[HeaderName];
        if (values.Count != 1 || !Guid.TryParse(values[0], out var requestId) || requestId == Guid.Empty)
            throw new ValidationException("Provide one valid Idempotency-Key GUID.");

        context.HttpContext.Features.Set(new RequestKey(requestId));
    }

    public static Guid GetRequestId(HttpContext context) =>
        context.Features.Get<RequestKey>()?.Id
        ?? throw new InvalidOperationException("The action must use the idempotency filter.");

    private sealed record RequestKey(Guid Id);
}
