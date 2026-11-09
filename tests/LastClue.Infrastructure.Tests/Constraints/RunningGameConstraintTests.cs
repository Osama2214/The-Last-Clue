using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class RunningGameConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_a_second_running_game_in_the_same_room()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (first, _, _) = await CreateTwoGamesAsync(database);
        database.Games.Add(NewGame(first.RoomId, first.SettingsVersionId, 2));
        await RejectAsync(database, "IX_Games_RoomId", 2601, 2627);
    }
}
