using Microsoft.Extensions.Hosting;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Services;

public class GraphClientInitialisator : IHostedService
{
    private readonly GraphClient _graphClient;

    public GraphClientInitialisator(GraphClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken) =>
        await _graphClient.ConnectAsync();

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _graphClient.Dispose();
        return Task.CompletedTask;
    }
}