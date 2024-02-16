using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class DiscussionsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public DiscussionsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _services = context.Services;
    }

    [Fact(DisplayName = "Get discussion by ID returns discussion")]
    public async Task GetDiscussionByIdHasAuthor_Positive()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var author = await authors.CreateAsync(
            id: Guid.NewGuid(),
            username: "zendef566t",
            displayName: "Z E N D E T",
            avatarUrl: null);

        // Create discussion
        var serializedCreateDiscussionRequest = JsonSerializer.Serialize(
            new DiscussionRequest("Test discussion", "Something"),
            GlobalSerializerOptions.SnakeCase);

        var accessToken = Authorization.ProduceAccessToken(author.Id);
        var createDiscussionResponse = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/v1/discussions")
        {
            Content = new StringContent(serializedCreateDiscussionRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        });

        createDiscussionResponse.Should().HaveStatusCode(HttpStatusCode.Created);
        var discussionId = (await createDiscussionResponse.Content.ReadFromJsonAsync<DiscussionDto>(GlobalSerializerOptions.SnakeCase))!.Id;

        // Act
        var getDiscussionResponse = await _client.GetAsync($"/api/v1/discussions/{discussionId}");
        getDiscussionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var discussion = await getDiscussionResponse.Content.ReadFromJsonAsync<CommentDto>(GlobalSerializerOptions.SnakeCase);

        // Assert
        discussion!.Author.Should().BeEquivalentTo(author);
    }
}