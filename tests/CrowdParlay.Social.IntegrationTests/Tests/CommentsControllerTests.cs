using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Communication;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.DTOs.Comment;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using FluentAssertions;
using MassTransit.Testing;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class CommentsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public CommentsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _harness = context.Harness;
    }

    [Fact(DisplayName = "Get comments by author returns comments")]
    public async Task GetCommentsByAuthor_Positive()
    {
        var authorId = Guid.NewGuid();
        
        // Create author
        var @event = new UserCreatedEvent(
            UserId: authorId.ToString(),
            Username: "maxaytt",
            DisplayName: "Rebus",
            AvatarUrl: null);

        await _harness.Bus.Publish(@event);
        _client.DefaultRequestHeaders.Add("X-UserId", @event.UserId);

        // Create top-level comment
        var createFirstCommentResponse =
            await _client.PostAsJsonAsync("api/v1/comments", new CommentRequest("Top-level comment 1!"));
        createFirstCommentResponse.StatusCode.Should().Be(HttpStatusCode.Created, "top-level comment cannot be created");
        
        var createSecondCommentResponse =
            await _client.PostAsJsonAsync("api/v1/comments", new CommentRequest("Top-level comment 2!"));
        createSecondCommentResponse.StatusCode.Should().Be(HttpStatusCode.Created, "top-level comment cannot be created");
        
        // Get Comments by author
        var getCommentsResponse =
            await _client.GetAsync($"api/v1/comments/?authorId={@event.UserId}&page=0&size=3");
        var comments = await getCommentsResponse.Content.ReadFromJsonAsync<IEnumerable<CommentDto>>();

        var commentList = comments.Should().NotBeNullOrEmpty()
            .And.Subject.ToList();

        commentList.Should().HaveCount(2)
            .And.OnlyContain(comment => comment.ReplyCount == 0)
            .And.OnlyContain(comment => comment.Author.Id == authorId)
            .And.Contain(comment => comment.Content == "Top-level comment 1!")
            .And.Contain(comment => comment.Content == "Top-level comment 2!");
    }
}