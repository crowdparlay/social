using System.Reflection;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<IDiscussionsService, DiscussionsService>()
        .AddScoped<ICommentsService, CommentsService>()
        .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
}