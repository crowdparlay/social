using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Communication;
using CrowdParlay.Social.Api.DTOs;
using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.DTOs.Comment;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using FluentAssertions;
using MassTransit.Testing;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class AuthorsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public AuthorsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _harness = context.Harness;
    }

    [Fact(DisplayName = "User created event creates author")]
    public async Task CreateAuthor_Positive()
    {
        var @event = new UserCreatedEvent(
            UserId: Guid.NewGuid().ToString(),
            Username: "compartmental",
            DisplayName: "Степной ишак",
            AvatarUrl: null);

        await _harness.Bus.Publish(@event);

        var message = await _client.GetAsync($"api/authors/{@event.UserId}");
        message.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await message.Content.ReadFromJsonAsync<AuthorDto>();
        response.Should().BeEquivalentTo(new AuthorDto
        {
            Id = Guid.Parse(@event.UserId),
            Username = @event.Username,
            DisplayName = @event.DisplayName,
            AvatarUrl = @event.AvatarUrl
        });
    }

    [Fact(DisplayName = "Reply to comment creates reply")]
    public async Task ReplyToComment_Positive()
    {
        // Create author
        var @event = new UserCreatedEvent(
            UserId: Guid.NewGuid().ToString(),
            Username: "zendet",
            DisplayName: "Z E N D E T",
            AvatarUrl: null);

        await _harness.Bus.Publish(@event);
        _client.DefaultRequestHeaders.Add("X-UserId", @event.UserId);

        // Create top-level comment
        var createCommentResponse = await _client.PostAsJsonAsync("api/comments", new CommentRequest("Top-level comment!"));
        createCommentResponse.StatusCode.Should().Be(HttpStatusCode.Created, "top-level comment cannot be created");

        // Create reply comment
        var comment = await createCommentResponse.Content.ReadFromJsonAsync<CommentDto>();
        var createReplyResponse = await _client.PostAsJsonAsync($"api/comments/{comment!.Id}/reply", new CommentRequest("Reply comment."));
        createReplyResponse.StatusCode.Should().Be(HttpStatusCode.Created, "reply comment cannot be created");

        // Get top-level comment
        var getCommentResponse = await _client.GetAsync($"api/comments/{comment.Id}");
        getCommentResponse.StatusCode.Should().Be(HttpStatusCode.OK, "top-level comment cannot be fetched");
        comment = await getCommentResponse.Content.ReadFromJsonAsync<CommentDto>();

        comment!.ReplyCount.Should().Be(1);
        comment.FirstRepliesAuthors.Should().ContainSingle(x => x.Id == Guid.Parse(@event.UserId));
    }
}