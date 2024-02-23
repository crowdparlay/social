using CrowdParlay.Communication;
using CrowdParlay.Social.Application.Abstractions;
using MassTransit;

namespace CrowdParlay.Social.Api.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class UserEventConsumer(IAuthorRepository authors) : IConsumer<UserCreatedEvent>, IConsumer<UserUpdatedEvent>, IConsumer<UserDeletedEvent>
{
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        await authors.CreateAsync(
            Guid.Parse(context.Message.UserId),
            context.Message.Username,
            context.Message.DisplayName,
            context.Message.AvatarUrl);
    }

    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        await authors.UpdateAsync(
            Guid.Parse(context.Message.UserId),
            context.Message.Username,
            context.Message.DisplayName,
            context.Message.AvatarUrl);
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context) =>
        await authors.DeleteAsync(Guid.Parse(context.Message.UserId));
}