using System.Text.Json;
using CrowdParlay.Social.Api.Services;

namespace CrowdParlay.Social.Api;

public static class GlobalSerializerOptions
{
    public static readonly JsonSerializerOptions SnakeCase = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(), 
        DictionaryKeyPolicy = new SnakeCaseJsonNamingPolicy() 
    };
}