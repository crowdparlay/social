using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class UsersServiceResilienceDecorator(
    IUsersService usersService,
    ILogger<UsersServiceResilienceDecorator> logger) : IUsersService
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cts = new CancellationTokenSource(Timeout);
        cancellationToken.Register(() => cts.Cancel());

        try
        {
            return await usersService.GetByIdAsync(id, cts.Token);
        }
        catch (RpcException exception)
        {
            logger.LogError(exception, message: null);
            return new UserDto
            {
                Id = id,
                Username = null,
                DisplayName = null,
                AvatarUrl = null
            };
        }
    }

    public async Task<IDictionary<Guid, UserDto?>> GetUsersAsync(ISet<Guid> ids, CancellationToken cancellationToken)
    {
        var cts = new CancellationTokenSource(Timeout);
        cancellationToken.Register(() => cts.Cancel());

        try
        {
            return await usersService.GetUsersAsync(ids, cts.Token);
        }
        catch (RpcException exception)
        {
            logger.LogError(exception, message: null);
            return ids.ToDictionary<Guid, Guid, UserDto?>(id => id, id => new UserDto
            {
                Id = id,
                Username = null,
                DisplayName = null,
                AvatarUrl = null
            });
        }
    }
}