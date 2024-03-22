using CrowdParlay.Communication;
using CrowdParlay.Social.Api.Consumers;
using CrowdParlay.Social.Api.Services;
using MassTransit;

namespace CrowdParlay.Social.Api.Extensions;

public static class ConfigureApiExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .ConfigureEndpoints()
            .ConfigureAuthentication()
            .AddAuthorization();

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