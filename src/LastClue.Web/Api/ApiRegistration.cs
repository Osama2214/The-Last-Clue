using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LastClue.Web.Api;

internal static class ApiRegistration
{
    public static IServiceCollection AddApiFoundation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = action =>
            {
                action.HttpContext.Response.Headers.CacheControl = "no-store";
                return new JsonResult(ApiErrors.ForStatus(action.HttpContext, 400)) { StatusCode = 400 };
            };
        });
        // Identity will register the cookie handler when accounts are implemented.
        services.AddAuthentication(IdentityConstants.ApplicationScheme);
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
            branch.UseMiddleware<ApiExceptionMiddleware>();
            branch.UseStatusCodePages(context => ApiErrors.WriteAsync(context.HttpContext, context.HttpContext.Response.StatusCode));
        });
        return app;
    }
}
