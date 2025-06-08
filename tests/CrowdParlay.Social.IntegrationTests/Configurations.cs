namespace CrowdParlay.Social.IntegrationTests;

public record MongoDbConfiguration(string ConnectionString, string Database);
public record RedisConfiguration(string ConnectionString);