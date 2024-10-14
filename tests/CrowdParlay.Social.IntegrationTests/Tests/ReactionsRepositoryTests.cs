using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.ValueObjects;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class ReactionsRepositoryTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Add reactions")]
    public async Task AddReaction_MultipleTimes_AddsReaction()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussionsRepository = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var reactionsRepository = scope.ServiceProvider.GetRequiredService<IReactionsRepository>();

        var authorId = Guid.NewGuid();
        var discussionId = await discussionsRepository.CreateAsync(Guid.NewGuid(), "Title", "Description");

        var thumbUp = new Reaction("\ud83d\udc4d");
        var thumbDown = new Reaction("\ud83d\udc4e");
        
        // Act
        for (var i = 0; i < 4; i++)
            await reactionsRepository.AddAsync(authorId, discussionId, thumbUp);
        
        for (var i = 0; i < 4; i++)
            await reactionsRepository.AddAsync(authorId, discussionId, thumbDown);
        
        var reactions = await reactionsRepository.GetAllAsync(authorId, discussionId);

        // Assert
        reactions.Should().BeEquivalentTo([thumbUp, thumbDown]);
    }
}