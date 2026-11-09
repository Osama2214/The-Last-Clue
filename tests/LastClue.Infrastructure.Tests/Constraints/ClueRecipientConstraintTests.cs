using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class ClueRecipientConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_a_recipient_from_another_game()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (first, second, player) = await CreateTwoGamesAsync(database);
        var clue = new GameClue { Id = Guid.NewGuid(), GameId = first.Id, ClueKey = "test", Round = 1, Body = "Test clue" };
        database.GameClues.Add(clue);
        await database.SaveChangesAsync();
        database.ClueRecipients.Add(new ClueRecipient { GameId = second.Id, ClueId = clue.Id, UserId = player.Id, AvailableAt = Now });
        await RejectAsync(database, "FK_ClueRecipients_GameClues_GameId_ClueId", 547);
    }
}
