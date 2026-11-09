using LastClue.Infrastructure.Persistence;
using LastClue.Web.Diagnostics;
using LastClue.Web.Api;
using LastClue.Web.Configuration;
using LastClue.Web.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSafeRequestLogging();
builder.Services.AddProjectConfiguration();
builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseSqlServer(services.GetRequiredService<IOptions<DatabaseOptions>>().Value.Default));
builder.Services.AddControllersWithViews();
builder.Services.AddApiFoundation();
builder.Services.AddHealthChecks()
    .AddCheck<ConfigurationHealthCheck>("configuration", tags: ["ready"])
    .AddCheck<FoundationReadinessCheck>("game-storage", tags: ["ready"]);

var app = builder.Build();
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
