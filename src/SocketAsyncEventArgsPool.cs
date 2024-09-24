using System.Collections.Concurrent;
using System.Net.Sockets;

namespace HeartbeatServer;
class SocketAsyncEventArgsPool : IDisposable
{
    private const int MaxQueueSize = 1024; // REVIEW: Is this good enough?
    private readonly bool _enabled;

    public SocketAsyncEventArgsPool(bool enabled) => _enabled = enabled;

    private readonly ConcurrentQueue<SocketAsyncEventArgs> _queue = new();
    private int _count;
    private bool _disposed;

    public SocketAsyncEventArgs Rent()
    {
        if (_enabled && _queue.TryDequeue(out var sender))
        {
            Interlocked.Decrement(ref _count);
            return sender;
        }
        return new SocketAsyncEventArgs(unsafeSuppressExecutionContextFlow:true);
    }

    public void Return(SocketAsyncEventArgs sender)
    {
        if (!_enabled || _disposed || Interlocked.Increment(ref _count) > MaxQueueSize)
        {
            Interlocked.Decrement(ref _count);
            sender.Dispose();
            return;
        }

        _queue.Enqueue(sender);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            while (_queue.TryDequeue(out var sender))
            {
                sender.Dispose();
            }
        }
    }
}