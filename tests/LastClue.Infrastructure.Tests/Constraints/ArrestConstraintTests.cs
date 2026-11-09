using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class ArrestConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_a_second_arrest_of_the_same_target()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (game, _, player) = await CreateTwoGamesAsync(database);
        database.Arrests.Add(new Arrest { GameId = game.Id, Round = 1, TargetId = player.Id, ArrestedAt = Now });
        await database.SaveChangesAsync();
        database.Arrests.Add(new Arrest { GameId = game.Id, Round = 2, TargetId = player.Id, ArrestedAt = Now });
        await RejectAsync(database, "IX_Arrests_GameId_TargetId", 2601, 2627);
    }
}
