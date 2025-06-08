using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class AuthenticationTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly HttpClient _client = context.Server.CreateClient();
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Create a discussion providing access JWT as Bearer token")]
    public async Task CreateDiscussionWithAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/discussions");
        request.Content = JsonContent.Create(new DiscussionRequest("Short title", "Long description."));
        request.Headers.Add("Authorization", "Bearer " + Authorization.ProduceAccessToken(userId));

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "Search discussions returns discussions with viewer reactions")]
    public async Task SearchReactionsWithAccessToken()
    {
        // Arrange
        HashSet<string> reactions = ["\u2764\ufe0f", "\ud83c\udf08"];

        await using var scope = _services.CreateAsyncScope();
        var discussionsRepository = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();

        var authorId = Guid.NewGuid();

        var discussionId = await discussionsRepository.CreateAsync(authorId, "Test discussion", "Description.");
        await discussionsRepository.SetReactionsAsync(discussionId, authorId, reactions);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/discussions?offset=0&count=10&authorId={authorId}");
        request.Headers.Add("Authorization", "Bearer " + Authorization.ProduceAccessToken(authorId));

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<Page<DiscussionResponse>>(GlobalSerializerOptions.SnakeCase);
        page?.Items.Should().ContainSingle().Which.ViewerReactions.Should().BeEquivalentTo(reactions);
    }
}