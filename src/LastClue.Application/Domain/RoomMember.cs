namespace LastClue.Application.Domain;

/// <summary>A player's membership and readiness in a room.</summary>
public sealed class RoomMember
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsReady { get; set; }
}
