namespace LastClue.Application.Rooms;

public interface IRoomService
{
    Task<LobbyView> CreateAsync(string userId, string language, CancellationToken cancellationToken);
    Task<LobbyView> JoinAsync(string userId, string code, CancellationToken cancellationToken);
    Task<LobbyView> GetAsync(Guid roomId, string userId, CancellationToken cancellationToken);
    Task<LobbyView> SetReadyAsync(Guid roomId, string userId, bool ready, CancellationToken cancellationToken);
    Task LeaveAsync(Guid roomId, string userId, CancellationToken cancellationToken);
    Task RestoreHostAsync(Guid roomId, string userId, CancellationToken cancellationToken);
}

public sealed record LobbyView(Guid RoomId, string Code, string Language, string? Host,
    long StateVersion, IReadOnlyList<LobbyMemberView> Members);

public sealed record LobbyMemberView(string UserId, string DisplayName, string AvatarSeed, bool IsReady, bool IsConnected = false);
