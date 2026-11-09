namespace LastClue.Application.Domain;

/// <summary>A finalized score change from a completed game.</summary>
public sealed class ScoreEntry
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    /// <summary>The stable key used to prevent duplicate scoring events.</summary>
    public string EventKey { get; set; } = string.Empty;
    public int Delta { get; set; }
    public DateTime CompletedAt { get; set; }
}
