using System.Text.Json;
using CrowdParlay.Social.Api.Services;

namespace CrowdParlay.Social.IntegrationTests;

public static class GlobalSerializerOptions
{
    public static JsonSerializerOptions SnakeCase { get; } = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(), 
        DictionaryKeyPolicy = new SnakeCaseJsonNamingPolicy() 
    };
}