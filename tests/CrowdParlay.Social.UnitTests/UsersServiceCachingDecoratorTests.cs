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
            .Setup(users => users.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        var redisDatabaseMock = new Mock<IDatabase>();
        redisDatabaseMock
            .Setup(database => database.StringGetAsync(user.Id.ToString(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(userPresentInCache ? JsonSerializer.Serialize(user) : RedisValue.Null);

        var cachedUsersService = new UsersServiceCachingDecorator(usersServiceMock.Object, redisDatabaseMock.Object);

        // Act
        UserDto[] results =
        [
            await cachedUsersService.GetByIdAsync(user.Id),
            await cachedUsersService.GetByIdAsync(user.Id),
            await cachedUsersService.GetByIdAsync(user.Id)
        ];

        // Assert
        results.Should().AllBeEquivalentTo(user);
        usersServiceMock.Verify(
            users => users.GetByIdAsync(user.Id),
            userPresentInCache ? Times.Never() : Times.Exactly(3));
    }
}