using LastClue.Web.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LastClue.Web.Api;

internal static class ApiRegistration
{
    public static IServiceCollection AddApiFoundation(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options => options.Filters.Add(new ApiErrorResultFilter()));
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = action =>
            {
                action.HttpContext.Response.Headers.CacheControl = "no-store";
                var error = ApiErrors.ForStatus(action.HttpContext, 400);
                if (action.HttpContext.Request.Path.Equals("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase))
                {
                    var fields = new Dictionary<string, string>();
                    foreach (var name in new[] { "email", "displayName", "password" })
                    {
                        if (action.ModelState.Any(field => field.Value?.Errors.Count > 0 &&
                            field.Key.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            fields[name] = "Check this field.";
                    }

                    if (fields.Count > 0)
                        error = error with { Fields = fields };
                }

                return new JsonResult(error) { StatusCode = 400 };
            };
        });
        services.ConfigureApplicationCookie(options =>
        {
            var login = options.Events.OnRedirectToLogin;
            var denied = options.Events.OnRedirectToAccessDenied;
            options.Events.OnRedirectToLogin = context => ApiRequest.IsApi(context.HttpContext)
                ? ApiErrors.WriteAsync(context.HttpContext, 401) : login(context);
            options.Events.OnRedirectToAccessDenied = context => ApiRequest.IsApi(context.HttpContext)
                ? ApiErrors.WriteAsync(context.HttpContext, 403) : denied(context);
        });
        return services;
    }

    public static IApplicationBuilder UseApiFoundation(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseWhen(ApiRequest.IsApi, branch =>
        {
            branch.UseMiddleware<ApiRequestLoggingMiddleware>();
            branch.UseMiddleware<ApiExceptionMiddleware>();
            branch.UseStatusCodePages(context => ApiErrors.WriteAsync(context.HttpContext, context.HttpContext.Response.StatusCode));
        });
        return app;
    }
}
