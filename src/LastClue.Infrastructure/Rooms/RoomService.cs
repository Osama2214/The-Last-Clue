using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;
using LastClue.Application.Rooms;
using LastClue.Application.Security;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Rooms;

public sealed record RoomOptions(int CodeLength);

internal sealed class RoomService(
    AppDbContext database, IRoomRepository rooms, IUnitOfWork unitOfWork, IGameAuthorization authorization,
    IRoomCodeGenerator codes, RoomOptions options, TimeProvider clock, LobbyPresence presence) : IRoomService
{
    private const int CodeAttempts = 8;
    private const int Capacity = 8;

    public async Task<LobbyView> CreateAsync(string userId, string language, CancellationToken cancellationToken)
    {
        language = InputRules.Trim(language).ToLowerInvariant();
        if (language is not ("en" or "ar"))
            throw new ValidationException();

        await authorization.RequirePlayerAccountAsync(userId, cancellationToken);
        for (var attempt = 0; attempt < CodeAttempts; attempt++)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            var now = clock.GetUtcNow().UtcDateTime;
            var room = new Room
            {
                Id = Guid.NewGuid(), Code = codes.Generate(options.CodeLength), HostUserId = userId,
                Language = language, Status = RoomStatus.Lobby, CreatedAt = now
            };

            try
            {
                await RequireNoActiveMembershipAsync(userId, cancellationToken);
                rooms.AddRoom(room);
                rooms.AddRoomMember(NewMember(room, userId, now));
                return await SaveLobbyAsync(room, now, cancellationToken);
            }
            catch (DuplicateException)
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
                await RequireNoActiveMembershipAsync(userId, cancellationToken);
                if (!await database.Rooms.AnyAsync(candidate => candidate.Code == room.Code, cancellationToken))
                    throw;
            }
            catch
            {
                await unitOfWork.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        throw new ConcurrencyConflictException();
    }

    public async Task<LobbyView> JoinAsync(string userId, string code, CancellationToken cancellationToken)
    {
        code = InputRules.Trim(code).ToUpperInvariant();
        if (!RoomCodeGenerator.IsValid(code))
            throw new NotFoundException();

        await authorization.RequirePlayerAccountAsync(userId, cancellationToken);
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var room = await database.Rooms
                .FromSql($"SELECT * FROM [dbo].[Rooms] WITH (UPDLOCK, HOLDLOCK) WHERE [Code] = {code}")
                .Include(candidate => candidate.Members)
                .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
            if (room.Status != RoomStatus.Lobby)
                throw new ConflictException(DomainErrorCode.RoomNotOpen);

            await RequireNoActiveMembershipAsync(userId, cancellationToken);
            if (room.Members.Count(member => member.LeftAt is null) >= Capacity)
                throw new ConflictException(DomainErrorCode.RoomFull);

            var now = clock.GetUtcNow().UtcDateTime;
            var returning = room.Members.SingleOrDefault(member => member.UserId == userId);
            if (returning is null)
                rooms.AddRoomMember(NewMember(room, userId, now));
            else
            {
                returning.LeftAt = null;
                returning.JoinedAt = now;
            }

            foreach (var member in room.Members.Where(member => member.LeftAt is null))
                member.IsReady = false;

            return await SaveLobbyAsync(room, now, cancellationToken);
        }
        catch (DuplicateException)
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            await RequireNoActiveMembershipAsync(userId, cancellationToken);
            throw;
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<LobbyView> GetAsync(Guid roomId, string userId, CancellationToken cancellationToken)
    {
        var room = await database.Rooms.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == roomId &&
            candidate.Members.Any(member => member.UserId == userId && member.LeftAt == null),
            cancellationToken) ?? throw new NotFoundException();
        return await ReadLobbyAsync(room, cancellationToken);
    }

    public Task<LobbyView> SetReadyAsync(Guid roomId, string userId, bool ready, CancellationToken cancellationToken)
        => UpdateLobbyAsync(roomId, userId, (room, member, _) =>
        {
            var changed = member.IsReady != ready;
            member.IsReady = ready;
            return changed;
        }, cancellationToken);

    public async Task LeaveAsync(Guid roomId, string userId, CancellationToken cancellationToken)
    {
        await UpdateLobbyAsync(roomId, userId, (room, member, now) =>
        {
            member.LeftAt = now;
            var remaining = room.Members.Where(candidate => candidate.LeftAt == null).ToArray();
            foreach (var candidate in remaining)
                candidate.IsReady = false;

            if (remaining.Length == 0)
            {
                room.HostUserId = null;
                room.Status = RoomStatus.Closed;
            }
            else if (room.HostUserId == userId)
                room.HostUserId = FirstConnected(room);

            return true;
        }, cancellationToken);

        presence.RemoveMember(roomId, userId);
    }

    public async Task RestoreHostAsync(Guid roomId, string userId, CancellationToken cancellationToken)
    {
        await UpdateLobbyAsync(roomId, userId, (room, _, _) =>
        {
            if (room.HostUserId is not null)
                return false;

            room.HostUserId = FirstConnected(room);
            return room.HostUserId is not null;
        }, cancellationToken, onlyIfLobby: true);
    }

    private string? FirstConnected(Room room) => room.Members
        .Where(candidate => candidate.LeftAt is null && presence.IsConnected(room.Id, candidate.UserId))
        .OrderBy(candidate => candidate.JoinedAt).ThenBy(candidate => candidate.Id)
        .Select(candidate => candidate.UserId).FirstOrDefault();

    private async Task<LobbyView> UpdateLobbyAsync(Guid roomId, string userId,
        Func<Room, RoomMember, DateTime, bool> mutate, CancellationToken cancellationToken, bool onlyIfLobby = false)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var room = await database.Rooms
                .FromSql($"SELECT * FROM [dbo].[Rooms] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {roomId}")
                .Include(candidate => candidate.Members)
                .SingleOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
            var member = room.Members.SingleOrDefault(candidate => candidate.UserId == userId && candidate.LeftAt is null)
                ?? throw new NotFoundException();

            if (room.Status != RoomStatus.Lobby && !onlyIfLobby)
                throw new ConflictException(DomainErrorCode.RoomNotOpen);

            var now = clock.GetUtcNow().UtcDateTime;
            if (room.Status == RoomStatus.Lobby && mutate(room, member, now))
                return await SaveLobbyAsync(room, now, cancellationToken);

            var view = await ReadLobbyAsync(room, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return view;
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task RequireNoActiveMembershipAsync(string userId, CancellationToken cancellationToken)
    {
        if (await database.RoomMembers.AnyAsync(member => member.UserId == userId && member.LeftAt == null,
                cancellationToken) ||
            await database.Games.AnyAsync(game => game.IsRunning && game.Players.Any(
                player => player.UserId == userId && player.DepartedAt == null), cancellationToken))
            throw new ConflictException(DomainErrorCode.AlreadyInRoom);
    }

    private async Task<LobbyView> SaveLobbyAsync(Room room, DateTime now, CancellationToken cancellationToken)
    {
        room.StateVersion++;
        database.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(), RoomId = room.Id, StateVersion = room.StateVersion, Audience = "Public",
            EventType = "LobbyUpdated", PayloadJson = "{}", CreatedAt = now
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var view = await ReadLobbyAsync(room, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return view;
    }

    private async Task<LobbyView> ReadLobbyAsync(Room room, CancellationToken cancellationToken)
    {
        var members = await (from member in database.RoomMembers
                             join user in database.Users on member.UserId equals user.Id
                             where member.RoomId == room.Id && member.LeftAt == null
                             orderby member.JoinedAt, member.Id
                             select new LobbyMemberView(user.Id, user.DisplayName, user.AvatarSeed, member.IsReady))
            .ToListAsync(cancellationToken);

        return new LobbyView(room.Id, room.Code, room.Language, room.HostUserId, room.StateVersion,
            members.Select(member => member with { IsConnected = presence.IsConnected(room.Id, member.UserId) }).ToArray());
    }

    private static RoomMember NewMember(Room room, string userId, DateTime now) =>
        new() { Id = Guid.NewGuid(), RoomId = room.Id, UserId = userId, JoinedAt = now };
}
