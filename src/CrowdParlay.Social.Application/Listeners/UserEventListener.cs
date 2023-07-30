using CrowdParlay.Communication;
using CrowdParlay.Communication.Abstractions;
using CrowdParlay.Social.Application.Features.Authors.Commands;
using Mapster;
using MediatR;

namespace CrowdParlay.Social.Application.Listeners;

public class UserEventsListener : IMessageListener<UserCreatedEvent>, IMessageListener<UserDeletedEvent>, IMessageListener<UserUpdatedEvent>
{
    private readonly ISender _sender;

    public UserEventsListener(ISender sender)
    {
        _sender = sender;
    }

    public async Task HandleAsync(UserCreatedEvent message)
    {
        var command = message.Adapt<CreateAuthorCommand>();
        await _sender.Send(command);
    }

    public async Task HandleAsync(UserDeletedEvent message)
    {
        var command = message.Adapt<DeleteAuthorCommand>();
        await _sender.Send(command);
    }

    public async Task HandleAsync(UserUpdatedEvent message)
    {
        var command = message.Adapt<UpdateAuthorCommand>();
        await _sender.Send(command);
    }
}