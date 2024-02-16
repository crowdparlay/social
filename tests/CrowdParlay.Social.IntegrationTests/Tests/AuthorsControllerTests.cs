using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class AuthorsControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public AuthorsControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _services = context.Services;
    }

    [Fact(DisplayName = "Get user by ID returns user")]
    public async Task GetAuthorById_Positive()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var author = await authors.CreateAsync(
            id: Guid.NewGuid(),
            username: "compartmental",
            displayName: "Степной ишак",
            avatarUrl: null);

        // Act
        var message = await _client.GetAsync($"/api/v1/authors/{author.Id}");
        message.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert
        var response = await message.Content.ReadFromJsonAsync<AuthorDto>(GlobalSerializerOptions.SnakeCase);
        response.Should().BeEquivalentTo(author);
    }
}