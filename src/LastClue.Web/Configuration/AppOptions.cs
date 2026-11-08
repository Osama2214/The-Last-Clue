namespace LastClue.Web.Configuration;

public sealed class AppOptions
{
    public const string SectionName = "AppOptions";
    public string AllowedOrigin { get; set; } = string.Empty;
}
