using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Infrastructure.Communication.Abstractions;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class UsersServiceCachingDecorator(IUsersService usersService, IUsersCache usersCache) : IUsersService
{
    public async Task<UserDto> GetByIdAsync(Guid userId)
    {
        var user = await usersCache.GetUserByIdAsync(userId);

        if (user is null)
        {
            user = await usersService.GetByIdAsync(userId);
            await usersCache.SaveAsync(user);
        }

        return user;
    }

    public async Task<IDictionary<Guid, UserDto>> GetUsersAsync(ISet<Guid> userIds)
    {
        var usersById = await usersCache.GetUsersByIdsAsync(userIds);
        var missingUserIds = usersById
            .Where(x => x.Value is null)
            .Select(x => x.Key)
            .ToHashSet();

        if (missingUserIds.Count > 0)
        {
            var missingUsersById = await usersService.GetUsersAsync(missingUserIds);
            foreach (var (missingUserId, missingUser) in missingUsersById)
                usersById[missingUserId] = missingUser;

            _ = usersCache.SaveAsync(missingUsersById.Values);
        }

        return usersById!;
    }
}