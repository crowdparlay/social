using CrowdParlay.Social.Api.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Social.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // ReSharper disable once InconsistentNaming
    private readonly Neo4jConfiguration _neo4jConfiguration;

    // ReSharper disable once InconsistentNaming
    public TestWebApplicationFactory(Neo4jConfiguration neo4jConfiguration) =>
        _neo4jConfiguration = neo4jConfiguration;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["NEO4J_URI"] = _neo4jConfiguration.Uri,
            ["NEO4J_USERNAME"] = _neo4jConfiguration.Username,
            ["NEO4J_PASSWORD"] = _neo4jConfiguration.Password
        }));

        builder.ConfigureServices(services =>
        {
            var massTransitDescriptors = services
                .Where(x => x.ServiceType.Namespace?.StartsWith(nameof(MassTransit)) == true)
                .ToArray();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransitTestHarness(bus =>
            {
                bus.AddDelayedMessageScheduler();
                bus.AddConsumersFromNamespaceContaining<UserEventConsumer>();
            });
        });
    }
}