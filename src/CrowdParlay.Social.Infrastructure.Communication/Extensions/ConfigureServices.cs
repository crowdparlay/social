using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Infrastructure.Communication.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.Infrastructure.Communication.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        var usersGrpcAddress =
            configuration.GetValue<Uri>("USERS_GRPC_ADDRESS")
            ?? throw new InvalidOperationException("Missing required configuration 'USERS_GRPC_ADDRESS'.");

        services.AddGrpcClient<Users.gRPC.UsersService.UsersServiceClient>(options => options.Address = usersGrpcAddress);
        return services.AddScoped<IUsersService, UsersService>();
    }
}