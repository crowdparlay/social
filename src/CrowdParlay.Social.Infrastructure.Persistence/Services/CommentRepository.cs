using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class CommentRepository : ICommentRepository
{
    private readonly IDriver _driver;

    public CommentRepository(IDriver driver) => _driver = driver;

    public async Task<CommentDto> GetByIdAsync(Guid id)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (comment:Comment { Id: $id })-[:AUTHORED_BY]->(author:Author)
                OPTIONAL MATCH (replyAuthor:Author)<-[:AUTHORED_BY]-(reply:Comment)-[:REPLIES_TO]->(comment)
                WITH comment, author, COUNT(reply) AS replyCount,
                    CASE WHEN COUNT(reply) > 0 THEN COLLECT(DISTINCT {
                        Id: replyAuthor.Id,
                        Username: replyAuthor.Username,
                        DisplayName: replyAuthor.DisplayName,
                        AvatarUrl: replyAuthor.AvatarUrl
                    })[0..3] ELSE [] END AS firstRepliesAuthors
                RETURN {
                    Id: comment.Id,
                    Content: comment.Content,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    },
                    CreatedAt: comment.CreatedAt,
                    ReplyCount: replyCount,
                    FirstRepliesAuthors: firstRepliesAuthors
                }
                """,
                new { id = id.ToString() });

            var record = await data.SingleAsync();
            return record[0].Adapt<CommentDto>();
        });
    }

    public async Task<Page<CommentDto>> SearchAsync(Guid? discussionId, Guid? authorId, int offset, int count)
    {
        var matchSelector = authorId is not null
            ? "MATCH (author:Author { Id: $authorId })<-[:AUTHORED_BY]-(comment:Comment)"
            : "MATCH (author:Author)<-[:AUTHORED_BY]-(comment:Comment)";

        if (discussionId is not null)
            matchSelector += "-[:REPLIES_TO]->(discussion:Discussion { Id: $discussionId })";

        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                matchSelector +
                """
                OPTIONAL MATCH (deepReplyAuthor:Author)<-[:AUTHORED_BY]-(deepReply:Comment)-[:REPLIES_TO*]->(comment)

                WITH author, comment, deepReplyAuthor, deepReply
                ORDER BY comment.CreatedAt, deepReply.CreatedAt DESC

                WITH author, comment, {
                    DeepReplyCount: COUNT(deepReply),
                    FirstDeepRepliesAuthors: CASE WHEN COUNT(deepReply) > 0 THEN COLLECT(DISTINCT {
                        Id: deepReplyAuthor.Id,
                        Username: deepReplyAuthor.Username,
                        DisplayName: deepReplyAuthor.DisplayName,
                        AvatarUrl: deepReplyAuthor.AvatarUrl
                    })[0..3] ELSE [] END
                } AS deepRepliesData

                RETURN {
                    TotalCount: COUNT(comment),
                    Items: COLLECT({
                        Id: comment.Id,
                        Content: comment.Content,
                        Author: {
                            Id: author.Id,
                            Username: author.Username,
                            DisplayName: author.DisplayName,
                            AvatarUrl: author.AvatarUrl
                        },
                        CreatedAt: comment.CreatedAt,
                        ReplyCount: deepRepliesData.DeepReplyCount,
                        FirstRepliesAuthors: deepRepliesData.FirstDeepRepliesAuthors
                    })[$offset..$offset + $count]
                }
                """,
                new
                {
                    discussionId = discussionId.ToString(),
                    authorId = authorId.ToString(),
                    offset,
                    count
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<Page<CommentDto>>();
        });
    }

    public async Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (author:Author { Id: $authorId })
                MATCH (discussion:Discussion { Id: $discussionId })
                CREATE (comment:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                CREATE (discussion)<-[:REPLIES_TO]-(comment)-[:AUTHORED_BY]->(author)
                RETURN {
                    Id: comment.Id,
                    Content: comment.Content,
                    Author: {
                        Id: author.Id,
                        Username: author.Username,
                        DisplayName: author.DisplayName,
                        AvatarUrl: author.AvatarUrl
                    },
                    CreatedAt: datetime(),
                    ReplyCount: 0,
                    FirstRepliesAuthors: []
                }
                """,
                new
                {
                    authorId = authorId.ToString(),
                    discussionId = discussionId.ToString(),
                    content
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<CommentDto>();
        });
    }

    public async Task<Page<CommentDto>> GetRepliesToCommentAsync(Guid parentCommentId, int offset, int count)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (author:Author)<-[:AUTHORED_BY]-(comment:Comment)-[:REPLIES_TO]->(parent:Comment { Id: $parentCommentId })
                WITH author, comment, COUNT(comment) AS totalCount
                ORDER BY comment.CreatedAt
                SKIP $offset
                LIMIT $count
                OPTIONAL MATCH (replyAuthor:Author)<-[:AUTHORED_BY]-(reply:Comment)-[:REPLIES_TO]->(comment)
                WITH totalCount, comment, author, COUNT(reply) AS replyCount,
                    CASE WHEN COUNT(reply) > 0 THEN COLLECT(DISTINCT {
                        Id: replyAuthor.Id,
                        Username: replyAuthor.Username,
                        DisplayName: replyAuthor.DisplayName,
                        AvatarUrl: replyAuthor.AvatarUrl
                    })[0..3] ELSE [] END AS firstRepliesAuthors
                RETURN {
                    TotalCount = totalCount,
                    Items = {
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
                }
                """,
                new
                {
                    parentCommentId = parentCommentId.ToString(),
                    offset,
                    count
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<Page<CommentDto>>();
        });
    }

    public async Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (replyAuthor:Author { Id: $authorId })
                MATCH (parent:Comment {Id: $parentCommentId})
                CREATE (reply:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                CREATE (parent)<-[:REPLIES_TO]-(reply)-[:AUTHORED_BY]->(replyAuthor)
                RETURN {
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
                """,
                new
                {
                    parentCommentId = parentCommentId.ToString(),
                    authorId = authorId.ToString(),
                    content
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<CommentDto>();
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var session = _driver.AsyncSession();
        var notFount = await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                OPTIONAL MATCH (comment:Comment { Id: $id })
                DETACH DELETE comment
                RETURN COUNT(comment) = 0
                """,
                new { id = id.ToString() });

            var record = await data.SingleAsync();
            return record[0].As<bool>();
        });

        if (notFount)
            throw new NotFoundException();
    }
}