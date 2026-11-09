using LastClue.Application.Domain;

namespace LastClue.Application.Interfaces;

public interface IRoomRepository
{
    /// <summary>Loads a tracked room and its memberships by its join code.</summary>
    Task<Room?> GetRoomByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Tracks a new room for insertion.</summary>
    void AddRoom(Room room);

    /// <summary>Tracks a new room membership for insertion.</summary>
    void AddRoomMember(RoomMember member);
}
