using CrowdParlay.Social.Api.Middlewares;
using CrowdParlay.Social.Application;
using CrowdParlay.Social.Infrastructure;
using Serilog;

namespace CrowdParlay.Social.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        // TODO: implement TraceIdMiddleware
        // app.UseMiddleware<TraceIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseHealthChecks("/health");

        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());

        if (_environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(builder => builder.MapControllers());
    }

    public void ConfigureServices(IServiceCollection services) => services
        .AddApi(_configuration)
        .AddApplication()
        .AddInfrastructure(_configuration);
}