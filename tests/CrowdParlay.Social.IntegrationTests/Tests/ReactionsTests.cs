using CrowdParlay.Social.Domain.Abstractions;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class ReactionsTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
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
        var discussionsService = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();
        var discussionsRepository = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        var viewerId = Guid.NewGuid();
        var discussion = await discussionsService.CreateAsync(viewerId, "Title", "Description");

        // Act
        await discussionsRepository.SetReactionsAsync(discussion.Id, viewerId, heartAndOldInvalid);
        await discussionsRepository.SetReactionsAsync(discussion.Id, viewerId, new HashSet<string>());
        await discussionsRepository.SetReactionsAsync(discussion.Id, viewerId, thumbs);

        var reactWithNewInvalid = async () => await discussionsService.SetReactionsAsync(discussion.Id, viewerId, thumbsAndNewInvalid);
        var reactions = await discussionsService.GetReactionsAsync(discussion.Id, viewerId);

        // Assert
        await reactWithNewInvalid.Should().ThrowAsync<ForbiddenException>();
        reactions.Should().BeEquivalentTo(thumbs);
    }

    [Fact(DisplayName = "Setting reactions overwrites existing reactions")]
    public async Task SetReactions_OverwritesExistingReactions()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussionsService = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();

        var viewerId = Guid.NewGuid();
        var discussion = await discussionsService.CreateAsync(viewerId, "Title", "Description");

        const string eggplant = "\ud83c\udf46";
        const string woozyFace = "\ud83e\udd74";
        const string nailPolish = "\ud83d\udc85";
        const string redHeart = "\u2764\ufe0f";

        // Act
        await discussionsService.SetReactionsAsync(discussion.Id, viewerId, new HashSet<string> { eggplant });
        await discussionsService.SetReactionsAsync(discussion.Id, viewerId, new HashSet<string> { eggplant, woozyFace });
        await discussionsService.SetReactionsAsync(discussion.Id, viewerId, new HashSet<string> { eggplant, woozyFace, nailPolish });
        await discussionsService.SetReactionsAsync(discussion.Id, viewerId, new HashSet<string> { woozyFace, redHeart });

        // Assert
        discussion = await discussionsService.GetByIdAsync(discussion.Id, viewerId);
        discussion.ViewerReactions.Should().BeEquivalentTo(woozyFace, redHeart);
        discussion.ReactionCounters.Should().BeEquivalentTo(new Dictionary<string, int>
        {
            { eggplant, 0 },
            { woozyFace, 1 },
            { nailPolish, 0 },
            { redHeart, 1 }
        });
    }
}