using System.Text.Json;
using LastClue.Application.Commands;
using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure.Tests;

public sealed class GameCommandSqlTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Concurrent_replays_save_one_change_and_recheck_access_before_returning_it()
    {
        var (gameId, userId) = await CreateGameAsync();
        var key = Guid.NewGuid();
        var command = GameCommandRequest.Create(userId, key, "statement", gameId, 0, new { text = "A clue", round = 1 });
        var reordered = GameCommandRequest.Create(userId, key, "statement", gameId, 0, new { round = 1, text = "A clue" });

        var responses = await Task.WhenAll(ExecuteAsync(command), ExecuteAsync(reordered));
        Assert.All(responses, response =>
        {
            Assert.Equal(201, response.StatusCode);
            Assert.Equal(1, response.Body.GetProperty("version").GetInt64());
        });

        var changed = GameCommandRequest.Create(userId, key, "statement", gameId, 0, new { round = 1, text = "Changed" });
        var mismatch = await Assert.ThrowsAsync<ConflictException>(() => ExecuteAsync(changed));
        Assert.Equal(DomainErrorCode.ReplayMismatch, mismatch.Code);

        var stale = GameCommandRequest.Create(userId, Guid.NewGuid(), "statement", gameId, 0, new { round = 1, text = "Another" });
        var conflict = await Assert.ThrowsAsync<ConflictException>(() => ExecuteAsync(stale));
        Assert.Equal(DomainErrorCode.PhaseChanged, conflict.Code);
        Assert.Equal(1, conflict.CurrentStateVersion);

        await using (var scope = fixture.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(1, (await database.Games.SingleAsync(game => game.Id == gameId)).StateVersion);
            Assert.Equal(1, await database.Statements.CountAsync(statement => statement.GameId == gameId));
            Assert.Equal(1, await database.ActionReceipts.CountAsync(receipt => receipt.GameId == gameId));
            Assert.Equal(1, await database.OutboxMessages.CountAsync(message => message.GameId == gameId));
            var player = await database.GamePlayers.SingleAsync(player => player.GameId == gameId && player.UserId == userId);
            player.IsActive = false;
            await database.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<ForbiddenException>(() => ExecuteAsync(command));
    }

    private async Task<GameCommandResponse> ExecuteAsync(GameCommandRequest command)
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var runner = scope.ServiceProvider.GetRequiredService<GameCommandRunner>();
        return await runner.ExecuteGameAsync(command,
            (game, _) => game.Players.Any(player => player.UserId == command.UserId && player.IsActive)
                ? Task.CompletedTask
                : throw new ForbiddenException(),
            (game, now, nextVersion, _) =>
            {
                database.Statements.Add(new Statement
                {
                    GameId = game.Id, Round = 1, AuthorId = command.UserId, Text = "A clue", CreatedAt = now
                });
                return Task.FromResult(new GameCommandChange(
                    new GameCommandResponse(201, JsonSerializer.SerializeToElement(new { version = nextVersion })),
                    [new OutboxMessage { Audience = "Public", EventType = "StateChanged", PayloadJson = "{}" }]));
            });
    }

    private async Task<(Guid GameId, string UserId)> CreateGameAsync()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new AppUser { DisplayName = "Command player", CreatedAt = DateTime.UtcNow };
        var room = new Room { Id = Guid.NewGuid(), Code = "command-test", Language = "en", CreatedAt = DateTime.UtcNow };
        var settings = new SettingsVersion
        {
            Id = Guid.NewGuid(), VersionNo = 1, ValuesJson = "{}", ChangedBy = user.Id, PublishedAt = DateTime.UtcNow
        };
        var game = new Game
        {
            Id = Guid.NewGuid(), RoomId = room.Id, SettingsVersionId = settings.Id, Number = 1,
            Status = GameStatus.Active, Phase = Phase.Statements, Round = 1, SettingsJson = "{}", CreatedAt = DateTime.UtcNow
        };

        database.Users.Add(user);
        database.Rooms.Add(room);
        database.SettingsVersions.Add(settings);
        database.Games.Add(game);
        database.GamePlayers.Add(new GamePlayer { GameId = game.Id, UserId = user.Id, IsActive = true });
        await database.SaveChangesAsync();
        return (game.Id, user.Id);
    }
}
