using CrowdParlay.Social.IntegrationTests.Services;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Nito.AsyncEx;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Social.IntegrationTests.Fixtures;

public class WebApplicationContext
{
    public readonly HttpClient Client;

    public WebApplicationContext()
    {
        var postgres = new PostgreSqlBuilder()
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .Build();

        AsyncContext.Run(async () => await postgres.StartAsync());

        var webApplicationFactory = new TestWebApplicationFactory<Program>(postgres.GetConnectionString());
        Client = webApplicationFactory.CreateClient();
    }
}