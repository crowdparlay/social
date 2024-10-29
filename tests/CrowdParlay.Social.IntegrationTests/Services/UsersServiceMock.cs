namespace CrowdParlay.Social.IntegrationTests.Services;

public class UsersServiceMock : IUsersService
{
    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<UserDto?>(new UserDto
    {
        Id = id,
        Username = $"user_{id:N}",
        DisplayName = $"User {id:N}",
        AvatarUrl = null
    });

    public Task<IDictionary<Guid, UserDto?>> GetUsersAsync(ISet<Guid> ids, CancellationToken cancellationToken)
    {
        var users = ids.Select(id => new UserDto
        {
            Id = id,
            Username = $"user_{id:N}",
            DisplayName = $"User {id:N}",
            AvatarUrl = null
        });

        var usersById = users.ToDictionary<UserDto, Guid, UserDto?>(user => user.Id, user => user);
        return Task.FromResult<IDictionary<Guid, UserDto?>>(usersById);
    }
}