using System.Reflection;
using CrowdParlay.Social.Api.Extensions;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CrowdParlay.Social.Api.Swagger;

/// <summary>
/// Web host factory that is used by the Swashbuckle.AspNetCore.Cli .NET tool to produce an OpenAPI specification.
/// </summary>
// ReSharper disable once UnusedType.Global
public class SwaggerWebHostFactory
{
    public static IWebHost CreateWebHost() => WebHost.CreateDefaultBuilder()
        .Configure(builder => builder.New())
        .ConfigureServices(services =>
        {
            services.ConfigureEndpoints();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                var xmlDocsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlDocsFileName));
                options.SupportNonNullableReferenceTypes();
            });
        })
        .Build();
}