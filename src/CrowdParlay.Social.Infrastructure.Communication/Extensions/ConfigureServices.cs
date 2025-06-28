using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Infrastructure.Communication.Abstractions;
using CrowdParlay.Social.Infrastructure.Communication.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CrowdParlay.Social.Infrastructure.Communication.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        var usersGrpcAddress =
            configuration.GetValue<Uri>("USERS_GRPC_ADDRESS")
            ?? throw new InvalidOperationException("Missing required configuration 'USERS_GRPC_ADDRESS'.");

        services.AddGrpcClient<Users.gRPC.UsersService.UsersServiceClient>(options => options.Address = usersGrpcAddress);

        var redisConnectionString =
            configuration["REDIS_CONNECTION_STRING"] ??
            throw new InvalidOperationException("REDIS_CONNECTION_STRING is not set!");

        services.AddSingleton<IDatabase>(_ =>
        {
            var connection = ConnectionMultiplexer.Connect(redisConnectionString);
            return connection.GetDatabase();
        });

        return services
            .ConfigureOpenTelemetry(configuration)
            .AddScoped<IUsersCache, RedisUsersCache>()
            .AddScoped<IUsersService, UsersService>()
            .Decorate<IUsersService, UsersServiceCachingDecorator>()
            .Decorate<IUsersService, UsersServiceResilienceDecorator>();
    }
}