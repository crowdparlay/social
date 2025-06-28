using System.Diagnostics;
using CrowdParlay.Social.Infrastructure.Communication.Services;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SimpleActivityExportProcessor = OpenTelemetry.SimpleActivityExportProcessor;

namespace CrowdParlay.Social.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram>(
    MongoDbConfiguration mongoDbConfiguration,
    RedisConfiguration redisConfiguration)
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MongoDb:ConnectionString"] = mongoDbConfiguration.ConnectionString,
            ["MongoDb:Database"] = mongoDbConfiguration.Database,
            ["OpenTelemetry:ServiceName"] = "social",
            ["OpenTelemetry:OtlpEndpoint"] = "http://localhost:8200",
            ["OpenTelemetry:AdditionalTraceSources"] = "CrowdParlay.Social.*",
            ["OpenTelemetry:MeterName"] = "CrowdParlay.Social.Metrics",
            ["REDIS_CONNECTION_STRING"] = redisConfiguration.ConnectionString,
            ["USERS_GRPC_ADDRESS"] = "https://users:5104",
            ["CORS_ORIGINS"] = "http://localhost;http://localhost:1234",
            ["DATA_PROTECTION_REDIS_CONNECTION_STRING"] = redisConfiguration.ConnectionString,
        }));

        builder.ConfigureServices(ConfigureMockServices);
        builder.ConfigureServices(ConfigureOpenTelemetryInMemoryExporter);
        builder.ConfigureServices(ConfigureRabbitMqTestHarness);
    }

    private static void ConfigureMockServices(IServiceCollection services)
    {
        services.RemoveAll(typeof(IUsersService));
        services.AddScoped<IUsersService, UsersServiceMock>();
        services.Decorate<IUsersService, UsersServiceCachingDecorator>();
    }

    private static void ConfigureOpenTelemetryInMemoryExporter(IServiceCollection services)
    {
        var storage = new OpenTelemetryInMemoryStorage();
        services.AddSingleton(storage);

        var openTelemetryBuilder = services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
        {
            var inMemoryExporter = new InMemoryExporter<Activity>(storage.Activities);
            tracerProviderBuilder.AddProcessor(new SimpleActivityExportProcessor(inMemoryExporter));
            services.AddSingleton(inMemoryExporter);
        });

        openTelemetryBuilder.WithMetrics(meterProviderBuilder =>
        {
            var inMemoryExporter = new InMemoryExporter<Metric>(storage.Metrics);
            meterProviderBuilder.AddReader(new BaseExportingMetricReader(inMemoryExporter));
            services.AddSingleton(inMemoryExporter);
        });
    }

    private static void ConfigureRabbitMqTestHarness(IServiceCollection services)
    {
        var massTransitDescriptors = services
            .Where(service => service.ServiceType.Namespace?.Split('.') is [nameof(MassTransit), ..])
            .ToArray();

        foreach (var descriptor in massTransitDescriptors)
            services.Remove(descriptor);

        services.AddMassTransitTestHarness(bus => bus.AddDelayedMessageScheduler());
    }
}