namespace LastClue.Application.Domain;

/// <summary>An individual real-time connection for a game player.</summary>
public sealed class PlayerConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid GameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
