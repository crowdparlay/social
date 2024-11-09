using CrowdParlay.Social.Domain.Abstractions;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class DiscussionsRepositoryTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Get discussions by author")]
    public async Task GetDiscussionsByAuthor()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        var authorId = Guid.NewGuid();
        Guid[] expectedDiscussionIds =
        [
            await discussions.CreateAsync(authorId, "Discussion 1", "bla bla bla"),
            await discussions.CreateAsync(authorId, "Discussion 2", "numa numa e")
        ];

        var expectedDiscussions = expectedDiscussionIds.Select(discussionId =>
            discussions.GetByIdAsync(discussionId, authorId).Result).ToArray();

        // Act
        var response = await discussions.SearchAsync(authorId, authorId, 0, 10);

        // Assert
        response.Items.Should().BeEquivalentTo(expectedDiscussions.Reverse());
        response.Items.Should().OnlyContain(discussion => discussion.ReactionCounters.Count == 0);
        response.Items.Should().OnlyContain(discussion => discussion.ViewerReactions.Count == 0);
    }

    [Fact(DisplayName = "Get discussions by author of no discussions")]
    public async Task GetNoDiscussionsByAuthor()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        // Act
        var response = await discussions.SearchAsync(Guid.NewGuid(), Guid.NewGuid(), 0, 10);

        // Assert
        response.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Get discussion with reactions")]
    public async Task GetDiscussionWithReactions()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var authorsRepository = scope.ServiceProvider.GetRequiredService<IAuthorsRepository>();
        var discussionsRepository = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var reactionsRepository = scope.ServiceProvider.GetRequiredService<IReactionsRepository>();

        var authorId = Guid.NewGuid();
        await authorsRepository.EnsureCreatedAsync(authorId);

        var viewerId = Guid.NewGuid();
        await authorsRepository.EnsureCreatedAsync(viewerId);

        await discussionsRepository.CreateAsync(viewerId, "Discussion 1", "bla bla bla");
        var discussionId = await discussionsRepository.CreateAsync(authorId, "Discussion 2", "bla bla bla");

        await reactionsRepository.SetAsync(discussionId, authorId, new HashSet<string> { "thumb-up", "heart" });
        await reactionsRepository.SetAsync(discussionId, viewerId, new HashSet<string> { "thumb-up", "rainbow" });

        // Act
        var discussion = await discussionsRepository.GetByIdAsync(discussionId, viewerId);

        // Assert
        discussion.ViewerReactions.Should().BeEquivalentTo("thumb-up", "rainbow");
        discussion.ReactionCounters.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["thumb-up"] = 2,
            ["heart"] = 1,
            ["rainbow"] = 1
        });
    }
}