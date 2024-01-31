using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly IGraphClient _graphClient;

    public DiscussionRepository(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<DiscussionDto> GetByIdAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .Match("(d:Discussion { Id: $id })")
            .With(
                """
                {
                    Id: d.Id,
                    Title: d.Title,
                    Description: d.Description,
                    Author: {
                        Id: a.Id,
                        Username: a.Username,
                        DisplayName: a.DisplayName,
                        AvatarUrl: a.AvatarUrl
                    }
                }
                AS d
                """)
            .Return<DiscussionDto>("d")
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }

    public async Task<IEnumerable<DiscussionDto>> GetAllAsync() => await _graphClient.Cypher
        .Match("(d:Discussion)-[:AUTHORED_BY]->(a:Author)")
        .With(
            """
            {
                Id: d.Id,
                Title: d.Title,
                Description: d.Description,
                Author: {
                    Id: a.Id,
                    Username: a.Username,
                    DisplayName: a.DisplayName,
                    AvatarUrl: a.AvatarUrl
                }
            }
            AS d
            """)
        .Return<DiscussionDto>("d")
        .ResultsAsync;

    public async Task<IEnumerable<DiscussionDto>> GetByAuthorAsync(Guid authorId) => await _graphClient.Cypher
        .WithParams(new { authorId })
        .Match("(d:Discussion)-[:AUTHORED_BY]->(a:Author { Id: $authorId })")
        .With(
            """
            {
                Id: d.Id,
                Title: d.Title,
                Description: d.Description,
                Author: {
                    Id: a.Id,
                    Username: a.Username,
                    DisplayName: a.DisplayName,
                    AvatarUrl: a.AvatarUrl
                }
            }
            AS d
            """)
        .Return<DiscussionDto>("d")
        .ResultsAsync;

    public async Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description)
    {
        var results = await _graphClient.Cypher
            .WithParams(new
            {
                authorId,
                title,
                description
            })
            .Match("(a:Author {Id: $authorId})")
            .Create(
                """
                (d:Discussion {
                    Id: randomUUID(),
                    Title: $title,
                    Description: $description,
                    CreatedAt: datetime()
                })
                """)
            .Create("(d)-[:AUTHORED_BY]->(a)")
            .With(
                """
                {
                    Id: d.Id,
                    Title: d.Title,
                    Description: d.Description,
                    Author: {
                        Id: a.Id,
                        Username: a.Username,
                        DisplayName: a.DisplayName,
                        AvatarUrl: a.AvatarUrl
                    }
                }
                AS d
                """)
            .Return<DiscussionDto>("d")
            .ResultsAsync;

        return results.Single();
    }
}