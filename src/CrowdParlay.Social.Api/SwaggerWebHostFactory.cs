using CrowdParlay.Social.Api.Routing;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
                options.SupportNonNullableReferenceTypes();
                options.SwaggerDoc(SwaggerVersion, new OpenApiInfo
                {
                    Title = "Crowd Parlay Social API",
                    Version = SwaggerVersion
                });
            });

            services.AddEndpointsApiExplorer();
            services.AddControllers(options =>
            {
                var transformer = new KebabCaseParameterPolicy();
                options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
            });
        })
        .Build();
}