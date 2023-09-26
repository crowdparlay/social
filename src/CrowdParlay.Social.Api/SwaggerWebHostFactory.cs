using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.OpenApi.Models;

namespace CrowdParlay.Social.Api;

public class SwaggerWebHostFactory
{
    private const string SwaggerVersion = "v1";

    public static IWebHost CreateWebHost() => WebHost.CreateDefaultBuilder()
        .Configure(builder => builder.New())
        .ConfigureServices(services =>
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(SwaggerVersion, new OpenApiInfo
                {
                    Title = Assembly.GetExecutingAssembly().GetName().Name,
                    Version = SwaggerVersion
                });
            });

            services.AddControllers();
            services.AddEndpointsApiExplorer();
        })
        .Build();
}