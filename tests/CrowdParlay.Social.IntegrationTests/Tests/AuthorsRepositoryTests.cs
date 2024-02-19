using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Social.IntegrationTests.Tests;

public class AuthorsRepositoryTests : IClassFixture<WebApplicationContext>
{
    private readonly IServiceProvider _services;

    public AuthorsRepositoryTests(WebApplicationContext context) => _services = context.Services;

    [Fact(DisplayName = "Get user by ID returns user")]
    public async Task GetAuthorById_Positive()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var authors = scope.ServiceProvider.GetRequiredService<IAuthorRepository>();
        var expected = await authors.CreateAsync(
            id: Guid.NewGuid(),
            username: "compartmental",
            displayName: "Степной ишак",
            avatarUrl: null);

        // Act
        var actual = await authors.GetByIdAsync(expected.Id);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}