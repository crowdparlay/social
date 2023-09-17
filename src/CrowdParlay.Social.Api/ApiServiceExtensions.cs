using CrowdParlay.Communication;
using CrowdParlay.Social.Api.Middlewares;
using CrowdParlay.Social.Application.Listeners;
using MassTransit;

namespace CrowdParlay.Social.Api;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSwaggerGen()
            .AddEndpointsApiExplorer()
            .AddTransient<ExceptionHandlingMiddleware>();

        return services.AddMassTransit(bus =>
        {
            bus.AddConsumersFromNamespaceContaining<UserEventsListener>();
            bus.UsingRabbitMq((context, configurator) =>
            {
                var amqpServerUrl =
                    configuration["RABBITMQ_AMQP_SERVER_URL"]
                    ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'.");

                configurator.Host(amqpServerUrl);
                configurator.ConfigureEndpoints(context);
                configurator.ConfigureTopology();
            });
        });
    }
}