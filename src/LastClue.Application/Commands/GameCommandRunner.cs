using System.Text.Json;
using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;

namespace LastClue.Application.Commands;

public sealed record GameCommandResponse(int StatusCode, JsonElement Body);

public sealed record GameCommandChange(GameCommandResponse Response, IReadOnlyList<OutboxMessage> Events);

public sealed record GameStartChange(Guid GameId, GameCommandResponse Response);

public sealed class GameCommandRunner(
    IUnitOfWork unitOfWork, IGameRepository games, IActionReceiptService receipts, TimeProvider clock)
{
    private const int MaxRetries = 2;

    /// <summary>Runs one game change. Authorize must check current access on both new and replayed requests.</summary>
    public Task<GameCommandResponse> ExecuteGameAsync(
        GameCommandRequest request,
        Func<Game, CancellationToken, Task> authorize,
        Func<Game, DateTime, long, CancellationToken, Task<GameCommandChange>> apply,
        CancellationToken cancellationToken = default)
    {
        var gameId = request.GameId ?? throw new ArgumentException("A game command requires a game ID.", nameof(request));

        return ExecuteAsync(request, async (saved, token) =>
        {
            var game = await games.GetGameForUpdateAsync(gameId, token) ?? throw new NotFoundException();
            await authorize(game, token);
            if (saved is not null)
                return (gameId, ReadResponse(saved));

            if (game.StateVersion != request.ExpectedVersion)
                throw new ConflictException(DomainErrorCode.PhaseChanged, game.StateVersion);

            var nextVersion = checked(game.StateVersion + 1);
            var now = clock.GetUtcNow().UtcDateTime;
            var change = await apply(game, now, nextVersion, token);
            game.StateVersion = nextVersion;
            foreach (var message in change.Events)
            {
                message.RoomId = null;
                message.GameId = game.Id;
                message.StateVersion = nextVersion;
                message.CreatedAt = now;
                games.AddOutboxMessage(message);
            }

            return (gameId, change.Response);
        }, cancellationToken);
    }

    /// <summary>Runs room startup writes. The handler locks the room, rechecks start rules and stages the game and events.</summary>
    public Task<GameCommandResponse> ExecuteStartAsync(
        GameCommandRequest request,
        Func<Guid?, CancellationToken, Task> authorize,
        Func<DateTime, CancellationToken, Task<GameStartChange>> start,
        CancellationToken cancellationToken = default)
    {
        if (request.GameId is not null)
            throw new ArgumentException("A start command creates a new game.", nameof(request));

        return ExecuteAsync(request, async (saved, token) =>
        {
            await authorize(saved?.GameId, token);
            if (saved is not null)
                return (saved.GameId, ReadResponse(saved));

            var change = await start(clock.GetUtcNow().UtcDateTime, token);
            if (change.GameId == Guid.Empty)
                throw new InvalidOperationException("A start command must return its new game ID.");

            return (change.GameId, change.Response);
        }, cancellationToken);
    }

    private async Task<GameCommandResponse> ExecuteAsync(
        GameCommandRequest request,
        Func<ActionReceipt?, CancellationToken, Task<(Guid? GameId, GameCommandResponse Response)>> execute,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var saved = await receipts.TryGetAsync(
                    request.UserId, request.RequestId, request.PayloadHash, cancellationToken);
                var result = await execute(saved, cancellationToken);
                if (saved is null)
                {
                    if (result.Response.StatusCode is < 200 or >= 300 || result.Response.Body.ValueKind == JsonValueKind.Undefined)
                        throw new InvalidOperationException("A completed game command must return a successful JSON response.");

                    receipts.Add(new ActionReceipt
                    {
                        UserId = request.UserId,
                        RequestId = request.RequestId,
                        GameId = result.GameId,
                        PayloadHash = request.PayloadHash,
                        ResponseJson = JsonSerializer.Serialize(result.Response),
                        CreatedAt = clock.GetUtcNow().UtcDateTime
                    });
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }
                await unitOfWork.CommitAsync(cancellationToken);
                return result.Response;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxRetries)
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
    }

    private static GameCommandResponse ReadResponse(ActionReceipt receipt) =>
        JsonSerializer.Deserialize<GameCommandResponse>(receipt.ResponseJson)
        ?? throw new InvalidOperationException("The stored command response is invalid.");
}
