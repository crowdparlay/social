using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class CommentsRepository(IDriver driver) : ICommentRepository
{
    public async Task<Comment> GetByIdAsync(Guid id)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteReadAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (comment:Comment { Id: $id })-[:AUTHORED_BY]->(author:Author)
                OPTIONAL MATCH (replyAuthor:Author)<-[:AUTHORED_BY]-(reply:Comment)-[:REPLIES_TO]->(comment)

                WITH comment, author, COUNT(reply) AS replyCount,
                    CASE WHEN COUNT(reply) > 0
                        THEN COLLECT(DISTINCT replyAuthor.Id)[0..3]
                        ELSE [] END AS firstRepliesAuthorIds
                    
                RETURN {
                    Id: comment.Id,
                    Content: comment.Content,
                    AuthorId: author.Id,
                    CreatedAt: comment.CreatedAt,
                    ReplyCount: replyCount,
                    FirstRepliesAuthorIds: firstRepliesAuthorIds
                }
                """,
                new { id = id.ToString() });

            if (await data.PeekAsync() is null)
                throw new NotFoundException();

            var record = await data.SingleAsync();
            return record[0].Adapt<Comment>();
        });
    }

    public async Task<Page<Comment>> SearchAsync(Guid? discussionId, Guid? authorId, int offset, int count)
    {
        var matchSelector = authorId is not null
            ? "MATCH (author:Author { Id: $authorId })<-[:AUTHORED_BY]-(comment:Comment)"
            : "MATCH (author:Author)<-[:AUTHORED_BY]-(comment:Comment)";

        if (discussionId is not null)
            matchSelector += "-[:REPLIES_TO]->(discussion:Discussion { Id: $discussionId })";

        await using var session = driver.AsyncSession();
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
                    FirstDeepRepliesAuthorIds: CASE WHEN COUNT(deepReply) > 0
                        THEN COLLECT(DISTINCT deepReplyAuthor.Id)[0..3]
                        ELSE [] END
                } AS deepRepliesData

                RETURN {
                    TotalCount: COUNT(comment),
                    Items: COLLECT({
                        Id: comment.Id,
                        Content: comment.Content,
                        AuthorId: author.Id,
                        CreatedAt: comment.CreatedAt,
                        ReplyCount: deepRepliesData.DeepReplyCount,
                        FirstRepliesAuthorIds: deepRepliesData.FirstDeepRepliesAuthorIds
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

            if (await data.PeekAsync() is null)
            {
                return new Page<Comment>
                {
                    TotalCount = 0,
                    Items = Enumerable.Empty<Comment>()
                };
            }

            var record = await data.SingleAsync();
            return record[0].Adapt<Page<Comment>>();
        });
    }

    public async Task<Comment> CreateAsync(Guid authorId, Guid discussionId, string content)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (discussion:Discussion { Id: $discussionId })
                MERGE (author:Author { Id: $authorId })

                CREATE (comment:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                CREATE (discussion)<-[:REPLIES_TO]-(comment)-[:AUTHORED_BY]->(author)

                RETURN {
                    Id: comment.Id,
                    Content: comment.Content,
                    AuthorId: author.Id,
                    CreatedAt: datetime(),
                    ReplyCount: 0,
                    FirstRepliesAuthorIds: []
                }
                """,
                new
                {
                    authorId = authorId.ToString(),
                    discussionId = discussionId.ToString(),
                    content
                });

            if (await data.PeekAsync() is null)
                throw new NotFoundException();

            var record = await data.SingleAsync();
            return record[0].Adapt<Comment>();
        });
    }

    public async Task<Page<Comment>> GetRepliesToCommentAsync(Guid parentCommentId, int offset, int count)
    {
        await using var session = driver.AsyncSession();
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

                WITH totalCount, author, comment, COUNT(reply) AS replyCount,
                    CASE WHEN COUNT(reply) > 0
                        THEN COLLECT(DISTINCT replyAuthor.Id)[0..3]
                        ELSE [] END AS firstRepliesAuthorIds
                    
                WITH totalCount,
                    COLLECT({
                        Id: comment.Id,
                        Content: comment.Content,
                        AuthorId: author.Id,
                        CreatedAt: datetime(comment.CreatedAt),
                        ReplyCount: replyCount,
                        FirstRepliesAuthorIds: firstRepliesAuthorIds
                    }) AS comments
                    
                RETURN {
                    TotalCount: totalCount,
                    Items: comments
                }
                """,
                new
                {
                    parentCommentId = parentCommentId.ToString(),
                    offset,
                    count
                });

            if (await data.PeekAsync() is null)
            {
                return new Page<Comment>
                {
                    TotalCount = 0,
                    Items = Enumerable.Empty<Comment>()
                };
            }

            var record = await data.SingleAsync();
            return record[0].Adapt<Page<Comment>>();
        });
    }

    public async Task<Comment> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        await using var session = driver.AsyncSession();
        return await session.ExecuteWriteAsync(async runner =>
        {
            var data = await runner.RunAsync(
                """
                MATCH (parent:Comment {Id: $parentCommentId})
                MERGE (replyAuthor:Author { Id: $authorId })

                CREATE (reply:Comment {
                    Id: randomUUID(),
                    Content: $content,
                    CreatedAt: datetime()
                })
                CREATE (parent)<-[:REPLIES_TO]-(reply)-[:AUTHORED_BY]->(replyAuthor)

                RETURN {
                    Id: reply.Id,
                    Content: reply.Content,
                    AuthorId: replyAuthor.Id,
                    CreatedAt: reply.CreatedAt,
                    ReplyCount: 0,
                    FirstRepliesAuthorIds: []
                }
                """,
                new
                {
                    parentCommentId = parentCommentId.ToString(),
                    authorId = authorId.ToString(),
                    content
                });

            var record = await data.SingleAsync();
            return record[0].Adapt<Comment>();
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var session = driver.AsyncSession();
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