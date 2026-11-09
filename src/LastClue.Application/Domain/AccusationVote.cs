namespace LastClue.Application.Domain;

/// <summary>A detective's final choice of suspect for a round.</summary>
public sealed class AccusationVote
{
    public Guid GameId { get; set; }
    public byte Round { get; set; }
    public string DetectiveId { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
