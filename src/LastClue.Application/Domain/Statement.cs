namespace LastClue.Application.Domain;

/// <summary>A player's statement for one round.</summary>
public sealed class Statement
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public byte Round { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
