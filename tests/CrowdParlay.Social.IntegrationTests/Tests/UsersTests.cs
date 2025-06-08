using System.Text.Json;
using StackExchange.Redis;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class UsersTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
        private readonly IServiceProvider _services = context.Services;

        [Fact(DisplayName = "Get user by ID returns cached user")]
        public async Task GetUserByIdReturnsCachedUser()
        {
            // Arrange
            await using var scope = _services.CreateAsyncScope();
            var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();
            var redisDatabase = scope.ServiceProvider.GetRequiredService<IDatabase>();

            var expectedUser = new UserDto
            {
                Id = Guid.NewGuid(),
                Username = "cached_user",
                DisplayName = "Cached user",
                AvatarUrl = null
            };
        
            await redisDatabase.StringSetAsync(
                expectedUser.Id.ToString(),
                JsonSerializer.Serialize(expectedUser),
                TimeSpan.FromMinutes(1));
        
            // Act
            var actualUser = await usersService.GetByIdAsync(expectedUser.Id);

            // Assert
            actualUser.Should().BeEquivalentTo(expectedUser);
        }
}