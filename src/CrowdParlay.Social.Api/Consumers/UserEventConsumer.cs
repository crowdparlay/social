using CrowdParlay.Communication;
using CrowdParlay.Social.Domain.Abstractions;
using MassTransit;

namespace CrowdParlay.Social.Api.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UserEventConsumer(IAuthorsRepository authors) : IConsumer<UserCreatedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context) =>
        await authors.EnsureCreatedAsync(Guid.Parse(context.Message.UserId));
}