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

public class DiscussionsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public DiscussionsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _harness = context.Harness;
    }

    [Fact(DisplayName = "Get discussion by ID returns discussion with author")]
    public async Task GetDiscussionByIdHasAuthor_Positive()
    {
        // Create author
        var @event = new UserCreatedEvent(
            UserId: Guid.NewGuid().ToString(),
            Username: "zendef566t",
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
        var discussionId = (await createDiscussionResponse.Content.ReadFromJsonAsync<DiscussionDto>(GlobalSerializerOptions.SnakeCase))!.Id;

        // Get discussion
        var getDiscussionResponse = await _client.GetAsync($"/api/v1/discussions/{discussionId}");
        getDiscussionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var discussion = await getDiscussionResponse.Content.ReadFromJsonAsync<CommentDto>(GlobalSerializerOptions.SnakeCase);

        discussion!.Author.Should().BeEquivalentTo(new AuthorDto
        {
            Id = Guid.Parse(@event.UserId),
            Username = @event.Username,
            DisplayName = @event.DisplayName,
            AvatarUrl = @event.AvatarUrl
        });
    }
}