using System.Net;

namespace HeartbeatTcpServer.IntegrationTests;

public class HearbeatTests : IClassFixture<ServerFixture>
{
    private readonly ServerFixture _fixture;

    public HearbeatTests(ServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Server_Accept_A_New_Heartbeat_Update()
    {
        // Arrange
        int deviceId = new Random().Next(1000);
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_fixture.ServerUri}");
        request.Headers.Add("Device-Id", deviceId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public async Task Server_Accept_Multiple_New_Heartbeat_Update(int totalRequests)
    {
        // Arrange
        ManualResetEventSlim waiter = new();
        Task<HttpStatusCode> NewRequest(int reqId)
        {
            return Task.Run(async () =>
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_fixture.ServerUri}");
                request.Headers.Add("Device-Id", reqId.ToString());

                waiter.Wait();
                var response = await client.SendAsync(request);
                return response.StatusCode;
            });
        }


        // Act
        var requests = Enumerable.Range(0, totalRequests).Select(i => NewRequest(i)).ToArray();
        waiter.Set();
        await Task.WhenAll(requests);

        // Assert
        foreach (var request in requests)
        {
            var actualStatusCode = await request;
            Assert.Equal(HttpStatusCode.NoContent, actualStatusCode);
        }
    }
}