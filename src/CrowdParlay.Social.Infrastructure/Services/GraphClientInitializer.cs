using Microsoft.Extensions.Hosting;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Services;

/// <summary>
/// Initalizes a GraphClient for Neo4j database
/// </summary>
public class GraphClientInitializer : IHostedService
{
    private readonly GraphClient _graphClient;

    public GraphClientInitializer(GraphClient graphClient) => _graphClient = graphClient;

    public async Task StartAsync(CancellationToken cancellationToken) =>
        await _graphClient.ConnectAsync();

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _graphClient.Dispose();
        return Task.CompletedTask;
    }
}