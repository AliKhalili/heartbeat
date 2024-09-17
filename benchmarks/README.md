# Benchmark

## Bombardier

```
bombardier -c 32 -m PUT -H "Device-Id:1234" --http1 -a  192.168.0.109:9096
bombardier -c 32 -m PUT -H "Device-Id:1234" --http1 -a  192.168.0.109:9097
```



```
bombardier -c 32 -m PUT -H "Device-Id:1234" --http1 -a  127.0.0.1:9096
bombardier -c 32 -m PUT -H "Device-Id:1234" --http1 -a  127.0.0.1:9097
```

```

dotnet run -c Release --InlineCompletions false --SocketPolling false

.\run.bat win_heartbeat_pool 16 127.0.0.1:9096
.\run.bat win_heartbeat_pool_inline 16 127.0.0.1:9096
.\run.bat win_baseline 16 127.0.0.1:9097
.\run.bat win_heartbeat 16 127.0.0.1:9096


./run.sh unix_heartbeat 16 127.0.0.1:9096
./run.sh unix_heartbeat_pool 16 127.0.0.1:9096
./run.sh unix_heartbeat_pool_inline 16 127.0.0.1:9096
./run.sh unix_baseline 16 127.0.0.1:9096
```