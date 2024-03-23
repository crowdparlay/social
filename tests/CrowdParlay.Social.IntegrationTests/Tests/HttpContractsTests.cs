namespace CrowdParlay.Social.IntegrationTests.Tests;

public class HttpContractsTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client = context.Server.CreateClient();

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
        var response = await _client.GetAsync($"/api/v1/authors/{Guid.Empty}");
        var actual = await response.Content.ReadAsStringAsync();

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}