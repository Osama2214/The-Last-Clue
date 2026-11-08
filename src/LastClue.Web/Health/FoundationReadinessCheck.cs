using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LastClue.Web.Health;

/// <summary>Keeps startup unavailable until durable game storage and case coverage are implemented.</summary>
internal sealed class FoundationReadinessCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Unhealthy(
            "Game storage and reviewed case coverage are not configured yet."));
    }
}
