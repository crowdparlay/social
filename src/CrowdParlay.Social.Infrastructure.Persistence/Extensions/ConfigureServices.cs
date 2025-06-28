using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Infrastructure.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services) => services
        .AddMongoDb()
        .AddScoped<ICommentsRepository, CommentsRepository>()
        .AddScoped<IDiscussionsRepository, DiscussionsRepository>()
        .AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>()
        .AddHostedService<StartupConfigurator>()
        .AddHostedService<DatabaseInitializer>();

    private static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        services
            .AddOptions<MongoDbSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .BindConfiguration("MongoDb");

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