using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CrowdParlay.Communication;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using FluentAssertions;
using MassTransit.Testing;
using Authorization = CrowdParlay.Social.IntegrationTests.Services.Authorization;

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

        // Create discussion
        var serializedCreateDiscussionRequest = JsonSerializer.Serialize(
            new DiscussionRequest("Test discussion", "Something"),
            GlobalSerializerOptions.SnakeCase);

        var accessToken = Authorization.ProduceAccessToken(@event.UserId);
        var createDiscussionResponse = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/v1/discussions")
        {
            Content = new StringContent(serializedCreateDiscussionRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        });

        createDiscussionResponse.Should().HaveStatusCode(HttpStatusCode.Created);
        var discussion = await createDiscussionResponse.Content.ReadFromJsonAsync<DiscussionDto>(GlobalSerializerOptions.SnakeCase);

        // Create parent comment
        var serializedCreateCommentRequest = JsonSerializer.Serialize(
            new CommentRequest(discussion!.Id, "Top-level comment!"),
            GlobalSerializerOptions.SnakeCase);

        var createCommentResponse = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/v1/comments")
        {
            Content = new StringContent(serializedCreateCommentRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        });

        createCommentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var comment = await createCommentResponse.Content.ReadFromJsonAsync<CommentDto>(GlobalSerializerOptions.SnakeCase);

        // Create reply comment
        var serializedCreateReplyRequest = JsonSerializer.Serialize(
            new ReplyRequest("Reply comment."),
            GlobalSerializerOptions.SnakeCase);

        var createReplyResponse = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"/api/v1/comments/{comment!.Id}")
        {
            Content = new StringContent(serializedCreateReplyRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        });

        createReplyResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Get parent comment
        var getCommentResponse = await _client.GetAsync($"/api/v1/comments/{comment.Id}");
        getCommentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        comment = await getCommentResponse.Content.ReadFromJsonAsync<CommentDto>(GlobalSerializerOptions.SnakeCase);

        comment!.ReplyCount.Should().Be(1);
        comment.FirstRepliesAuthors.Should().ContainSingle(x => x.Id == Guid.Parse(@event.UserId));
    }
}