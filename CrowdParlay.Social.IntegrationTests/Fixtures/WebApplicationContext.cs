using CrowdParlay.Social.IntegrationTests.Services;
using MassTransit.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
        var neo4jConfiguration = new Neo4jTestConfiguration
        {
            Username = "neo4j",
            Password = "neo4j_password",
            Port = 7474
        };
        
        // ReSharper disable once InconsistentNaming
        var neo4j = new Neo4jBuilder()
            .WithEnvironment("NEO4J_USERNAME", neo4jConfiguration.Username)
            .WithEnvironment("NEO4J_PASSWORD", neo4jConfiguration.Password)
            .WithExposedPort(neo4jConfiguration.Port)
            .WithPortBinding(neo4jConfiguration.Port, true)
            .Build();

        AsyncContext.Run(async () => await neo4j.StartAsync());

        var webApplicationFactory = new TestWebApplicationFactory<Program>(neo4jConfiguration);
        Client = webApplicationFactory.CreateClient();
        Harness = webApplicationFactory.Services.GetTestHarness();
    }
}