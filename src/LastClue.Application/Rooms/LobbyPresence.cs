namespace LastClue.Application.Rooms;

public sealed class LobbyPresence
{
    private readonly Dictionary<(Guid RoomId, string UserId), HashSet<string>> _connections = [];
    private readonly Lock _gate = new();

    public void Connect(Guid roomId, string userId, string connectionId)
    {
        lock (_gate)
        {
            var key = (roomId, userId);
            if (!_connections.TryGetValue(key, out var connections))
                _connections[key] = connections = [];

            connections.Add(connectionId);
        }
    }

    public void Disconnect(Guid roomId, string userId, string connectionId)
    {
        lock (_gate)
        {
            var key = (roomId, userId);
            if (_connections.TryGetValue(key, out var connections) &&
                connections.Remove(connectionId) && connections.Count == 0)
                _connections.Remove(key);
        }
    }

    public bool IsConnected(Guid roomId, string userId)
    {
        lock (_gate)
            return _connections.ContainsKey((roomId, userId));
    }

    public void RemoveMember(Guid roomId, string userId)
    {
        lock (_gate)
            _connections.Remove((roomId, userId));
    }
}
