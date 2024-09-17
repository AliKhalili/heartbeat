using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace HeartbeatServer;
public sealed class ServerOptions
{
    public int Port { get; set; } = 9096;
    public string Address { get; set; } = "127.0.0.1";
    public int MaxRequestSizeInByte { get; set; } = 512;
    public bool InlineCompletions { get; set; } = false;
    public bool SocketPolling { get; set; } = false;

    public ServerOptions(string[] args)
    {
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();


        configuration.Bind(this);
    }

    public override string ToString()
    {
        return $"ServerOptions: Port={Port}, Address={Address}, MaxRequestSizeInByte={MaxRequestSizeInByte}, InlineCompletions={InlineCompletions}, SocketPolling={SocketPolling}";
    }
}