using System.Reflection;
using CrowdParlay.Social.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CrowdParlay.Social.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings()
            .WriteTo.File("logs/CrowdParlay.Social.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        var assembly = Assembly.GetExecutingAssembly();
        return services
            .AddValidatorsFromAssembly(assembly, includeInternalTypes: true)
            .AddMediatR(x => x.RegisterServicesFromAssembly(assembly))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}