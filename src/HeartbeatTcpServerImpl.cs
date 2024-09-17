using System.Net;
using System.Net.Sockets;

namespace HeartbeatServer;

public class HeartbeatTcpServerImpl : IDisposable
{
    private readonly ServerOptions _options;
    private readonly int _port;
    private readonly Socket _listenerSocket;
    private readonly ConnectionManager _connectionManager;
    private readonly Func<Memory<byte>, bool> _delegate;
    private readonly SocketAsyncEventArgsPool _socketEventArgsPool;

    public HeartbeatTcpServerImpl(string[] args)
    {
        _options = new ServerOptions(args);
        _port = _options.Port;
        _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if (_options.InlineCompletions)
        {
            SetNonBlockingSocketInlineCompletion();
        }

        _connectionManager = new ConnectionManager();
        _socketEventArgsPool = new SocketAsyncEventArgsPool(_options.SocketPolling);
        _delegate = Handler_ReadCompleted;
    }

    public void Dispose()
    {
        _listenerSocket.Dispose();
    }

    public void Start()
    {
        _listenerSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
        _listenerSocket.Listen(backlog: 4096);
        Console.WriteLine($"Server started");
        Console.WriteLine(_options.ToString());
        while (true)
        {
            var connectionId = _connectionManager.GetNewConnectionId();
            var acceptSocket = _listenerSocket.Accept();

            acceptSocket.NoDelay = true;
            var connection = new HeartbeatConnection(connectionId, acceptSocket, _connectionManager, _socketEventArgsPool, _options.MaxRequestSizeInByte, _delegate);

            _connectionManager.AddConnection(connectionId, connection);

            connection.Start();
        }
    }

    public long ActiveConnections => _connectionManager.TotalNumberOfActiveConnection;
    void SetNonBlockingSocketInlineCompletion()
    {
        Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_SOCKETS_INLINE_COMPLETIONS", "1");
    }

    bool Handler_ReadCompleted(Memory<byte> buffer)
    {
        if (Http.TryExtractDeviceId(buffer.Span, out var deviceId))
        {
            return true;
        }
        return false;
    }
}