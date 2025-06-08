using CrowdParlay.Social.Infrastructure.Communication.Services;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CrowdParlay.Social.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram>(
    MongoDbConfiguration mongoDbConfiguration,
    RedisConfiguration redisConfiguration)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = mongoDbConfiguration.ConnectionString,
                ["MongoDb:Database"] = mongoDbConfiguration.Database,
                ["REDIS_CONNECTION_STRING"] = redisConfiguration.ConnectionString,
                ["USERS_GRPC_ADDRESS"] = "https://users:5104",
                ["TELEMETRY_SOURCE_NAME"] = "Social",
                ["TELEMETRY_OTLP_EXPORTER_ENDPOINT"] = "http://localhost:8200",
                ["CORS_ORIGINS"] = "http://localhost;http://localhost:1234",
                ["DATA_PROTECTION_REDIS_CONNECTION_STRING"] = redisConfiguration.ConnectionString,
            }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IUsersService));
            services.AddScoped<IUsersService, UsersServiceMock>();
            services.Decorate<IUsersService, UsersServiceCachingDecorator>();

            var massTransitDescriptors = services
                .Where(service => service.ServiceType.Namespace?.Split('.') is [nameof(MassTransit), ..])
                .ToArray();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransitTestHarness(bus => bus.AddDelayedMessageScheduler());
        });
    }
}