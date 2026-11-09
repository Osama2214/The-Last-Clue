using LastClue.Infrastructure.Persistence;
using LastClue.Web.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastClue.Web.Health;

internal sealed class SqlReadinessCheck(
    AppDbContext database, IOptions<DatabaseOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (ConfigurationRules.CoreErrors(options.Value).Length != 0)
            return HealthCheckResult.Unhealthy("Database configuration is invalid.");

        return await database.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("SQL Server is unavailable.");
    }
}
