using System.Text.Json;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Infrastructure.Communication.Abstractions;
using StackExchange.Redis;

namespace CrowdParlay.Social.Infrastructure.Communication.Services;

public class RedisUsersCache(IDatabase redis) : IUsersCache
{
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var cacheValue = await redis.StringGetAsync(userId.ToString());
        return cacheValue.HasValue
            ? JsonSerializer.Deserialize<UserDto>(cacheValue.ToString())
            : null;
    }

    public async Task<Dictionary<Guid, UserDto?>> GetUsersByIdsAsync(ISet<Guid> userIds)
    {
        var cacheKeys = userIds.Select(userId => new RedisKey(userId.ToString())).ToArray();
        var cacheValues = await redis.StringGetAsync(cacheKeys);

        var users = cacheValues.Select(cacheValue => cacheValue switch
        {
            { HasValue: false } => null,
            _ => JsonSerializer.Deserialize<UserDto>(cacheValue.ToString())
        }).ToArray();

        return userIds.Zip(users).ToDictionary(x => x.First, x => x.Second);
    }

    public async Task SaveAsync(UserDto user) =>
        await redis.StringSetAsync(user.Id.ToString(), JsonSerializer.Serialize(user), TimeSpan.FromMinutes(1));

    public async Task SaveAsync(IEnumerable<UserDto> users)
    {
        var transaction = redis.CreateTransaction();

        foreach (var user in users)
            await transaction.StringSetAsync(user.Id.ToString(), JsonSerializer.Serialize(user), TimeSpan.FromMinutes(1));

        await transaction.ExecuteAsync();
    }
}