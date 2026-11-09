using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Web.IntegrationTests;

public sealed class ApiTestFactory(string environment = "Development") : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.ConfigureTestServices(services =>
        {
            services.AddControllersWithViews().AddApplicationPart(typeof(FoundationApiController).Assembly);
            services.ConfigureApplicationCookie(options =>
                options.Events.OnValidatePrincipal = _ => Task.CompletedTask);
        });
    }

    public HttpClient ApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
}
