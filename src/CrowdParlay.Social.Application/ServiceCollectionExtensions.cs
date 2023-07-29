using System.Reflection;
using CrowdParlay.Social.Application.Behaviors;
using CrowdParlay.Social.Application.Middlewares;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CrowdParlay.Social.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings()
            .WriteTo.File("logs/CrowdParlay.Social.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        var assembly = Assembly.GetExecutingAssembly();
        return services
            .AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, null, true)
            .AddMediatR(assembly)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddTransient<ExceptionHandlingMiddleware>();
    }
}