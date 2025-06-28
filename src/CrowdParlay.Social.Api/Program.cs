using CrowdParlay.Social.Infrastructure.Communication;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;
using Serilog.Sinks.OpenTelemetry;

namespace CrowdParlay.Social.Api;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
        .UseSerilog((context, logger) => logger
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.WithOpenTelemetrySpanId()
            .Enrich.WithOpenTelemetryTraceId()
            .WriteTo.OpenTelemetry(sink =>
            {
                sink.Endpoint = context.Configuration.GetRequiredSection("OpenTelemetry").Get<OpenTelemetrySettings>()?.OtlpEndpoint;
                sink.Protocol = OtlpProtocol.HttpProtobuf;
            }));
}