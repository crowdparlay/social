using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Users.gRPC;
using Grpc.Core;
using Mapster;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class UsersService(Users.gRPC.UsersService.UsersServiceClient usersClient) : IUsersService
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = new GetUserRequest { Id = id.ToString() };
        var user = await usersClient.GetUserAsync(request, cancellationToken: cancellationToken);
        return user.Adapt<UserDto?>();
    }

    public async Task<IDictionary<Guid, UserDto?>> GetUsersAsync(ISet<Guid> ids, CancellationToken cancellationToken)
    {
        var request = new GetUsersRequest();
        request.Ids.AddRange(ids.Select(id => id.ToString()));
        var usersStream = usersClient.GetUsers(request, cancellationToken: cancellationToken).ResponseStream;
        var users = await usersStream.ReadAllAsync(cancellationToken).ToArrayAsync(cancellationToken);

        return users.ToDictionary(
            user => new Guid(user.Id),
            user => user.Adapt<UserDto?>());
    }
}