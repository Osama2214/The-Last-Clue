using LastClue.Application.Domain;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class ScoreRepository(AppDbContext database) : IScoreRepository
{
    public Task<long> GetTotalScoreAsync(string userId, CancellationToken cancellationToken) =>
        database.ScoreEntries.Where(entry => entry.UserId == userId)
            .SumAsync(entry => (long)entry.Delta, cancellationToken);

    public void AddScoreEntry(ScoreEntry scoreEntry) => database.ScoreEntries.Add(scoreEntry);
}
