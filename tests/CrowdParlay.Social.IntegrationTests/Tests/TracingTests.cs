using System.Diagnostics;
using CrowdParlay.Social.Application.Services;
using CrowdParlay.Social.IntegrationTests.Services;
using OpenTelemetry.Exporter;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class TracingTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;

    [Fact]
    public async Task UsersServiceProducesTraces()
    {
        // Arrange
        var storage = _services.GetRequiredService<OpenTelemetryInMemoryStorage>();
        var exporter = _services.GetRequiredService<InMemoryExporter<Activity>>();

        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUsersService>();

        var userId = Guid.NewGuid();

        // Act
        await users.GetByIdAsync(userId);
        exporter.ForceFlush();

        // Assert
        storage.Activities.Should()
            .ContainSingle(activity =>
                activity.Source.Name == typeof(UsersServiceMock).FullName
                && activity.OperationName == nameof(UsersServiceMock.GetByIdAsync))
            .Which.Tags.Should().Contain("parameters.id", $"\"{userId}\"");
    }

    [Fact]
    public async Task CommentsServiceProducesTraces()
    {
        // Arrange
        var storage = _services.GetRequiredService<OpenTelemetryInMemoryStorage>();
        var exporter = _services.GetRequiredService<InMemoryExporter<Activity>>();

        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsService>();
        var comments = scope.ServiceProvider.GetRequiredService<ICommentsService>();

        var authorId = Guid.NewGuid();
        var discussion = await discussions.CreateAsync(authorId, "Discussion 1", "Content.");
        await comments.ReplyToDiscussionAsync(discussion.Id, authorId, "Comment.");

        // Act
        await comments.GetRepliesAsync(discussion.Id, true, null, 0, 100);
        exporter.ForceFlush();

        // Assert
        storage.Activities.Should()
            .ContainSingle(activity =>
                activity.Source.Name == typeof(CommentsService).FullName
                && activity.OperationName == nameof(CommentsService.GetRepliesAsync))
            .Which.Tags.Should().Contain("parameters.subjectId", $"\"{discussion.Id}\"")
            .And.Contain("parameters.flatten", "true")
            .And.Contain("parameters.viewerId", "null")
            .And.Contain("parameters.offset", "0")
            .And.Contain("parameters.count", "100");
    }
}