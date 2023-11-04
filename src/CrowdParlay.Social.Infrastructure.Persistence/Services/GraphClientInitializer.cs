using Microsoft.Extensions.Hosting;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

/// <summary>
/// Initializes a GraphClient for Neo4j database
/// </summary>
public class GraphClientInitializer : IHostedService
{
    private readonly IGraphClient _graphClient;

    public GraphClientInitializer(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _graphClient.ConnectAsync();
        await _graphClient.Cypher
            .Create("CONSTRAINT IF NOT EXISTS unique_author_id ON (a:Author) ASSERT a.Id IS UNIQUE")
            .ExecuteWithoutResultsAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _graphClient.Dispose();
        return Task.CompletedTask;
    }
}