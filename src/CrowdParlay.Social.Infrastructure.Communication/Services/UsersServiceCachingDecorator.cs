using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Infrastructure.Communication.Abstractions;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class UsersServiceCachingDecorator(IUsersService usersService, IUsersCache usersCache) : IUsersService
{
    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await usersCache.GetUserByIdAsync(userId);

        if (user is null)
        {
            user = await usersService.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
                await usersCache.SaveAsync(user);
        }

        return user;
    }

    public async Task<IDictionary<Guid, UserDto?>> GetUsersAsync(ISet<Guid> userIds, CancellationToken cancellationToken)
    {
        var usersById = await usersCache.GetUsersByIdsAsync(userIds);
        var missingUserIds = usersById
            .Where(x => x.Value is null)
            .Select(x => x.Key)
            .ToHashSet();

        if (missingUserIds.Count > 0)
        {
            var missingUsersById = await usersService.GetUsersAsync(missingUserIds, cancellationToken);
            foreach (var (missingUserId, missingUser) in missingUsersById)
                usersById[missingUserId] = missingUser;

            var missingUsers = missingUsersById.Values.Where(user => user is not null).Cast<UserDto>();
            _ = usersCache.SaveAsync(missingUsers);
        }

        return usersById;
    }
}