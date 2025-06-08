using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class StartupConfigurator : IHostedService
{
    private static int _executed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Ensure this code runs only once, so that global state configurators
        // don't get called multiple times when running integration tests.
        if (Interlocked.CompareExchange(ref _executed, 1, 0) == 1)
            return Task.CompletedTask;

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        ConventionRegistry.Register(
            name: "CamelCaseElementNameConvention",
            conventions: new ConventionPack { new CamelCaseElementNameConvention() },
            filter: _ => true);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}