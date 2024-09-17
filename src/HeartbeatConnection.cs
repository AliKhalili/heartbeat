using System.Buffers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace HeartbeatServer;

internal class HeartbeatConnection
{
    private readonly long _connectionId;
    private readonly ArrayPool<byte> _bufferPool;
    private readonly Socket _socket;
    private readonly ConnectionManager _connectionManager;
    private readonly SocketAsyncEventArgsPool _socketAsyncEventArgsPool;
    private readonly int _requestMaxSize;
    private byte[] _buffer;
    private int _bytesRead;

    private readonly Func<Memory<byte>, bool> _delegate;

    public HeartbeatConnection(long connectionId, Socket socket, ConnectionManager connectionManager, SocketAsyncEventArgsPool socketAsyncEventArgsPool, int requestMaxSize, Func<Memory<byte>, bool> readDone)
    {
        _connectionId = connectionId;
        _bufferPool = ArrayPool<byte>.Shared;
        _requestMaxSize = requestMaxSize;
        _socket = socket;
        _connectionManager = connectionManager;
        _socketAsyncEventArgsPool = socketAsyncEventArgsPool;
        _delegate = readDone;
        _bytesRead = 0;
        _buffer = [];
    }

    public void Start()
    {
        var receiveEventArgs = _socketAsyncEventArgsPool.Rent();
        receiveEventArgs.AcceptSocket = _socket;
        
        _buffer = _bufferPool.Rent(_requestMaxSize);
        receiveEventArgs.SetBuffer(_buffer, 0, _requestMaxSize);
        receiveEventArgs.Completed += RecvEventArg_Completed;

        if (!_socket.ReceiveAsync(receiveEventArgs))
        {
            Task.Run(() => RecvEventArg_Completed(null, receiveEventArgs));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IncrementRead(int bytesTransferred)
    {
        _bytesRead += bytesTransferred;
        Span<byte> buffer = _buffer;
        if (Http.IsEndOfRequest(buffer.Slice(_bytesRead - 4, _bytesRead)))
            return true;
        return _bytesRead == _requestMaxSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBuffer(SocketAsyncEventArgs e) => e.SetBuffer(_buffer, _bytesRead, _requestMaxSize - _bytesRead);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadDone(SocketAsyncEventArgs e)
    {
        Memory<byte> memory = _buffer.AsMemory().Slice(0, _bytesRead);
        var readResult = _buffer.Length > 0 && _delegate.Invoke(memory);
        e.Completed -= RecvEventArg_Completed;
        e.Completed += SendEventArg_Completed;

        e.SetBuffer(readResult ? Http.OkResponse : Http.BadRequestResponse);
        if (!_socket.SendAsync(e))
        {
            SendEventArg_Completed(null, e);
        }
    }

    private void RecvEventArg_Completed(object? sender, SocketAsyncEventArgs e)
    {
        do
        {
            if (e.BytesTransferred == 0 || e.SocketError != SocketError.Success)
            {
                ReadDone(e);

                return;
            }
            if (IncrementRead(e.BytesTransferred))
            {
                ReadDone(e);

                return;
            }
            SetBuffer(e);

        } while (!e.AcceptSocket!.ReceiveAsync(e));
    }

    private void SendEventArg_Completed(object? sender, SocketAsyncEventArgs e)
    {
        _bufferPool.Return(_buffer);
        e.AcceptSocket?.Close();
        e.Completed -= SendEventArg_Completed;
        _socketAsyncEventArgsPool.Return(e);
        _connectionManager.RemoveConnection(_connectionId);
    }
}