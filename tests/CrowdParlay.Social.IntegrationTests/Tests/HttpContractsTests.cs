namespace CrowdParlay.Social.IntegrationTests.Tests;

public class HttpContractsTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client = context.Server.CreateClient();

    [Fact(DisplayName = "Resource not found error is returned as RFC 7807 problem details")]
    public async Task NotFoundIsReturnedAsProblemDetails()
    {
        const string expected = """{"status":404,"detail":"The requested resource doesn\u0027t exist."}""";
        var response = await _client.GetAsync($"/api/v1/authors/{Guid.Empty}");
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equivalent(expected, actual);
    }
}