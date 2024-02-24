using System.Net.Http.Json;
using System.Text;
using CrowdParlay.Social.Api.Hubs;
using CrowdParlay.Social.Api.v1.DTOs;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class SignalRTests(WebApplicationContext context) : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services = context.Services;
    private readonly HttpClient _client = context.Server.CreateClient();
    private readonly HttpMessageHandler _handler = context.Server.CreateHandler();

    [Fact(DisplayName = "Listen to new comments in discussion")]
    public async Task ListenToNewCommentsInDiscussion()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();

        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var discussions = scope.ServiceProvider.GetRequiredService<IDiscussionRepository>();

        var author = await authors.CreateAsync(Guid.NewGuid(), "test123", "Test author", null);
        var discussion = await discussions.CreateAsync(author.Id, "Test discussion", "Test discussion");
        var newComments = new List<CommentDto>();

        // Act
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_client.BaseAddress!, $"/api/v1/hubs/comments?discussionId={discussion.Id}"), options =>
            {
                options.Transports = HttpTransportType.ServerSentEvents;
                options.HttpMessageHandlerFactory = _ => _handler;
            })
            .Build();

        await connection.StartAsync();

        connection.On<CommentDto>(
            CommentsHub.Events.NewComment.ToString(),
            comment => newComments.Add(comment));

        var request = JsonSerializer.Serialize(
            new CommentRequest(discussion.Id, "Test comment"),
            GlobalSerializerOptions.SnakeCase);

        var accessToken = Authorization.ProduceAccessToken(author.Id);
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/v1/comments")
        {
            Content = new StringContent(request, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        });

        var comment = await response.Content.ReadFromJsonAsync<CommentDto>(GlobalSerializerOptions.SnakeCase);
        await Task.Delay(1000);

        // Assert
        newComments.Should().BeEquivalentTo([comment]);
    }
}