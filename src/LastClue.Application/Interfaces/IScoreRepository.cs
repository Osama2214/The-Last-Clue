using LastClue.Application.Domain;

namespace LastClue.Application.Interfaces;

public interface IScoreRepository
{
    /// <summary>Reads a player's stored finalized score total.</summary>
    Task<long> GetTotalScoreAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Tracks a finalized score entry for insertion.</summary>
    void AddScoreEntry(ScoreEntry scoreEntry);
}
