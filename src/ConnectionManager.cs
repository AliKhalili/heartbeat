using System.Collections.Concurrent;

namespace HeartbeatServer;

internal class ConnectionManager
{
    private readonly ConcurrentDictionary<long, HeartbeatConnection> _connectionReferences = new();
    private long _lastConnectionId = 0;
    public long GetNewConnectionId() => Interlocked.Increment(ref _lastConnectionId);
    public long TotalNumberOfActiveConnection => _connectionReferences.Count;

    public void AddConnection(long id, HeartbeatConnection connection)
    {
        if (!_connectionReferences.TryAdd(id, connection))
        {
            throw new ArgumentException("Unable to add connection.", nameof(id));
        }
    }

    public void RemoveConnection(long id)
    {
        if (!_connectionReferences.TryRemove(id, out var reference))
        {
            throw new ArgumentException("Unable to remove connection.", nameof(id));
        }
    }
}