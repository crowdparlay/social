namespace CrowdParlay.Social.IntegrationTests.Tests;

public class DiscussionsRepositoryTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Get discussions by author")]
    public async Task GetDiscussionsByAuthor()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionRepository>();

        var author = await authors.CreateAsync(
            id: Guid.NewGuid(),
            username: "compartmental",
            displayName: "Степной ишак",
            avatarUrl: null);

        DiscussionDto[] expected =
        [
            await discussions.CreateAsync(author.Id, "Discussion 1", "bla bla bla"),
            await discussions.CreateAsync(author.Id, "Discussion 2", "numa numa e")
        ];

        // Act
        var response = await discussions.GetByAuthorAsync(author.Id);

        // Assert
        response.Should().BeEquivalentTo(expected);
    }

    [Fact(DisplayName = "Get discussions by author of no discussions")]
    public async Task GetNoDiscussionsByAuthor()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionRepository>();

        var author = await authors.CreateAsync(
            id: Guid.NewGuid(),
            username: "compartmental",
            displayName: "Степной ишак",
            avatarUrl: null);

        // Act
        var response = await discussions.GetByAuthorAsync(author.Id);

        // Assert
        response.Should().BeEmpty();
    }
}