using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Users.gRPC;
using Grpc.Core;
using Mapster;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class UsersService(Users.gRPC.UsersService.UsersServiceClient usersClient) : IUsersService
{
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await usersClient.GetUserAsync(new GetUserRequest { Id = id.ToString() });
        return user.Adapt<UserDto>();
    }

    public async Task<IDictionary<Guid, UserDto>> GetUsersAsync(ISet<Guid> ids)
    {
        var request = new GetUsersRequest();
        request.Ids.AddRange(ids.Select(id => id.ToString()));
        var users = await usersClient.GetUsers(request).ResponseStream.ReadAllAsync().ToArrayAsync();

        return users.ToDictionary(
            user => new Guid(user.Id),
            user => user.Adapt<UserDto>());
    }
}