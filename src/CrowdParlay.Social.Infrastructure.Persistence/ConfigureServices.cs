using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Infrastructure.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence;

public static class ConfigurePersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration) => services
        .AddNeo4j(configuration)
        .AddScoped<ICommentRepository, CommentsRepository>()
        .AddScoped<IDiscussionsRepository, DiscussionsRepository>();

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

        var driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, password));
        return services.AddSingleton(driver);
    }
}