using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Application.Abstractions;

public interface IUsersService
{
    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<IDictionary<Guid, UserDto?>> GetUsersAsync(ISet<Guid> ids, CancellationToken cancellationToken = default);
}