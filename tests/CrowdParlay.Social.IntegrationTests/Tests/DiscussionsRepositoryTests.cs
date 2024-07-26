using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.Entities;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class DiscussionsRepositoryTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Get all discussions")]
    public async Task GetAllDiscussions()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        Discussion[] expected =
        [
            await discussions.CreateAsync(Guid.NewGuid(), "Discussion 1", "bla bla bla"),
            await discussions.CreateAsync(Guid.NewGuid(), "Discussion 2", "numa numa e"),
            await discussions.CreateAsync(Guid.NewGuid(), "Discussion 3", "bara bara bara")
        ];

        // Act
        var response = await discussions.GetAllAsync(0, 2);

        // Assert
        response.Items.Should().BeEquivalentTo(expected.TakeLast(2).Reverse());
        response.TotalCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact(DisplayName = "Get discussions by author")]
    public async Task GetDiscussionsByAuthor()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        var authorId = Guid.NewGuid();
        Discussion[] expected =
        [
            await discussions.CreateAsync(authorId, "Discussion 1", "bla bla bla"),
            await discussions.CreateAsync(authorId, "Discussion 2", "numa numa e")
        ];

        // Act
        var response = await discussions.GetByAuthorAsync(authorId, 0, 10);

        // Assert
        response.Items.Should().BeEquivalentTo(expected.Reverse());
    }

    [Fact(DisplayName = "Get discussions by author of no discussions")]
    public async Task GetNoDiscussionsByAuthor()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        // Act
        var response = await discussions.GetByAuthorAsync(Guid.NewGuid(), 0, 10);

        // Assert
        response.Items.Should().BeEmpty();
    }
}