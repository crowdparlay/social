using CrowdParlay.Social.Api.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Social.Api.Extensions;

partial class ServiceCollectionExtensions
{
    internal static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        var mvcBuilder = services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
        });

        mvcBuilder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = GlobalSerializerOptions.SnakeCase.PropertyNamingPolicy;
            options.JsonSerializerOptions.DictionaryKeyPolicy = GlobalSerializerOptions.SnakeCase.DictionaryKeyPolicy;
        });

        services.AddApiVersioning(options => options.ReportApiVersions = true);

        return services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}