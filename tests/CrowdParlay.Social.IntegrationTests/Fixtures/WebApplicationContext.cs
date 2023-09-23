using CrowdParlay.Social.Api;
using CrowdParlay.Social.IntegrationTests.Services;
using MassTransit.Testing;
using Nito.AsyncEx;
using Testcontainers.Neo4j;

namespace CrowdParlay.Social.IntegrationTests.Fixtures;

public class WebApplicationContext
{
    public readonly HttpClient Client;
    public readonly ITestHarness Harness;

    public WebApplicationContext()
    {
        // ReSharper disable once InconsistentNaming
        var neo4j = new Neo4jBuilder()
            .WithExposedPort(7474)
            .WithPortBinding(7474, true)
            .Build();

        AsyncContext.Run(async () => await neo4j.StartAsync());

        var webApplicationFactory = new TestWebApplicationFactory<Program>(neo4j.GetConnectionString());
        Client = webApplicationFactory.CreateClient();
        Harness = webApplicationFactory.Services.GetTestHarness();
    }
}