using System.Reflection;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.Application;

public static class ConfigureServicesExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<IDiscussionsService, DiscussionsService>()
        .AddScoped<ICommentsService, CommentsService>()
        .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
}