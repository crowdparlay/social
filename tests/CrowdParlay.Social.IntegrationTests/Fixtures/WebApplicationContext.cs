using CrowdParlay.Social.Api;
using CrowdParlay.Social.IntegrationTests.Services;
using Microsoft.AspNetCore.TestHost;
using Neo4j.Driver;
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

        var configuration = new Neo4jConfiguration(
            Uri: neo4j.GetConnectionString(),
            Username: "neo4j",
            Password: "neo4j");

        var driver = GraphDatabase.Driver(configuration.Uri, AuthTokens.Basic(configuration.Username, configuration.Password));
        AsyncContext.Run(async () =>
        {
            await using var session = driver.AsyncSession();
            await session.RunAsync(
                """
                CREATE (discussion:Discussion {
                    Id: "6ef436dc-8e38-4a4b-b0e7-ff9fcd55ac0e",
                    Title: "Discussion about pets",
                    Description: "I like dogs and cats.",
                    CreatedAt: datetime()
                })-[:AUTHORED_BY]->(author:Author { Id: "df194a2d-368c-43ea-b48d-66042f74691d" })
                """);
        });
        
        return configuration;
    }

    private static RedisConfiguration SetupRedis()
    {
        var redis = new RedisBuilder().Build();
        AsyncContext.Run(async () => await redis.StartAsync());
        return new RedisConfiguration(redis.GetConnectionString());
    }
}