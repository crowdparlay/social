using CrowdParlay.Social.Api;
using CrowdParlay.Social.IntegrationTests.Services;
using Microsoft.AspNetCore.TestHost;
using Nito.AsyncEx;
using Testcontainers.Neo4j;
using Testcontainers.Redis;

namespace CrowdParlay.Social.IntegrationTests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class WebApplicationContext
{
    public readonly IServiceProvider Services;
    public readonly TestServer Server;

    public WebApplicationContext()
    {
        // ReSharper disable once InconsistentNaming
        var neo4jConfiguration = SetupNeo4j();
        var redisConfiguration = SetupRedis();

        var webApplicationFactory = new TestWebApplicationFactory<Program>(neo4jConfiguration, redisConfiguration);

        Services = webApplicationFactory.Services;
        Server = webApplicationFactory.Server;
    }

    // ReSharper disable once InconsistentNaming
    private static Neo4jConfiguration SetupNeo4j()
    {
        // ReSharper disable once InconsistentNaming
        var neo4j = new Neo4jBuilder()
            .WithExposedPort(7474)
            .WithPortBinding(7474, true)
            .Build();

        AsyncContext.Run(async () => await neo4j.StartAsync());

        return new Neo4jConfiguration(
            Uri: neo4j.GetConnectionString(),
            Username: "neo4j",
            Password: "neo4j");
    }

    private static RedisConfiguration SetupRedis()
    {
        var redis = new RedisBuilder().Build();
        AsyncContext.Run(async () => await redis.StartAsync());
        return new RedisConfiguration(redis.GetConnectionString());
    }
}