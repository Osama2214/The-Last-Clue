namespace LastClue.Application.Domain;

/// <summary>A versioned achievement and its award criteria.</summary>
public sealed class AchievementDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CriteriaJson { get; set; } = string.Empty;
    public Guid SettingsVersionId { get; set; }
}
