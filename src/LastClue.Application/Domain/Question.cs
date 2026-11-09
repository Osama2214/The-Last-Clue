namespace LastClue.Application.Domain;

/// <summary>A detective's question and its target-owned answer.</summary>
public sealed class Question
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public byte Round { get; set; }
    public string DetectiveId { get; set; } = string.Empty;
    /// <summary>The numbered question slot available to the detective in this round.</summary>
    public byte Slot { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public DateTime AskedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
}
