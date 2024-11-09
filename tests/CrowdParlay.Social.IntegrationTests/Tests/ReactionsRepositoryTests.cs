using CrowdParlay.Social.Domain.Abstractions;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class ReactionsRepositoryTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Set reactions")]
    public async Task SetReactions_OnlyWorksWithAllowedReactionSets()
    {
        // Arrange
        const string heart = "\u2764\ufe0f";
        const string thumbUp = "\ud83d\udc4d";
        const string thumbDown = "\ud83d\udc4e";
        const string oldInvalid = "This reaction is not allowed, but somebody has already reacted with it when it used to be allowed";
        const string newInvalid = "This reaction is not allowed, and nobody used it";

        HashSet<string> thumbs = [thumbUp, thumbDown];
        HashSet<string> thumbsAndNewInvalid = [..thumbs, newInvalid];
        HashSet<string> heartAndOldInvalid = [heart, oldInvalid];

        await using var scope = _services.CreateAsyncScope();
        var authorsRepository = scope.ServiceProvider.GetRequiredService<IAuthorsRepository>();
        var discussionsRepository = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var reactionsRepository = scope.ServiceProvider.GetRequiredService<IReactionsRepository>();
        var reactionsService = scope.ServiceProvider.GetRequiredService<IReactionsService>();

        var viewerId = Guid.NewGuid();
        await authorsRepository.EnsureCreatedAsync(viewerId);

        var discussionId = await discussionsRepository.CreateAsync(viewerId, "Title", "Description");
        await reactionsRepository.SetAsync(discussionId, viewerId, new HashSet<string> { oldInvalid });

        // Act
        await reactionsService.SetAsync(discussionId, viewerId, heartAndOldInvalid);
        await reactionsService.SetAsync(discussionId, viewerId, new HashSet<string>());
        await reactionsService.SetAsync(discussionId, viewerId, thumbs);

        var reactWithNewInvalid = async () => await reactionsService.SetAsync(discussionId, viewerId, thumbsAndNewInvalid);
        var reactions = await reactionsService.GetAsync(discussionId, viewerId);

        // Assert
        await reactWithNewInvalid.Should().ThrowAsync<ForbiddenException>();
        reactions.Should().BeEquivalentTo(thumbs);
    }
}