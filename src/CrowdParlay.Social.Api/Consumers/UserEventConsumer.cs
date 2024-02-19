using CrowdParlay.Communication;
using CrowdParlay.Social.Application.Abstractions;
using MassTransit;

namespace CrowdParlay.Social.Api.Consumers;

public class UserEventConsumer : IConsumer<UserCreatedEvent>, IConsumer<UserUpdatedEvent>, IConsumer<UserDeletedEvent>
{
    private readonly IAuthorRepository _authors;

    public UserEventConsumer(IAuthorRepository authors) => _authors = authors;

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        await _authors.CreateAsync(
            Guid.Parse(context.Message.UserId),
            context.Message.Username,
            context.Message.DisplayName,
            context.Message.AvatarUrl);
    }

    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        await _authors.UpdateAsync(
            Guid.Parse(context.Message.UserId),
            context.Message.Username,
            context.Message.DisplayName,
            context.Message.AvatarUrl);
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context) =>
        await _authors.DeleteAsync(Guid.Parse(context.Message.UserId));
}