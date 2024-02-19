using Microsoft.Extensions.Hosting;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

/// <summary>
/// Initializes a GraphClient for Neo4j database
/// </summary>
public class GraphClientInitializer : IHostedService
{
    private readonly IDriver _driver;

    public GraphClientInitializer(IDriver driver) => _driver = driver;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async runner =>
            await runner.RunAsync("CREATE CONSTRAINT unique_author_id IF NOT EXISTS FOR (a:Author) REQUIRE a.Id IS UNIQUE"));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}