using System.Buffers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace HeartbeatTcpServer.MicroBenchmarks;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]

public class BufferPoolBenchmarks
{
    private int N = 1000;
    private int BufferSize = 1024;

    private ArrayPool<byte>? _dotnetPool;

    [GlobalSetup]
    public void Setup()
    {
        _dotnetPool = ArrayPool<byte>.Shared;
    }


    [Benchmark(Baseline = true)]
    public void AllocateNewArray()
    {
        for (int i = 0; i < N; i++)
        {
            Task.Run(() =>
            {
                var buffer = new byte[BufferSize];
                DoArrayWrok(buffer);
            });
        }
    }

    [Benchmark]
    public void AllocateDotnetArrayPool()
    {
        
        for (int i = 0; i < N; i++)
        {
            Task.Run(() =>
            {
                var buffer = _dotnetPool!.Rent(BufferSize);
                DoArrayWrok(buffer);
                _dotnetPool.Return(buffer);
            });
        }
    }

    private void DoArrayWrok(Memory<byte> buffer)
    {
        Span<byte> span = buffer.Span;
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = (byte)i;
        }
    }
}
