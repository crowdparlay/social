using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CrowdParlay.Social.Api.Swagger;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions swaggerOptions)
    {
        foreach (var description in provider.ApiVersionDescriptions)
            swaggerOptions.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));

        swaggerOptions.SupportNonNullableReferenceTypes();
        swaggerOptions.UseAllOfToExtendReferenceSchemas();
        swaggerOptions.AddSignalRSwaggerGen(options => options.ScanAssembly(Assembly.GetExecutingAssembly()));
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription versionDescription)
    {
        var info = new OpenApiInfo
        {
            Title = "Crowd Parlay Social API",
            Description = "RESTful API of the Crowd Parlay's Social service.",
            Version = versionDescription.ApiVersion.ToString()
        };

        if (versionDescription.IsDeprecated)
            info.Description += " This API version has been deprecated.";

        return info;
    }
}