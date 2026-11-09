namespace LastClue.Application.Domain;

/// <summary>A rank and its minimum points in a settings version.</summary>
public sealed class RankDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MinPoints { get; set; }
    public Guid SettingsVersionId { get; set; }
}
