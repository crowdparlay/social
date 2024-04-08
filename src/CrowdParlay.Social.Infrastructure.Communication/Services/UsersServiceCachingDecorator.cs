using System.Text.Json;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using StackExchange.Redis;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class UsersServiceCachingDecorator(IUsersService usersService, IDatabase redis) : IUsersService
{
    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var cacheKey = id.ToString();
        var cacheValue = await redis.StringGetAsync(cacheKey);
        if (cacheValue.HasValue)
            return JsonSerializer.Deserialize<UserDto>(cacheValue.ToString())!;

        var user = await usersService.GetByIdAsync(id);
        _ = redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(1));
        return user;
    }

    public async Task<IDictionary<Guid, UserDto>> GetUsersAsync(ISet<Guid> ids)
    {
        var distinctUserIds = ids.ToArray();
        var cacheKeys = distinctUserIds.Select(id => new RedisKey(id.ToString())).ToArray();
        var cacheValues = await redis.StringGetAsync(cacheKeys);

        var users = cacheValues.Select(cacheValue => cacheValue switch
        {
            { HasValue: false } => null,
            _ => JsonSerializer.Deserialize<UserDto>(cacheValue.ToString())
        }).ToArray();

        var usersById = distinctUserIds.Zip(users).ToDictionary(x => x.First, x => x.Second);
        if (cacheValues.All(cacheValue => cacheValue.HasValue))
            return usersById!;

        var missingUserIds = usersById.Where(x => x.Value is null).Select(x => x.Key).ToHashSet();
        var missingUsersMap = await usersService.GetUsersAsync(missingUserIds);

        foreach (var (missingUserId, missingUser) in missingUsersMap)
            usersById[missingUserId] = missingUser;

        _ = Task.Run(async () =>
        {
            var transaction = redis.CreateTransaction();

            foreach (var (missingUserId, missingUser) in missingUsersMap)
            {
                await transaction.StringSetAsync(
                    missingUserId.ToString(),
                    JsonSerializer.Serialize(missingUser),
                    TimeSpan.FromMinutes(1));
            }

            await transaction.ExecuteAsync();
        });

        return usersById!;
    }
}