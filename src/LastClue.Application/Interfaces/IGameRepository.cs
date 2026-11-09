using LastClue.Application.Domain;

namespace LastClue.Application.Interfaces;

public interface IGameRepository
{
    /// <summary>Locks and loads a tracked game with its players inside an active transaction.</summary>
    Task<Game?> GetGameForUpdateAsync(Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>Tracks a new game for insertion.</summary>
    void AddGame(Game game);

    /// <summary>Queues an outbox row to be saved with the associated state change.</summary>
    void AddOutboxMessage(OutboxMessage message);
}
