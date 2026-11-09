namespace LastClue.Application.Domain;

/// <summary>A version of the rules and progression settings.</summary>
public sealed class SettingsVersion
{
    public Guid Id { get; set; }
    public int VersionNo { get; set; }
    public string ValuesJson { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
