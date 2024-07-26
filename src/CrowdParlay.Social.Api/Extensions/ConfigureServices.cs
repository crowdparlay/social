using CrowdParlay.Social.Api.Services;

namespace CrowdParlay.Social.Api.Extensions;

public static class ConfigureApiExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        return services
            .ConfigureEndpoints()
            .ConfigureAuthentication(configuration)
            .ConfigureCors(configuration)
            .ConfigureSignalR(configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .AddAuthorization();
    }
}