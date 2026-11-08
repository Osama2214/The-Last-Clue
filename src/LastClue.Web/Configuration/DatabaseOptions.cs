namespace LastClue.Web.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public string? Default { get; set; }
}
