namespace LastClue.Web.Configuration;

public sealed class AiProviderOptions
{
    public const string SectionName = "AiProvider";
    public bool Enabled { get; set; }
    public bool KnownOutage { get; set; }
    public string? ApiKey { get; set; }
    public string? Model { get; set; }

    public string? FallbackReason => !Enabled ? "disabled"
        : KnownOutage ? "known-outage"
        : string.IsNullOrWhiteSpace(ApiKey) ? "credentials-missing"
        : string.IsNullOrWhiteSpace(Model) ? "model-missing"
        : null;

    public bool CanAttemptGeneration => FallbackReason is null;
}
