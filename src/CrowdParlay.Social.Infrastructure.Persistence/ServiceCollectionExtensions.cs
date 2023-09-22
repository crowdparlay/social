using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Infrastructure.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration) => services
        .AddNeo4j(configuration)
        .AddHostedService<GraphClientInitializer>()
        .AddScoped<IAuthorRepository, AuthorRepository>()
        .AddScoped<ICommentRepository, CommentRepository>();

    // ReSharper disable once InconsistentNaming
    private static IServiceCollection AddNeo4j(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration["NEO4J_CONNECTION_STRING"] ??
            throw new InvalidOperationException("NEO4J_CONNECTION_STRING is not set!");

        return services.AddSingleton<IGraphClient>(new BoltGraphClient(connectionString));
    }
}