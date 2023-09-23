using CrowdParlay.Communication;
using CrowdParlay.Social.Api.Middlewares;
using CrowdParlay.Social.Api.Routing;
using CrowdParlay.Social.Application.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Social.Api;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthorization()
            .AddSwaggerGen()
            .AddEndpointsApiExplorer()
            .AddTransient<ExceptionHandlingMiddleware>();

        var mvcBuilder = services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
        });

        mvcBuilder.AddNewtonsoftJson();

        return services.AddMassTransit(bus =>
        {
            bus.AddConsumersFromNamespaceContaining<UserEventConsumer>();
            bus.UsingRabbitMq((context, configurator) =>
            {
                var amqpServerUrl =
                    configuration["RABBITMQ_AMQP_SERVER_URL"]
                    ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'.");

                configurator.Host(amqpServerUrl);
                configurator.ConfigureEndpoints(context);
                configurator.ConfigureTopology();
                
                configurator.Message<UserUpdatedEvent>(x => x.SetEntityName("user"));
            });
        });
    }
}