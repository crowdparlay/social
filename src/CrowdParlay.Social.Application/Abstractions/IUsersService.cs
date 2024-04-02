using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IUsersService
{
    public Task<UserDto> GetByIdAsync(Guid id);
    public IAsyncEnumerable<UserDto> GetUsersAsync(IEnumerable<Guid> ids);
}