using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Neo4jClient;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class CommentRepository : ICommentRepository
{
    private readonly IGraphClient _graphClient;

    public CommentRepository(IGraphClient graphClient) => _graphClient = graphClient;

    public async Task<CommentDto> GetByIdAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .Match("(comment:Comment { Id: $id })-[:AUTHORED_BY]->(author:Author)")
            .OptionalMatch("(replyAuthor:Author)<-[:AUTHORED_BY]-(reply:Comment)-[:REPLIES_TO]->(comment)")
            .With(
                """
                *, COUNT(reply) AS replyCount,
                CASE WHEN COUNT(reply) > 0 THEN COLLECT(DISTINCT {
                    Id: replyAuthor.Id,
                    Username: replyAuthor.Username,
                    DisplayName: replyAuthor.DisplayName,
                    AvatarUrl: replyAuthor.AvatarUrl
                })[0..3] ELSE [] END AS firstRepliesAuthors
                """)
            .With(
                """
                {
                    Id: comment.Id,
                    Content: comment.Content,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    },
                    CreatedAt: datetime(comment.CreatedAt),
                    ReplyCount: replyCount,
                    FirstRepliesAuthors: firstRepliesAuthors
                }
                AS comment
                """)
            .Return<CommentDto>("comment")
            .ResultsAsync;

        return
            results.SingleOrDefault()
            ?? throw new NotFoundException();
    }

    public async Task<IEnumerable<CommentDto>> SearchAsync(Guid? discussionId, Guid? authorId, int page, int size)
    {
        var query = _graphClient.Cypher.WithParams(new { discussionId, authorId });

        var matchSelector = authorId is not null
            ? "(author:Author { Id: $authorId })<-[:AUTHORED_BY]-(comment:Comment)"
            : "(author:Author)<-[:AUTHORED_BY]-(comment:Comment)";

        if (discussionId is not null)
            matchSelector += "-[:REPLIES_TO]->(discussion:Discussion { Id: $discussionId })";

        return await query
            .Match(matchSelector)
            .With("*")
            .OrderBy("comment.CreatedAt")
            .Skip(page * size)
            .Limit(size)
            .OptionalMatch("(replyAuthor:Author)<-[:AUTHORED_BY]-(reply:Comment)-[:REPLIES_TO]->(comment)")
            .With(
                """
                *, COUNT(reply) AS replyCount,
                CASE WHEN COUNT(reply) > 0 THEN COLLECT(DISTINCT {
                    Id: replyAuthor.Id,
                    Username: replyAuthor.Username,
                    DisplayName: replyAuthor.DisplayName,
                    AvatarUrl: replyAuthor.AvatarUrl
                })[0..3] ELSE [] END AS firstRepliesAuthors
                """)
            .With(
                """
                {
                    Id: comment.Id,
                    Content: comment.Content,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    },
                    CreatedAt: datetime(comment.CreatedAt),
                    ReplyCount: replyCount,
                    FirstRepliesAuthors: firstRepliesAuthors
                }
                AS comment
                """)
            .Return<CommentDto>("comment")
            .ResultsAsync;
    }

    public async Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content)
    {
        var results = await _graphClient.Cypher
            .WithParams(new
            {
                authorId,
                content,
                discussionId
            })
            .Match("(author:Author {Id: $authorId})")
            .Match("(discussion:Discussion {Id: $discussionId})")
            .Create(
                """
                (comment:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                """)
            .Create("(discussion)<-[:REPLIES_TO]-(comment)-[:AUTHORED_BY]->(author)")
            .Return<CommentDto>("comment")
            .ResultsAsync;

        return results.Single();
    }

    public async Task<IEnumerable<CommentDto>> GetRepliesToCommentAsync(Guid parentCommentId, int page, int size)
    {
        return await _graphClient.Cypher
            .WithParams(new { parentCommentId })
            .Match("(commentAuthor:Author)<-[:AUTHORED_BY]-(comment:Comment)-[:REPLIES_TO]->(parent:Comment { Id: $parentCommentId })")
            .With("*")
            .OrderBy("comment.CreatedAt")
            .Skip(page * size)
            .Limit(size)
            .OptionalMatch("(replyAuthor:Author)<-[:AUTHORED_BY]-(reply:Comment)-[:REPLIES_TO]->(comment)")
            .With(
                """
                *, COUNT(reply) AS replyCount,
                CASE WHEN COUNT(reply) > 0 THEN COLLECT(DISTINCT {
                    Id: replyAuthor.Id,
                    Username: replyAuthor.Username,
                    DisplayName: replyAuthor.DisplayName,
                    AvatarUrl: replyAuthor.AvatarUrl
                })[0..3] ELSE [] END AS firstRepliesAuthors
                """)
            .With(
                """
                {
                    Id: comment.Id,
                    Content: comment.Content,
                    Author: {
                        Id: commentAuthor.Id,
                        Username: commentAuthor.Username,
                        DisplayName: commentAuthor.DisplayName,
                        AvatarUrl: commentAuthor.AvatarUrl
                    },
                    CreatedAt: datetime(comment.CreatedAt),
                    ReplyCount: replyCount,
                    FirstRepliesAuthors: firstRepliesAuthors
                }
                AS comment
                """)
            .Return<CommentDto>("comment")
            .ResultsAsync;
    }

    public async Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        var results = await _graphClient.Cypher
            .WithParams(new
            {
                authorId,
                content,
                parentCommentId
            })
            .Match("(replyAuthor:Author {Id: $authorId})")
            .Match("(parent:Comment {Id: $parentCommentId})")
            .Create(
                """
                (reply:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                """)
            .Create("(parent)<-[:REPLIES_TO]-(reply)-[:AUTHORED_BY]->(replyAuthor)")
            .With(
                """
                {
                    Id: reply.Id,
                    Content: reply.Content,
                    Author: {
                        Id: replyAuthor.Id,
                        Username: replyAuthor.Username,
                        DisplayName: replyAuthor.DisplayName,
                        AvatarUrl: replyAuthor.AvatarUrl
                    },
                    CreatedAt: reply.CreatedAt,
                    ReplyCount: 0,
                    FirstRepliesAuthors: []
                }
                AS reply
                """)
            .Return<CommentDto>("reply")
            .ResultsAsync;

        return results.Single();
    }

    public async Task DeleteAsync(Guid id)
    {
        var results = await _graphClient.Cypher
            .WithParams(new { id })
            .OptionalMatch("(comment:Comment { Id: $id })")
            .DetachDelete("comment")
            .Return<bool>("COUNT(comment) = 0")
            .ResultsAsync;

        if (results.Single())
            throw new NotFoundException();
    }
}