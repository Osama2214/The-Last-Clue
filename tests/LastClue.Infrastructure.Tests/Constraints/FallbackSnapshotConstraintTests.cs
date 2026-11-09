using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class FallbackSnapshotConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_a_fallback_snapshot_without_a_variant()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (game, _, _) = await CreateTwoGamesAsync(database);
        database.GameCases.Add(new GameCase { GameId = game.Id, Source = CaseSource.Fallback,
            PayloadJson = "{}", SnapshotHash = new string('0', 64) });
        await RejectAsync(database, "CK_GameCases_SourceVariant", 547);
    }
}
