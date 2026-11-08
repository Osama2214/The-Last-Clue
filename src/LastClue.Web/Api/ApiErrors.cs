using System.Globalization;
using LastClue.Application.Exceptions;

namespace LastClue.Web.Api;

internal static class ApiErrors
{
    public static ApiError ForStatus(HttpContext context, int status) => status switch
    {
        400 => Error(context, "INVALID_INPUT", "The request is invalid."),
        401 => Error(context, "AUTHENTICATION_REQUIRED", "Sign in to continue."),
        403 => Error(context, "FORBIDDEN", "You cannot perform this action."),
        404 => Error(context, "NOT_FOUND", "The requested resource was not found."),
        405 => Error(context, "METHOD_NOT_ALLOWED", "This request method is not allowed."),
        409 => Error(context, "CONFLICT", "The request conflicts with the current state."),
        413 => Error(context, "PAYLOAD_TOO_LARGE", "The request is too large."),
        415 => Error(context, "UNSUPPORTED_MEDIA_TYPE", "This content type is not supported."),
        429 => Error(context, "RATE_LIMITED", "Too many requests. Try again later."),
        503 => Error(context, "SERVICE_UNAVAILABLE", "The service is temporarily unavailable."),
        >= 500 => Error(context, "INTERNAL_ERROR", "The request could not be completed."),
        _ => Error(context, "HTTP_ERROR", "The request could not be completed.")
    };

    public static async Task WriteAsync(HttpContext context, int status, ApiError? error = null)
    {
        context.Response.StatusCode = status;
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Remove("Location");
        await context.Response.WriteAsJsonAsync(error ?? ForStatus(context, status), context.RequestAborted);
    }

    public static Task WriteExceptionAsync(HttpContext context, Exception exception)
    {
        var status = exception switch
        {
            ValidationException => 400, ForbiddenException => 403, NotFoundException => 404,
            ConflictException => 409, RateLimitedException => 429,
            BadHttpRequestException bad when bad.StatusCode is >= 400 and < 500 => bad.StatusCode,
            _ => 500
        };
        ApiError? error = exception is ConflictException conflict ? conflict.Code switch
        {
            DomainErrorCode.PhaseChanged => Error(context, "PHASE_CHANGED", "The game phase has changed. Reload the current state.", conflict.CurrentStateVersion),
            DomainErrorCode.RoomFull => Error(context, "ROOM_FULL", "This room is full."),
            DomainErrorCode.AlreadyInRoom => Error(context, "ALREADY_IN_ROOM", "You already belong to a room."),
            DomainErrorCode.ReplayMismatch => Error(context, "REPLAY_MISMATCH", "This request ID was used for a different request."),
            _ => null
        } : null;
        if (exception is RateLimitedException { RetryAfterSeconds: { } seconds })
            context.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        return WriteAsync(context, status, error);
    }

    private static ApiError Error(HttpContext context, string code, string message, long? version = null)
        => new(code, message, ApiRequest.CorrelationId(context), version);
}
