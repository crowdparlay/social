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

    public IAsyncEnumerable<UserDto> GetUsersAsync(IEnumerable<Guid> ids)
    {
        var users = ids.Select(id => new UserDto
        {
            Id = id,
            Username = $"user_{id:N}",
            DisplayName = $"User {id:N}",
            AvatarUrl = null
        });

        return users.ToAsyncEnumerable();
    }
}