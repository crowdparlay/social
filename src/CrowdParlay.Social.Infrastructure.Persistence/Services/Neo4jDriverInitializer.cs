using Microsoft.Extensions.Hosting;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

// ReSharper disable once InconsistentNaming
public class Neo4jDriverInitializer(IDriver driver) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var session = driver.AsyncSession();
        await session.ExecuteWriteAsync(async runner =>
            await runner.RunAsync("CREATE CONSTRAINT unique_author_id IF NOT EXISTS FOR (author:Author) REQUIRE author.Id IS UNIQUE"));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}