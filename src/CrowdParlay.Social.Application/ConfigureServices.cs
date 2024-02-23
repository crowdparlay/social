using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.Application;

public static class ConfigureServicesExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
    }
}