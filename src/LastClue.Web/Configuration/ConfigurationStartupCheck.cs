using Microsoft.Extensions.Options;

namespace LastClue.Web.Configuration;

internal sealed class ConfigurationStartupCheck(
    IOptions<DatabaseOptions> database,
    IOptions<AiProviderOptions> ai,
    ILogger<ConfigurationStartupCheck> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var errors = ConfigurationRules.CoreErrors(database.Value);
        if (errors.Length > 0)
        {
            logger.LogWarning("Readiness requires valid configuration for {ConfigurationKeys}.", string.Join(", ", errors));
        }

        if (ai.Value.FallbackReason is { } reason)
        {
            logger.LogInformation("Case generation is configured for the fallback library. Reason: {FallbackReason}.", reason);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
