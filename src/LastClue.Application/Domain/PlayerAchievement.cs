namespace LastClue.Application.Domain;

/// <summary>A player's earned achievement tied to the game that awarded it.</summary>
public sealed class PlayerAchievement
{
    public string UserId { get; set; } = string.Empty;
    public string AchievementCode { get; set; } = string.Empty;
    public Guid DefinitionId { get; set; }
    public Guid GameId { get; set; }
    public DateTime AwardedAt { get; set; }
}
