using LastClue.Application.Domain;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class GameRepository(AppDbContext database) : IGameRepository
{
    public Task<Game?> GetGameForUpdateAsync(Guid gameId, CancellationToken cancellationToken)
    {
        if (database.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Start a transaction before locking a game.");

        return database.Games
            .FromSql($"SELECT * FROM [dbo].[Games] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {gameId}")
            .AsTracking()
            .Include(game => game.Players)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void AddGame(Game game) => database.Games.Add(game);

    public void AddOutboxMessage(OutboxMessage message) => database.OutboxMessages.Add(message);
}
