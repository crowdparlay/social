using CrowdParlay.Social.Api.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Social.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram>(
    // ReSharper disable once InconsistentNaming
    Neo4jConfiguration neo4jConfiguration,
    RedisConfiguration redisConfiguration)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["NEO4J_URI"] = neo4jConfiguration.Uri,
            ["NEO4J_USERNAME"] = neo4jConfiguration.Username,
            ["NEO4J_PASSWORD"] = neo4jConfiguration.Password,
            ["REDIS_CONNECTION_STRING"] = redisConfiguration.ConnectionString
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