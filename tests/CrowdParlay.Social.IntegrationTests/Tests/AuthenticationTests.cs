using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Social.Api.v1.DTOs;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class AuthenticationTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client = context.Server.CreateClient();

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
        response.Should().HaveStatusCode(HttpStatusCode.Created);
    }
}