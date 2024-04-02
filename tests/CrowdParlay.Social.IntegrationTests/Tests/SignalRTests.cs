using CrowdParlay.Social.Api.Hubs;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class SignalRTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;
    private readonly HttpClient _client = context.Server.CreateClient();
    private readonly HttpMessageHandler _handler = context.Server.CreateHandler();

    [Fact(DisplayName = "Listen to new comments in discussion")]
    public async Task ListenToNewCommentsInDiscussion()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var commentsHub = scope.ServiceProvider.GetRequiredService<IHubContext<CommentsHub>>();
        var newComments = new List<CommentDto>();

        var discussionId = new Guid("6ef436dc-8e38-4a4b-b0e7-ff9fcd55ac0e");
        var expectedComment = new CommentDto
        {
            Id = Guid.NewGuid(),
            Content = "Sample comment.",
            Author = new AuthorDto
            {
                Id = new Guid("df194a2d-368c-43ea-b48d-66042f74691d"),
                Username = "sample_author",
                DisplayName = "Sample Author",
                AvatarUrl = null
            },
            CreatedAt = DateTimeOffset.Now,
            ReplyCount = 0,
            FirstRepliesAuthors = []
        };

        // Act
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_client.BaseAddress!, $"/api/v1/hubs/comments?discussionId={discussionId}"), options =>
            {
                options.Transports = HttpTransportType.ServerSentEvents;
                options.HttpMessageHandlerFactory = _ => _handler;
            })
            .Build();

        await connection.StartAsync();

        connection.On<CommentDto>(
            CommentsHub.Events.NewComment.ToString(),
            comment => newComments.Add(comment));

        await commentsHub.Clients
            .Group(CommentsHub.GroupNames.NewCommentInDiscussion(discussionId))
            .SendCoreAsync(CommentsHub.Events.NewComment.ToString(), [expectedComment]);

        await Task.Delay(1000);
        await connection.StopAsync();

        // Assert
        newComments.Should().BeEquivalentTo([expectedComment]);
    }
}