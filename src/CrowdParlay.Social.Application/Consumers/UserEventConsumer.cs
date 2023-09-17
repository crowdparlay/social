using CrowdParlay.Communication;
using CrowdParlay.Social.Application.Features.Authors.Commands;
using Mapster;
using MassTransit;
using MediatR;

namespace CrowdParlay.Social.Application.Listeners;

public class UserEventConsumer : IConsumer<UserCreatedEvent>, IConsumer<UserUpdatedEvent>, IConsumer<UserDeletedEvent>
{
    private readonly ISender _sender;

    public UserEventConsumer(ISender sender) => _sender = sender;

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var command = context.Message.Adapt<CreateAuthorCommand>();
        await _sender.Send(command);
    }

    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var command = context.Message.Adapt<UpdateAuthorCommand>();
        await _sender.Send(command);
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var command = context.Message.Adapt<DeleteAuthorCommand>();
        await _sender.Send(command);
    }
}