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

    public IAsyncEnumerable<UserDto> GetUsersAsync(IEnumerable<Guid> ids)
    {
        var request = new GetUsersRequest();
        request.Ids.AddRange(ids.Select(x => x.ToString()));
        var response = usersClient.GetUsers(request).ResponseStream;
        return response.ReadAllAsync().Select(x => x.Adapt<UserDto>());
    }
}