using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class AiSnapshotConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_an_ai_snapshot_with_a_fallback_variant()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (game, _, _) = await CreateTwoGamesAsync(database);
        var version = await CreatePublishedVersionAsync(database);
        var variant = new CaseVariant { Id = Guid.NewGuid(), CaseVersionId = version.Id, Language = "en", PlayerCount = 4, PayloadJson = "{}" };
        database.CaseVariants.Add(variant);
        await database.SaveChangesAsync();
        database.GameCases.Add(new GameCase { GameId = game.Id, CaseVariantId = variant.Id, Source = CaseSource.AI,
            PayloadJson = "{}", SnapshotHash = new string('0', 64) });
        await RejectAsync(database, "CK_GameCases_SourceVariant", 547);
    }
}
