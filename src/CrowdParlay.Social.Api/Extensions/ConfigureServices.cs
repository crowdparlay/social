using CrowdParlay.Communication;
using CrowdParlay.Social.Api.Services;
using MassTransit;

namespace CrowdParlay.Social.Api.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .ConfigureEndpoints()
            .ConfigureAuthentication(configuration)
            .ConfigureCors(configuration)
            .ConfigureSignalR(configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .AddAuthorization();

        return services.AddMassTransit(bus =>
        {
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