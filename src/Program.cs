using HeartbeatServer;

using var server = new HeartbeatTcpServerImpl(args);
try
{
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
catch (Exception ex)
{
    server.Dispose();
    Console.WriteLine($"Unable to initialize server due to exception: {ex.Message}");
}