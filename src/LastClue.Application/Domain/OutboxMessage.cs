namespace LastClue.Application.Domain;

/// <summary>An event stored for delivery after its transaction commits.</summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? GameId { get; set; }
    /// <summary>The committed version clients use to order state updates.</summary>
    public long StateVersion { get; set; }
    public string Audience { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    /// <summary>The UTC time at which event delivery was recorded.</summary>
    public DateTime? DispatchedAt { get; set; }
}
