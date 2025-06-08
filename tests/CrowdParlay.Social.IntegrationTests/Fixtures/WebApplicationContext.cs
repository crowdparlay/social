using CrowdParlay.Social.Api;
using CrowdParlay.Social.IntegrationTests.Services;
using Microsoft.AspNetCore.TestHost;
using Nito.AsyncEx;
using Testcontainers.MongoDb;
using Testcontainers.Redis;

namespace CrowdParlay.Social.IntegrationTests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class WebApplicationContext
{
    public readonly IServiceProvider Services;
    public readonly TestServer Server;

    public WebApplicationContext()
    {
        var mongoDbConfiguration = SetupMongoDb();
        var redisConfiguration = SetupRedis();

        var webApplicationFactory = new TestWebApplicationFactory<Program>(mongoDbConfiguration, redisConfiguration);

        Services = webApplicationFactory.Services;
        Server = webApplicationFactory.Server;
    }

    private static MongoDbConfiguration SetupMongoDb()
    {
        var mongoDb = new MongoDbBuilder().WithReplicaSet().Build();
        AsyncContext.Run(async () => await mongoDb.StartAsync());
        return new MongoDbConfiguration(mongoDb.GetConnectionString(), "social");
    }

    private static RedisConfiguration SetupRedis()
    {
        var redis = new RedisBuilder().Build();
        AsyncContext.Run(async () => await redis.StartAsync());
        return new RedisConfiguration(redis.GetConnectionString());
    }
}