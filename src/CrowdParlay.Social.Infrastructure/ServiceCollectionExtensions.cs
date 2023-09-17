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
        var uri =
            configuration["NEO4J_URI"] ??
            throw new InvalidOperationException("NEO4J_URI is not set!");

        var username =
            configuration["NEO4J_USERNAME"] ??
            throw new InvalidOperationException("NEO4J_USERNAME is not set!");

        var password =
            configuration["NEO4J_PASSWORD"] ??
            throw new InvalidOperationException("NEO4J_PASSWORD is not set!");

        return services.AddSingleton(new GraphClient(uri, username, password));
    }
}