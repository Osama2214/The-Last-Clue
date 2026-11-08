namespace LastClue.Application.Domain;

/// <summary>Shared limits for a supported game configuration.</summary>
public static class GameRules
{
    public const int MinimumPlayers = 4;
    public const int MaximumPlayers = 8;
    public const int MaximumRounds = 3;
    public const int MaximumAccusations = 2;
    public const int MaximumAccusationsPerRound = 1;

    /// <summary>Returns the detective count for a supported lobby size.</summary>
    public static int DetectiveCount(int playerCount)
    {
        if (playerCount is < MinimumPlayers or > MaximumPlayers)
        {
            throw new ArgumentOutOfRangeException(nameof(playerCount), playerCount,
                "A game requires between four and eight players.");
        }

        return playerCount <= 5 ? 1 : 2;
    }
}
