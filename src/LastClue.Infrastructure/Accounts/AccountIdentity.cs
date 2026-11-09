using LastClue.Application.Accounts;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure.Accounts;

public static class AccountIdentity
{
    public const string Player = "Player";
    public const string Admin = "Admin";

    public static IServiceCollection AddAccountIdentity(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddDataProtection();
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.Events.OnValidatePrincipal = async context =>
            {
                var signIn = context.HttpContext.RequestServices.GetRequiredService<SignInManager<AppUser>>();
                var user = await signIn.ValidateSecurityStampAsync(context.Principal);
                if (user is { IsBlocked: false })
                    return;

                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            };
        });

        services.AddScoped<IAuthService, IdentityAuthService>();
        services.AddScoped<IdentitySeeder>();
        return services;
    }
}
