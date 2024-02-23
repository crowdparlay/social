namespace CrowdParlay.Social.IntegrationTests;

// ReSharper disable once InconsistentNaming
public record Neo4jConfiguration(string Uri, string Username, string Password);
public record RedisConfiguration(string ConnectionString);