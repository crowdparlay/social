using CrowdParlay.Social.Application.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Social.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // ReSharper disable once InconsistentNaming
    private readonly string _neo4jConnectionString;

    // ReSharper disable once InconsistentNaming
    public TestWebApplicationFactory(string neo4jConnectionString) =>
        _neo4jConnectionString = neo4jConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["NEO4J_CONNECTION_STRING"] = _neo4jConnectionString
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