using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs.Comment;
using CrowdParlay.Social.Application.Exceptions;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class CommentRepository : ICommentRepository
{
    private readonly IGraphClient _graphClient;

    public CommentRepository(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<CommentDto> FindAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .Match("(c:Comment { Id: $id })-[:AUTHORED_BY]->(a:Author)")
            .OptionalMatch("(ra:Author)<-[:AUTHORED_BY]-(r:Comment)-[:REPLIES_TO]->(c)")
            .With(
                """
                c, a, COUNT(r) AS rc,
                CASE WHEN COUNT(r) > 0 THEN COLLECT(DISTINCT {
                    Id: ra.Id,
                    Username: ra.Username,
                    DisplayName: ra.DisplayName,
                    AvatarUrl: ra.AvatarUrl
                })[0..3] ELSE [] END AS fras
                """)
            .With(
                """
                {
                    Id: c.Id,
                    Content: c.content,
                    Author: {
                        Id: a.Id,
                        Username: a.Username,
                        DisplayName: a.DisplayName,
                        AvatarUrl: a.AvatarUrl
                    },
                    CreatedAt: c.CreatedAt,
                    ReplyCount: rc,
                    FirstRepliesAuthors: fras
                }
                AS c
                """)
            .Return<CommentDto>("c")
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }

    public async Task<IEnumerable<CommentDto>> FindByAuthorAsync(Guid authorId) => await _graphClient.Cypher
        .WithParams(new { authorId })
        .Match("(c:Comment)-[:AUTHORED_BY]->(a:Author { Id: $authorId })")
        .OptionalMatch("(ra:Author)<-[:AUTHORED_BY]-(r:Comment)-[:REPLIES_TO]->(c)")
        .With(
            """
            c, a, COUNT(r) AS rc,
            CASE WHEN COUNT(r) > 0 THEN COLLECT(DISTINCT {
                Id: ra.Id,
                Username: ra.Username,
                DisplayName: ra.DisplayName,
                AvatarUrl: ra.AvatarUrl
            })[0..3] ELSE [] END AS fras
            """)
        .With(
            """
            {
                Id: c.Id,
                Content: c.content,
                Author: {
                    Id: a.Id,
                    Username: a.Username,
                    DisplayName: a.DisplayName,
                    AvatarUrl: a.AvatarUrl
                },
                ReplyCount: rc,
                FirstRepliesAuthors: fras
            }
            AS c
            """)
        .Return<CommentDto>("c")
        .ResultsAsync;

    public async Task<CommentDto> CreateAsync(Guid authorId, string content)
    {
        var result = await _graphClient.Cypher
            .WithParams(new
            {
                authorId,
                content
            })
            .Match("(a:Author {Id: $authorId})")
            .Create(
                """
                (c:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                """)
            .Create("(c)-[:AUTHORED_BY]->(a)")
            .Return<CommentDto>("c")
            .ResultsAsync;

        return result.Single();
    }

    public async Task<CommentDto> ReplyAsync(Guid authorId, Guid targetCommentId, string content)
    {
        var result = await _graphClient.Cypher
            .WithParams(new
            {
                authorId,
                content,
                targetCommentId
            })
            .Match("(a:Author {Id: $authorId})")
            .Match("(c:Comment {Id: $targetCommentId})")
            .Create(
                """
                (r:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                """)
            .Create("(c)<-[:REPLIES_TO]-(r)-[:AUTHORED_BY]->(a)")
            .Return<CommentDto>("r")
            .ResultsAsync;

        return result.Single();
    }

    public async Task DeleteAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .OptionalMatch("(c:Comment { Id: $id })")
            .Delete("c")
            .Return<bool>("COUNT(c) = 0")
            .ResultsAsync;

        if (results.Single())
            throw new NotFoundException();
    }
}