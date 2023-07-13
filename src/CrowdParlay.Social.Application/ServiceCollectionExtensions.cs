using System.Reflection;
using CrowdParlay.Social.Application.Behaviors;
using CrowdParlay.Social.Application.Middlewares;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services
            .AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, null, true)
            .AddMediatR(assembly)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddTransient<ExceptionHandlingMiddleware>();
    }
}