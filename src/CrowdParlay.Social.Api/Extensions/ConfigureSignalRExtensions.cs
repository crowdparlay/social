namespace CrowdParlay.Social.Api.Extensions;

partial class ServiceCollectionExtensions
{
    private static IServiceCollection ConfigureSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString =
            configuration["REDIS_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Missing required configuration 'REDIS_CONNECTION_STRING'.");

        services.AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions = GlobalSerializerOptions.SnakeCase)
            .AddStackExchangeRedis(redisConnectionString);

        return services;
    }
}