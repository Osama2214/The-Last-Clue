using LastClue.Application.Domain;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class RoomRepository(AppDbContext database) : IRoomRepository
{
    public Task<Room?> GetRoomByCodeAsync(string code, CancellationToken cancellationToken) =>
        database.Rooms.Include(room => room.Members)
            .SingleOrDefaultAsync(room => room.Code == code, cancellationToken);

    public void AddRoom(Room room) => database.Rooms.Add(room);

    public void AddRoomMember(RoomMember member) => database.RoomMembers.Add(member);
}
