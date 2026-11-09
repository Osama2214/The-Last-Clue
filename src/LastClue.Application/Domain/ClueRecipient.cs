namespace LastClue.Application.Domain;

/// <summary>A player granted access to a particular clue.</summary>
public sealed class ClueRecipient
{
    public Guid GameId { get; set; }
    public Guid ClueId { get; set; }
    public string UserId { get; set; } = string.Empty;
    /// <summary>The UTC time from which the recipient may read the clue.</summary>
    public DateTime AvailableAt { get; set; }
}
