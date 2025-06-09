using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Infrastructure.Persistence.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence;

public static class ConfigurePersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration) => services
        .AddMongoDb(configuration)
        .AddScoped<ICommentsRepository, CommentsRepository>()
        .AddScoped<IDiscussionsRepository, DiscussionsRepository>()
        .AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>()
        .AddHostedService<StartupConfigurator>()
        .AddHostedService<DatabaseInitializer>();

    private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<MongoDbSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Bind(configuration.GetSection("MongoDb"));

        services.AddSingleton<IMongoClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>();
            return new MongoClient(settings.Value.ConnectionString);
        });

        services.AddScoped<IClientSessionHandle>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.StartSession();
        });

        return services.AddScoped<IMongoDatabase>(provider =>
        {
            var session = provider.GetRequiredService<IClientSessionHandle>();
            var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>();
            return session.Client.GetDatabase(settings.Value.Database);
        });
    }
}