using LastClue.Infrastructure;
using LastClue.Infrastructure.Persistence;
using LastClue.Web.Diagnostics;
using LastClue.Web.Api;
using LastClue.Web.Configuration;
using LastClue.Web.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var seedSettings = args.Length > 0 && args[0] == SettingsSeedCommand.Name;
var builder = WebApplication.CreateBuilder(seedSettings ? [] : args);
builder.Logging.AddSafeRequestLogging();
builder.Services.AddProjectConfiguration();
builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseSqlServer(services.GetRequiredService<IOptions<DatabaseOptions>>().Value.Default));
builder.Services.AddInfrastructure();
builder.Services.AddControllersWithViews();
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
app.UseApiFoundation();
if (!app.Environment.IsDevelopment())
{
    app.UseWhen(context => !ApiRequest.IsApi(context), pages => pages.UseExceptionHandler("/Home/Error"));
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
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
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();

public partial class Program;
