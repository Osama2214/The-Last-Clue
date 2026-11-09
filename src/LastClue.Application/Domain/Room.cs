namespace LastClue.Application.Domain;

/// <summary>A lobby that can host multiple games.</summary>
public sealed class Room
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? HostUserId { get; set; }
    public string Language { get; set; } = string.Empty;
    public RoomStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>The committed version clients use to order state updates.</summary>
    public long StateVersion { get; set; }
    /// <summary>The database token used to detect conflicting writes.</summary>
    public byte[] RowVersion { get; set; } = [];

    public ICollection<RoomMember> Members { get; } = new List<RoomMember>();
    public ICollection<Game> Games { get; } = new List<Game>();
}
