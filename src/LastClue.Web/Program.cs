using LastClue.Infrastructure;
using LastClue.Infrastructure.Persistence;
using LastClue.Infrastructure.Accounts;
using LastClue.Web.Diagnostics;
using LastClue.Web.Api;
using LastClue.Web.Configuration;
using LastClue.Web.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var seedSettings = args.Length > 0 && args[0] == SettingsSeedCommand.Name;
var seedIdentity = args.Length > 0 && args[0] == IdentitySeedCommand.Name;
var builder = WebApplication.CreateBuilder(seedSettings || seedIdentity ? [] : args);
builder.Logging.AddSafeRequestLogging();
builder.Services.AddProjectConfiguration();
builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseSqlServer(services.GetRequiredService<IOptions<DatabaseOptions>>().Value.Default));
builder.Services.AddInfrastructure();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddAccountIdentity();
builder.Services.AddSingleton<LastClue.Web.Avatars.IAvatarService, LastClue.Web.Avatars.DiceBearAvatarService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlayerOnly", policy => policy.RequireRole(AccountIdentity.Player));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AccountIdentity.Admin));
});
builder.Services.AddScoped<LastClue.Application.Security.IGameAuthorization, LastClue.Application.Security.GameAuthorization>();
builder.Services.AddRoomRateLimits(builder.Configuration);
builder.Services.AddSignalR();
var roomCodeLength = builder.Configuration.GetValue("RoomCodeLength", 6);
if (roomCodeLength is < 1 or > LastClue.Application.Rooms.RoomCodeGenerator.MaxLength)
    throw new InvalidOperationException("RoomCodeLength must be between 1 and 12.");
builder.Services.AddSingleton(new LastClue.Infrastructure.Rooms.RoomOptions(roomCodeLength));
builder.Services.AddApiFoundation();
builder.Services.AddHealthChecks()
    .AddCheck<ConfigurationHealthCheck>("configuration", tags: ["ready"])
    .AddCheck<SqlReadinessCheck>("sql-server", tags: ["ready"], timeout: TimeSpan.FromSeconds(4));

var app = builder.Build();
if (seedSettings)
{
    await using (app)
        Environment.ExitCode = await SettingsSeedCommand.RunAsync(app.Services, args);
    return;
}
if (seedIdentity)
{
    await using (app)
        Environment.ExitCode = await IdentitySeedCommand.RunAsync(app.Services, app.Configuration, args);
    return;
}
app.UseApiFoundation();
if (!app.Environment.IsDevelopment())
{
    app.UseWhen(context => !ApiRequest.IsApi(context), pages => pages.UseExceptionHandler("/Home/Error"));
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseWhen(context => context.Request.Path.StartsWithSegments("/hubs/game"), hub =>
    hub.Use(async (context, next) =>
    {
        if (context.Request.Headers.TryGetValue("Origin", out var origin) &&
            !string.Equals(origin, $"{context.Request.Scheme}://{context.Request.Host}", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }));
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapStaticAssets();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthResponse.WriteAsync
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthResponse.WriteAsync
});
app.MapControllers();
app.MapHub<LastClue.Web.Realtime.GameHub>("/hubs/game", options => options.CloseOnAuthenticationExpiration = true);
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();

public partial class Program;
