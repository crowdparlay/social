namespace CrowdParlay.Social.Api.Extensions;

partial class ServiceCollectionExtensions
{
    private static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins =
            configuration["CORS_ORIGINS"]?.Split(';')
            ?? throw new InvalidOperationException("Missing required configuration 'CORS_ORIGINS'.");

        return services.AddCors(options => options
            .AddDefaultPolicy(builder => builder
                .WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()));
    }
}