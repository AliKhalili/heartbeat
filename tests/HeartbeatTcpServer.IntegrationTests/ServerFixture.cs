using HeartbeatServer;

namespace HeartbeatTcpServer.IntegrationTests;

public class ServerFixture
{
    readonly HeartbeatTcpServerImpl _server;

    public ServerFixture()
    {
        int port = 9091;
        ServerUri = new Uri($"http://127.0.0.1:{port}");
        _server = new HeartbeatTcpServerImpl([$"--{nameof(ServerOptions.Port)}={port}"]);
        Task.Run(() => _server.Start());
        //_server.StartAsync();
    }

    public Uri ServerUri { get; internal set; }

    public void Dispose()
    {
        _server?.Dispose();
    }
}