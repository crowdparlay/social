using System.Net.Http.Json;
using System.Text.Json;
using CrowdParlay.Social.Domain.Abstractions;
using MongoDB.Bson;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class HttpContractsTests(WebApplicationContext context) : IAssemblyFixture<WebApplicationContext>
{
    private readonly HttpClient _client = context.Server.CreateClient();
    private readonly IServiceProvider _services = context.Services;

    [Fact(DisplayName = "Preflight request to SignalR returns specific CORS allowed origins")]
    public async Task PreflightRequestToSignalRReturnsSpecificCorsAllowedOrigins()
    {
        // Arrange
        const string origin = "http://localhost:1234";
        var uri = $"/api/v1/hubs/comments/negotiate?discussionId={Guid.NewGuid()}&negotiateVersion=1";
        var request = new HttpRequestMessage(HttpMethod.Options, uri);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("Access-Control-Allow-Credentials").WhoseValue.Should().ContainSingle("true");
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin").WhoseValue.Should().Contain(origin);
    }

    [Fact(DisplayName = "Resource not found error is returned as RFC 7807 problem details")]
    public async Task NotFoundIsReturnedAsProblemDetails()
    {
        // Arrange
        const string expected = """{"status":404,"detail":"The requested resource doesn\u0027t exist."}""";

        // Act
        var response = await _client.GetAsync($"/api/v1/comments/{ObjectId.Empty}");
        var actual = await response.Content.ReadAsStringAsync();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact(DisplayName = "Responses are JSON-serialized in snake case")]
    public async Task ResponsesAreJsonSerializedInSnakeCase()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionsRepository>();
        var discussionId = await discussions.CreateAsync(Guid.NewGuid(), "Title", "Content");

        // Act
        var response = await _client.GetAsync($"/api/v1/discussions/{discussionId}");
        var rawResponseContent = await response.Content.ReadAsStringAsync();
        var discussion = await response.Content.ReadFromJsonAsync<DiscussionResponse>(GlobalSerializerOptions.SnakeCase);
        var serializedInSnakeCase = JsonSerializer.Serialize(discussion, GlobalSerializerOptions.SnakeCase);

        // Assert
        rawResponseContent.Should().Be(serializedInSnakeCase);
    }
}