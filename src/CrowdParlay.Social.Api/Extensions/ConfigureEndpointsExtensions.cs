using CrowdParlay.Social.Api.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Social.Api.Extensions;

public static class ConfigureEndpointsExtensions
{
    public static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
        }).AddNewtonsoftJson();

        services.AddApiVersioning(options => options.ReportApiVersions = true);

        return services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}