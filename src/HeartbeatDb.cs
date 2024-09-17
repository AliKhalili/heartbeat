using System.Collections.Concurrent;
using System.Text;

namespace HeartbeatServer;

internal class HeartbeatDb
{
    private readonly ConcurrentDictionary<int, DateTimeOffset> _db = new();

    public void Update(int key, DateTimeOffset value)
    {
        _db.AddOrUpdate(key, addValue: value, updateValueFactory: (int key, DateTimeOffset oldValue) => value);
    }

    public void StartReporting(HeartbeatTcpServerImpl server, TimeSpan deadIntervalThreshold, CancellationToken cancellation)
    {
        Task.Run(() =>
        {
            var output = new StringBuilder();
            while (!cancellation.IsCancellationRequested)
            {
                Task.Delay(deadIntervalThreshold).Wait();


                var utcNow = DateTimeOffset.UtcNow;
                output.AppendLine($"Report {utcNow:T}, Active Connection: {server.ActiveConnections}");

                var deadEntries = _db?.Where(x => utcNow - x.Value > deadIntervalThreshold).Select(x => x.Key.ToString()).ToHashSet();
                var numOfDead = deadEntries?.Count ?? 0;
                var numOfActive = (_db?.Count() ?? 0) - numOfDead;
                output.AppendLine($"number of active devices:{numOfActive}, number of dead devives: {numOfDead}");
                //if (numOfDead > 0)
                //{
                //    output.AppendLine(deadEntries?.Aggregate((c, n) => $"{c}, {n}"));
                //}

                Console.WriteLine(output.ToString());
                output.Clear();
            }
        });
    }
}