using System.Reflection;
using CrowdParlay.Communication.RabbitMq.DependencyInjection;
using CrowdParlay.Social.Application.Behaviors;
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
        
        var rabbitMqAmqpServerUrl =
            configuration["RABBITMQ_AMQP_SERVER_URL"] ??
            throw new InvalidOperationException("RABBITMQ_AMQP_SERVER_URL is not set!");

        return services
            .AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, null, true)
            .AddMediatR(assembly)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddRabbitMqCommunication(options => options
                .UseAmqpServer(rabbitMqAmqpServerUrl)
                .UseMessageListenersFromAssembly(assembly)
            );
    }
}