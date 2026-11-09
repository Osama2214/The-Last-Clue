namespace LastClue.Application.Domain;

/// <summary>A clue scheduled for release to a specific audience.</summary>
public sealed class GameClue
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string ClueKey { get; set; } = string.Empty;
    public byte Round { get; set; }
    public ClueAudience Audience { get; set; }
    public string? CharacterKey { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime? ReleasedAt { get; set; }
}
