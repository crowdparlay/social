using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Communication;
using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using FluentAssertions;
using MassTransit.Testing;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class AuthorsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public AuthorsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _harness = context.Harness;
    }

    [Fact(DisplayName = "User created event creates author")]
    public async Task CreateAuthor_Positive()
    {
        var @event = new UserCreatedEvent(
            UserId: Guid.NewGuid().ToString(),
            Username: "compartmental",
            DisplayName: "Степной ишак",
            AvatarUrl: null);

        await _harness.Bus.Publish(@event);

        var message = await _client.GetAsync($"api/authors/{@event.UserId}");
        message.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await message.Content.ReadFromJsonAsync<AuthorDto>();
        response.Should().BeEquivalentTo(new AuthorDto
        {
            Id = Guid.Parse(@event.UserId),
            Username = @event.Username,
            DisplayName = @event.DisplayName,
            AvatarUrl = @event.AvatarUrl
        });
    }

    [Fact(DisplayName = "Reply to comment creates reply")]
    public async Task ReplyToComment_Positive()
    {
        var @event = new UserCreatedEvent(
            UserId: Guid.NewGuid().ToString(),
            Username: "compartmental",
            DisplayName: "Степной ишак",
            AvatarUrl: null);

        await _harness.Bus.Publish(@event);

        var message = await _client.GetAsync($"api/authors/{@event.UserId}");
        message.StatusCode.Should().Be(HttpStatusCode.OK);

        var createReplyCommand = new CreateReplyToCommentCommand(
            AuthorId: authorId,
            Content: "Reply",
            InReplyToCommentId: s);
        await _harness.Bus.Publish(@event);

        var message = await _client.GetAsync($"api/authors/{@event.UserId}");
        message.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await message.Content.ReadFromJsonAsync<AuthorDto>();
        response.Should().BeEquivalentTo(new AuthorDto
        {
            Id = Guid.Parse(@event.UserId),
            Username = @event.Username,
            DisplayName = @event.DisplayName,
            AvatarUrl = @event.AvatarUrl
        });
    }
}