namespace LastClue.Application.Domain;

/// <summary>The resolved arrest for one game round.</summary>
public sealed class Arrest
{
    public Guid GameId { get; set; }
    public byte Round { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public bool IsCriminal { get; set; }
    public DateTime ArrestedAt { get; set; }
}
