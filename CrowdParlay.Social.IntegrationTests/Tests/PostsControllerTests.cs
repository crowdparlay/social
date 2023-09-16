using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class PostsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;

    public PostsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
    }

    [Fact(DisplayName = "Create post returns Status201Created")]
    public async Task CreatePost_Positive()
    {
        const string content = "В лесу подходит, как правило, медведь";
        var authorId = Guid.NewGuid(); //todo: should be real id from jwt claims
        var message = await _client.PostAsJsonAsync($"api/posts/{authorId}",content);

        message.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Fact(DisplayName = "Create post returns Status400BadRequest")]
    public async Task CreatePost_Negative()
    {
        const string content = "В лесу подходит, как правило, медведь";
        var authorId = Guid.NewGuid();
        var message = await _client.PostAsJsonAsync($"api/posts/{authorId}",content);

        message.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact(DisplayName = "Get post returns Status200Ok")]
    public async Task GetPost_Positive()
    {
        var postId = Guid.NewGuid(); //todo: should be real id(create post) 
        var message = await _client.GetAsync($"api/posts/{postId}");

        message.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Fact(DisplayName = "Get post returns Status400BadRequest")]
    public async Task GetPost_Negative()
    {
        var postId = Guid.NewGuid();
        var message = await _client.GetAsync($"api/posts/{postId}");

        message.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    //todo: get all
}