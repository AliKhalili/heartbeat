using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using HeartbeatTcpServer.MicroBenchmarks;

var config = DefaultConfig.Instance;
var summary = BenchmarkRunner.Run<BufferPoolBenchmarks>(config, args);