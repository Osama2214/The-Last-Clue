namespace LastClue.Web.Configuration;

internal static class ConfigurationRegistration
{
    public static IServiceCollection AddProjectConfiguration(this IServiceCollection services)
    {
        services.AddOptions<AppOptions>()
            .BindConfiguration(AppOptions.SectionName)
            .Validate(ConfigurationRules.IsOriginValid,
                "AppOptions:AllowedOrigin must be an HTTP or HTTPS origin without credentials, a path, query or fragment.")
            .ValidateOnStart();

        services.AddOptions<LoggingConfigurationOptions>()
            .BindConfiguration("Logging")
            .Validate(ConfigurationRules.AreLogLevelsValid,
                "Logging:LogLevel must contain supported logging level names.")
            .ValidateOnStart();

        services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
        services.AddOptions<AiProviderOptions>().BindConfiguration(AiProviderOptions.SectionName);
        services.AddHostedService<ConfigurationStartupCheck>();
        return services;
    }
}
