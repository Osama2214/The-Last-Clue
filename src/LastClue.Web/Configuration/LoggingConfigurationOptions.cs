namespace LastClue.Web.Configuration;

public sealed class LoggingConfigurationOptions
{
    public Dictionary<string, string> LogLevel { get; set; } = new();
}
