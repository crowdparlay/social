using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.Hubs;
using CrowdParlay.Social.Application.Extensions;
using CrowdParlay.Social.Infrastructure.Communication.Extensions;
using CrowdParlay.Social.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Http.Connections;
using Serilog;

namespace CrowdParlay.Social.Api;

public class Startup(IConfiguration configuration)
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
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
        .AddPersistence()
        .AddCommunication(configuration);
}