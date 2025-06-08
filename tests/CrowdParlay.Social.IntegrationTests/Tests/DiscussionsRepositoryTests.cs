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
        string[] expectedDiscussionIds =
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
        var discussionsService = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();

        const string heart = "\u2764\ufe0f";
        const string thumbUp = "\ud83d\udc4d";
        const string thumbDown = "\ud83d\udc4e";

        var authorId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();

        await discussionsService.CreateAsync(viewerId, "Discussion 1", "bla bla bla");
        var discussion = await discussionsService.CreateAsync(authorId, "Discussion 2", "bla bla bla");

        await discussionsService.SetReactionsAsync(discussion.Id, authorId, new HashSet<string> { thumbUp, heart });
        await discussionsService.SetReactionsAsync(discussion.Id, viewerId, new HashSet<string> { thumbUp, thumbDown });

        // Act
        discussion = await discussionsService.GetByIdAsync(discussion.Id, viewerId);

        // Assert
        discussion.ViewerReactions.Should().BeEquivalentTo(thumbUp, thumbDown);
        discussion.ReactionCounters.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { thumbUp, 2 },
            { heart, 1 },
            { thumbDown, 1 }
        });
    }

    [Theory(DisplayName = "Create discussions in parallel")]
    [InlineData(1), InlineData(2), InlineData(100)]
    public async Task CreateDiscussionsInParallel(int degreeOfParallelism)
    {
        var tasks = Enumerable.Range(0, degreeOfParallelism).Select(i => Task.Run(async () =>
        {
            await using var scope = _services.CreateAsyncScope();
            var discussionsService = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();
            await discussionsService.CreateAsync(Guid.NewGuid(), $"Discussion {i}", $"Content {i}");
        }));
        
        await Task.WhenAll(tasks);
    }
}