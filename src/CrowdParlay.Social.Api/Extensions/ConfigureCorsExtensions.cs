namespace CrowdParlay.Social.Api.Extensions;

public static class ConfigureCorsExtensions
{
    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
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