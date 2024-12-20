using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.Hubs;
using CrowdParlay.Social.Application;
using CrowdParlay.Social.Infrastructure.Communication;
using CrowdParlay.Social.Infrastructure.Persistence;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Http.Connections;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;

namespace CrowdParlay.Social.Api;

public class Startup(IConfiguration configuration)
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithOpenTelemetrySpanId()
            .Enrich.WithOpenTelemetryTraceId()
            .CreateLogger();

        app.UseAllElasticApm(configuration);
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();
        app.UseHealthChecks("/healthz");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();
        app.UseCors();
        app.UseEndpoints(builder =>
        {
            builder.MapHub<CommentsHub>("/api/v1/hubs/comments", options => options.Transports = HttpTransportType.ServerSentEvents);
            builder.MapControllers();
        });
    }

    public void ConfigureServices(IServiceCollection services) => services
        .AddApi(configuration)
        .AddApplication()
        .AddPersistence(configuration)
        .AddCommunication(configuration);
}