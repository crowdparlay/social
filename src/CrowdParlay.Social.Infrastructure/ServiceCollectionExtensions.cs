using CrowdParlay.Social.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services
        .AddNeo4j(configuration)
        .AddHostedService<GraphClientInitializer>();

    // ReSharper disable once InconsistentNaming
    private static IServiceCollection AddNeo4j(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration["NEO4J_CONNECTION_STRING"] ??
            throw new InvalidOperationException("NEO4J_CONNECTION_STRING is not set!");

        return services.AddSingleton<IGraphClient>(new BoltGraphClient(connectionString));
    }
}