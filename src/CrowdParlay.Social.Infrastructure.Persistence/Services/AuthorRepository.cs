using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class AuthorRepository : IAuthorRepository
{
    private readonly IGraphClient _graphClient;

    public AuthorRepository(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<AuthorDto> GetByIdAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .Match("(a:Author { Id: $id })")
            .Return<AuthorDto>("a")
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }

    public async Task<AuthorDto> CreateAsync(Guid id, string username, string displayName, string? avatarUrl)
    {
        var results = await _graphClient.Cypher
            .WithParams(new
            {
                id,
                username,
                displayName,
                avatarUrl
            })
            .Create(
                """
                (a:Author {
                    Id: $id,
                    Username: $username,
                    DisplayName: $displayName,
                    AvatarUrl: $avatarUrl
                })
                """)
            .Return<AuthorDto>("a")
            .ResultsAsync;

        return results.Single();
    }

    public async Task<AuthorDto> UpdateAsync(Guid id, string username, string displayName, string? avatarUrl)
    {
        var results = await _graphClient.Cypher
            .WithParams(new
            {
                id,
                username,
                displayName,
                avatarUrl
            })
            .Match("(a:Author { Id: $id })")
            .Set(
                """
                a.Username = $username,
                a.DisplayName = $displayName,
                a.AvatarUrl = $avatarUrl
                """)
            .Return<AuthorDto>("a")
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }

    public async Task DeleteAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .OptionalMatch("(a:Author { Id: $id })")
            .Delete("a")
            .Return<bool>("COUNT(a) = 0")
            .ResultsAsync;

        if (results.Single())
            throw new NotFoundException();
    }
}