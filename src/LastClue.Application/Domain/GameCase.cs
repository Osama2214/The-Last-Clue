namespace LastClue.Application.Domain;

/// <summary>The private case snapshot used for a game.</summary>
public sealed class GameCase
{
    public Guid GameId { get; set; }
    public Guid? CaseVariantId { get; set; }
    public CaseSource Source { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    /// <summary>The SHA-256 hash of the saved case payload.</summary>
    public string SnapshotHash { get; set; } = string.Empty;
}
