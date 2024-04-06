namespace CrowdParlay.Social.IntegrationTests.Services;

public class UsersServiceMock : IUsersService
{
    public Task<UserDto> GetByIdAsync(Guid id) => Task.FromResult(new UserDto
    {
        Id = id,
        Username = $"user_{id:N}",
        DisplayName = $"User {id:N}",
        AvatarUrl = null
    });

    public Task<IDictionary<Guid, UserDto>> GetUsersAsync(ISet<Guid> ids)
    {
        var users = ids.Select(id => new UserDto
        {
            Id = id,
            Username = $"user_{id:N}",
            DisplayName = $"User {id:N}",
            AvatarUrl = null
        });

        return Task.FromResult<IDictionary<Guid, UserDto>>(users.ToDictionary(x => x.Id, x => x));
    }
}