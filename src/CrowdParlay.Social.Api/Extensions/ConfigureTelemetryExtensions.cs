using OpenTelemetry.Trace;

namespace CrowdParlay.Social.Api.Extensions;

public static class ConfigureTelemetryExtensions
{
    public static IServiceCollection ConfigureTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpExporterEndpoint =
            configuration["TELEMETRY_OTLP_EXPORTER_ENDPOINT"]
            ?? throw new InvalidOperationException("Missing required configuration 'CORS_ORIGINS'.");

        var telemetrySourceName =
            configuration["TELEMETRY_SOURCE_NAME"]
            ?? throw new InvalidOperationException("Missing required configuration 'TELEMETRY_SOURCE_NAME'.");

        var openTelemetryBuilder = services.AddOpenTelemetry();

        openTelemetryBuilder.WithMetrics(metrics => metrics
            .AddMeter(telemetrySourceName)
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel"));

        openTelemetryBuilder.WithTracing(builder => builder
            .AddSource(telemetrySourceName)
            .AddOtlpExporter(options => options.Endpoint = new Uri(otlpExporterEndpoint))
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation());

        return services;
    }
}