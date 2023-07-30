using CrowdParlay.Social.Api.Middlewares;

namespace CrowdParlay.Social.Api;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services) => services
        .AddSwaggerGen()
        .AddEndpointsApiExplorer()
        .AddTransient<ExceptionHandlingMiddleware>();
}
