using CrowdParlay.Social.Application.DTOs;

namespace CrowdParlay.Social.Infrastructure.Communication.Abstractions;

public interface IUsersCache
{
    public Task<UserDto?> GetUserByIdAsync(Guid userId);
    public Task<Dictionary<Guid, UserDto?>> GetUsersByIdsAsync(ISet<Guid> userIds);
    public Task SaveAsync(UserDto user);
    public Task SaveAsync(IEnumerable<UserDto> users);
}