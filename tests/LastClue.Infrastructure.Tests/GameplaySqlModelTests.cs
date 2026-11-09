using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Tests;

public sealed class GameplaySqlModelTests
{
    [Fact]
    public void Case_schema_enforces_published_versions_snapshot_sources_and_same_game_recipients()
    {
        using var database = CreateDatabase();
        var sql = database.Database.GenerateCreateScript();

        Assert.Contains("WHERE [ReviewStatus] = 'Published'", sql);
        Assert.Contains("[PlayerCount] BETWEEN 4 AND 8", sql);
        Assert.Contains("ISJSON([PayloadJson]) = 1", sql);
        Assert.Contains("[Source] = 'AI' AND [CaseVariantId] IS NULL", sql);
        Assert.Contains("[Source] = 'Fallback' AND [CaseVariantId] IS NOT NULL", sql);
        Assert.Contains("FOREIGN KEY ([GameId], [ClueId]) REFERENCES [GameClues] ([GameId], [Id])", sql);
        Assert.Contains("FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId])", sql);
    }

    [Fact]
    public void Action_schema_keeps_actors_in_the_same_game_and_prevents_duplicate_arrests_and_scores()
    {
        using var database = CreateDatabase();
        var sql = database.Database.GenerateCreateScript();

        Assert.Contains("FOREIGN KEY ([GameId], [AuthorId]) REFERENCES [GamePlayers] ([GameId], [UserId])", sql);
        Assert.Contains("FOREIGN KEY ([GameId], [DetectiveId]) REFERENCES [GamePlayers] ([GameId], [UserId])", sql);
        Assert.Contains("FOREIGN KEY ([GameId], [TargetId]) REFERENCES [GamePlayers] ([GameId], [UserId])", sql);
        Assert.Contains("PRIMARY KEY NONCLUSTERED ([GameId], [Round], [DetectiveId])", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Arrests_GameId_TargetId]", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_PendingScoreEvents_GameId_UserId_EventKey]", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_ScoreEntries_GameId_UserId_EventKey]", sql);
    }

    [Fact]
    public void Outbox_schema_supports_lobby_events_and_protects_the_target_and_dispatch_order()
    {
        using var database = CreateDatabase();
        var sql = database.Database.GenerateCreateScript();
        var outbox = database.Model.FindEntityType(typeof(OutboxMessage))!;

        Assert.True(outbox.FindProperty(nameof(OutboxMessage.RoomId))!.IsNullable);
        Assert.True(outbox.FindProperty(nameof(OutboxMessage.GameId))!.IsNullable);
        Assert.Contains("([RoomId] IS NOT NULL AND [GameId] IS NULL) OR ([RoomId] IS NULL AND [GameId] IS NOT NULL)", sql);
        Assert.Contains(outbox.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Room)
            && foreignKey.Properties.Select(property => property.Name).SequenceEqual([nameof(OutboxMessage.RoomId)]));
        Assert.Contains(outbox.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(GamePlayer)
            && foreignKey.Properties.Select(property => property.Name).SequenceEqual([nameof(OutboxMessage.GameId), nameof(OutboxMessage.UserId)]));
        Assert.Contains("CREATE INDEX [IX_OutboxMessages_DispatchedAt_CreatedAt]", sql);
        Assert.Contains("CREATE INDEX [IX_Games_Status_PhaseEndsAt]", sql);
        Assert.Contains("CREATE INDEX [IX_GenerationJobs_Status_LeaseUntil]", sql);
        Assert.Contains("([CompletedAt], [UserId]) INCLUDE ([Delta])", sql);
    }

    private static AppDbContext CreateDatabase() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer()
        .Options);
}
