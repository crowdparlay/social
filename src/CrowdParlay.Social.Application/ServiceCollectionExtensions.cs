using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddMediatR();

    private static IServiceCollection AddMediatR(this IServiceCollection services) =>
        services.AddMediatR(Assembly.GetExecutingAssembly());
}