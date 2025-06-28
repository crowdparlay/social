using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CrowdParlay.Social.Infrastructure.Communication.Extensions;

partial class ServiceCollectionExtensions
{
    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var openTelemetryConfiguration = configuration.GetRequiredSection("OpenTelemetry");
        var openTelemetrySettings = openTelemetryConfiguration.Get<OpenTelemetrySettings>()!;

        services
            .AddOptions<OpenTelemetrySettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Bind(openTelemetryConfiguration);

        var openTelemetryBuilder = services.AddOpenTelemetry();

        openTelemetryBuilder.ConfigureResource(resource => resource
            .AddService(openTelemetrySettings.ServiceName));

        openTelemetryBuilder.WithTracing(tracing => tracing
            .AddSource(openTelemetrySettings.AdditionalTraceSources.Split(';'))
            .AddAspNetCoreInstrumentation()
            .AddGrpcClientInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddMassTransitInstrumentation()
            .AddRedisInstrumentation()
            .AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(openTelemetrySettings.OtlpEndpoint);
                exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));

        openTelemetryBuilder.WithMetrics(metrics => metrics
            .AddMeter(openTelemetrySettings.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(openTelemetrySettings.OtlpEndpoint);
                exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));

        return services;
    }
}