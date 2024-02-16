using CrowdParlay.Social.Api;
using CrowdParlay.Social.IntegrationTests.Services;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using Testcontainers.Neo4j;

namespace CrowdParlay.Social.IntegrationTests.Fixtures;

public class WebApplicationContext
{
    public readonly HttpClient Client;
    public readonly IServiceProvider Services;

    public WebApplicationContext()
    {
        // ReSharper disable once InconsistentNaming
        var neo4j = new Neo4jBuilder()
            .WithExposedPort(7474)
            .WithPortBinding(7474, true)
            .Build();

        AsyncContext.Run(async () => await neo4j.StartAsync());

        // ReSharper disable once InconsistentNaming
        var neo4jConfiguration = new Neo4jConfiguration(
            Uri: neo4j.GetConnectionString(),
            Username: "neo4j",
            Password: "neo4j");

        var webApplicationFactory = new TestWebApplicationFactory<Program>(neo4jConfiguration);
        Client = webApplicationFactory.CreateClient();
        Services = webApplicationFactory.Services;
    }
}