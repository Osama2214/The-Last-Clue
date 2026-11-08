using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LastClue.Web.Api;

internal sealed class ApiErrorResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (!ApiRequest.IsApi(context.HttpContext)) return;
        var status = context.Result switch
        {
            ObjectResult result => result.StatusCode,
            JsonResult result => result.StatusCode,
            StatusCodeResult result => result.StatusCode,
            ContentResult result => result.StatusCode,
            _ => null
        } ?? context.HttpContext.Response.StatusCode;
        if (status is < 400 or > 599) return;

        context.HttpContext.Response.Headers.CacheControl = "no-store";
        if (context.Result is JsonResult { Value: ApiError } or ObjectResult { Value: ApiError }) return;
        context.Result = new JsonResult(ApiErrors.ForStatus(context.HttpContext, status)) { StatusCode = status };
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
