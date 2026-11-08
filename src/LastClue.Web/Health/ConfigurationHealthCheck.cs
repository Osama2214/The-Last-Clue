using LastClue.Web.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastClue.Web.Health;

internal sealed class ConfigurationHealthCheck(IOptions<DatabaseOptions> database) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var errors = ConfigurationRules.CoreErrors(database.Value);
        var result = errors.Length == 0 ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Missing or invalid core configuration: " + string.Join(", ", errors));
        return Task.FromResult(result);
    }
}
