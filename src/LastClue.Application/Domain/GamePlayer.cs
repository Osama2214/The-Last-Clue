namespace LastClue.Application.Domain;

/// <summary>A player's assigned role and participation in one game.</summary>
public sealed class GamePlayer
{
    public Guid GameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public GameRole Role { get; set; }
    public string? CharacterKey { get; set; }
    /// <summary>Private character instructions visible only to the assigned player.</summary>
    public string PrivateScript { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsArrested { get; set; }
    /// <summary>Whether the player has already used the reconnect grace period.</summary>
    public bool GraceUsed { get; set; }
    /// <summary>The UTC deadline for the current reconnect grace period.</summary>
    public DateTime? GraceUntil { get; set; }
    public DateTime? DepartedAt { get; set; }
}
