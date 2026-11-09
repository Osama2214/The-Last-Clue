using LastClue.Application.Exceptions;
using LastClue.Application.Rooms;
using LastClue.Application.Security;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Web.Realtime;

[Authorize(Policy = "PlayerOnly")]
public sealed class GameHub(
    IRoomService rooms, LobbyPresence presence, AppDbContext database,
    IGameAuthorization gameAuthorization, SignInManager<AppUser> signIn, TimeProvider clock) : Hub
{
    private static readonly object SubscribedRoom = new();
    private static readonly object SubscribedGame = new();

    public async Task JoinRoom(Guid roomId)
    {
        var userId = await GetUserIdAsync();
        try
        {
            await rooms.GetAsync(roomId, userId, Context.ConnectionAborted);
            if (Context.Items.TryGetValue(SubscribedRoom, out var current) && !Equals(current, roomId))
                throw new HubException("This connection is already subscribed to another room.");

            presence.Connect(roomId, userId, Context.ConnectionId);
            await rooms.RestoreHostAsync(roomId, userId, Context.ConnectionAborted);
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomId), Context.ConnectionAborted);
            Context.Items[SubscribedRoom] = roomId;
        }
        catch (NotFoundException)
        {
            presence.Disconnect(roomId, userId, Context.ConnectionId);
            throw new HubException("Room not found.");
        }
        catch
        {
            presence.Disconnect(roomId, userId, Context.ConnectionId);
            throw;
        }
    }

    public async Task JoinGame(Guid gameId)
    {
        var userId = await GetUserIdAsync();
        var game = await database.Games.AsNoTracking()
            .Include(candidate => candidate.Players.Where(player => player.UserId == userId))
            .SingleOrDefaultAsync(candidate => candidate.Id == gameId, Context.ConnectionAborted);

        LastClue.Application.Domain.GamePlayer player;
        try
        {
            player = gameAuthorization.RequireMember(game, userId);
        }
        catch (NotFoundException)
        {
            throw new HubException("Game not found.");
        }

        if (player.DepartedAt is not null)
            throw new HubException("Game not found.");

        if (Context.Items.TryGetValue(SubscribedGame, out var current))
        {
            if (current is Guid currentId && currentId == gameId)
                return;
            throw new HubException("This connection is already subscribed to another game.");
        }

        database.PlayerConnections.Add(new()
        {
            ConnectionId = Context.ConnectionId, GameId = gameId, UserId = userId,
            LastSeenAt = clock.GetUtcNow().UtcDateTime
        });
        await database.SaveChangesAsync(Context.ConnectionAborted);
        Context.Items[SubscribedGame] = gameId;

        await Groups.AddToGroupAsync(Context.ConnectionId, GameGroup(gameId), Context.ConnectionAborted);
        if (player.IsActive && !player.IsArrested)
            await Groups.AddToGroupAsync(Context.ConnectionId, PrivateGroup(gameId, userId), Context.ConnectionAborted);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is { } userId &&
            Context.Items.TryGetValue(SubscribedRoom, out var current) && current is Guid roomId)
            presence.Disconnect(roomId, userId, Context.ConnectionId);

        if (Context.Items.ContainsKey(SubscribedGame))
        {
            await database.PlayerConnections.Where(connection =>
                    connection.ConnectionId == Context.ConnectionId && connection.ClosedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(connection => connection.ClosedAt,
                    clock.GetUtcNow().UtcDateTime));
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<string> GetUserIdAsync()
    {
        var user = await signIn.ValidateSecurityStampAsync(Context.User);
        if (user is { IsBlocked: false })
            return user.Id;

        Context.Abort();
        throw new HubException("Sign in to continue.");
    }

    public static string RoomGroup(Guid roomId) => $"room:{roomId:D}";
    public static string GameGroup(Guid gameId) => $"game:{gameId:D}";
    public static string PrivateGroup(Guid gameId, string userId) => $"user:{gameId:D}:{userId}";
}
