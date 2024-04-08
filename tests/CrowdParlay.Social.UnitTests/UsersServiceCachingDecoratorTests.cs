using System.Text.Json;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Infrastructure.Communication.Services;
using StackExchange.Redis;

namespace CrowdParlay.Social.UnitTests;

public class UsersServiceCachingDecoratorTests
{
    [Theory(DisplayName = "Get user by ID returns cached result if possible")]
    [InlineData(true), InlineData(false)]
    public async Task GetUserById_ThatIsCached_ReturnsCachedResult(bool userPresentInCache)
    {
        // Arrange
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            Username = "username",
            DisplayName = "Display Name",
            AvatarUrl = null
        };

        var usersServiceMock = new Mock<IUsersService>();
        usersServiceMock
            .Setup(service => service.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var redisDatabaseMock = new Mock<IDatabase>();
        redisDatabaseMock
            .Setup(database => database.StringGetAsync(user.Id.ToString(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(userPresentInCache ? JsonSerializer.Serialize(user) : RedisValue.Null);

        var cachedUsersService = new UsersServiceCachingDecorator(usersServiceMock.Object, redisDatabaseMock.Object);

        // Act
        var result = await cachedUsersService.GetByIdAsync(user.Id);

        // Assert
        result.Should().BeEquivalentTo(user);
        usersServiceMock.Verify(
            service => service.GetByIdAsync(user.Id),
            userPresentInCache ? Times.Never : Times.Once);
    }

    [Theory(DisplayName = "Get users by IDs returns cached results if possible")]
    [InlineData(true), InlineData(false)]
    public async Task GetUsersByIds_ThatAreCached_ReturnsCachedResults(bool usersPresentInCache)
    {
        // Arrange
        UserDto[] users =
        [
            new UserDto
            {
                Id = Guid.NewGuid(),
                Username = "username1",
                DisplayName = "Display Name 1",
                AvatarUrl = null
            },
            new UserDto
            {
                Id = Guid.NewGuid(),
                Username = "username2",
                DisplayName = "Display Name 2",
                AvatarUrl = null
            }
        ];

        var userIds = users.Select(user => user.Id).ToHashSet();
        var usersById = users.ToDictionary(user => user.Id, user => user);

        var usersServiceMock = new Mock<IUsersService>();
        usersServiceMock
            .Setup(service => service.GetUsersAsync(userIds))
            .ReturnsAsync(usersById);

        var cacheKeys = userIds.Select(id => new RedisKey(id.ToString())).ToArray();
        var cachedUsers = users
            .Select(user => JsonSerializer.Serialize(user))
            .Select(serializedUser => new RedisValue(serializedUser))
            .ToArray();

        var redisDatabaseMock = new Mock<IDatabase>();
        redisDatabaseMock
            .Setup(database => database.StringGetAsync(cacheKeys, It.IsAny<CommandFlags>()))
            .ReturnsAsync(usersPresentInCache ? cachedUsers : [RedisValue.Null, RedisValue.Null]);

        var cachedUsersService = new UsersServiceCachingDecorator(usersServiceMock.Object, redisDatabaseMock.Object);

        // Act
        var results = await cachedUsersService.GetUsersAsync(userIds);

        // Assert
        results.Should().BeEquivalentTo(usersById);
        usersServiceMock.Verify(
            service => service.GetUsersAsync(userIds),
            usersPresentInCache ? Times.Never : Times.Once);
    }
}