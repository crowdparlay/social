using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Infrastructure.Communication.Abstractions;
using CrowdParlay.Social.Infrastructure.Communication.Services;

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
            .Setup(service => service.GetByIdAsync(user.Id, CancellationToken.None))
            .ReturnsAsync(user);

        var usersCacheMock = new Mock<IUsersCache>();
        usersCacheMock
            .Setup(cache => cache.GetUserByIdAsync(user.Id))
            .ReturnsAsync(userPresentInCache ? user : null);

        var cachedUsersService = new UsersServiceCachingDecorator(usersServiceMock.Object, usersCacheMock.Object);

        // Act
        var result = await cachedUsersService.GetByIdAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(user);
        usersServiceMock.Verify(
            service => service.GetByIdAsync(user.Id, CancellationToken.None),
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
        var usersById = users.ToDictionary<UserDto, Guid, UserDto?>(user => user.Id, user => user);
        var nullsById = userIds.ToDictionary<Guid, Guid, UserDto?>(userId => userId, _ => null);

        var usersServiceMock = new Mock<IUsersService>();
        usersServiceMock
            .Setup(service => service.GetUsersAsync(userIds, CancellationToken.None))
            .ReturnsAsync(usersPresentInCache ? nullsById! : usersById);

        var cachedUsers = users.ToDictionary(user => user.Id, user => (UserDto?)user);
        var usersCacheMock = new Mock<IUsersCache>();
        usersCacheMock
            .Setup(cache => cache.GetUsersByIdsAsync(userIds))
            .ReturnsAsync(usersPresentInCache ? cachedUsers : nullsById);

        var cachedUsersService = new UsersServiceCachingDecorator(usersServiceMock.Object, usersCacheMock.Object);

        // Act
        var results = await cachedUsersService.GetUsersAsync(userIds, CancellationToken.None);

        // Assert
        results.Should().BeEquivalentTo(usersById);
        usersServiceMock.Verify(
            service => service.GetUsersAsync(userIds, CancellationToken.None),
            usersPresentInCache ? Times.Never : Times.Once);
    }
}