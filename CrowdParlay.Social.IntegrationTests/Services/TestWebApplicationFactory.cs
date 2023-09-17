using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Social.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // ReSharper disable once InconsistentNaming
    private readonly Neo4jTestConfiguration _neo4jConfiguration;

    // ReSharper disable once InconsistentNaming
    public TestWebApplicationFactory(Neo4jTestConfiguration neo4jConfiguration) =>
        _neo4jConfiguration = neo4jConfiguration;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["NEO4J_USERNAME"] = _neo4jConfiguration.Username,
            ["NEO4J_PASSWORD"] = _neo4jConfiguration.Password,
            ["NEO4J_URI"] = $"http://neo4j:{_neo4jConfiguration.Port}"
        }));
    }
}
