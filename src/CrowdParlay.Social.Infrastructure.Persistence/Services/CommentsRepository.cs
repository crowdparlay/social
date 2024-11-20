using System.Text;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class CommentsRepository(IAsyncQueryRunner runner) : ICommentsRepository
{
    public async Task<Comment> GetByIdAsync(Guid commentId, Guid? viewerId)
    {
        var data = await runner.RunAsync(
            """
            MATCH (comment:Comment { Id: $commentId })-[:AUTHORED_BY]->(author:Author)
            OPTIONAL MATCH (deepReplyAuthor:Author)<-[:AUTHORED_BY]-(deepReply:Comment)-[:REPLIES_TO*]->(comment)
            OPTIONAL MATCH (comment)<-[reaction:REACTED_TO]-(:Author)
            OPTIONAL MATCH (comment)<-[viewerReaction:REACTED_TO]-(:Author { Id: $viewerId })

            WITH author, comment, deepReply, deepReplyAuthor, reaction,
                 collect(viewerReaction.Value) AS viewerReactions

            WITH author, comment, deepReply, deepReplyAuthor, viewerReactions,
                 reaction.Value AS reactionValue, count(reaction) AS reactionCount

            WITH author, comment, deepReply, deepReplyAuthor, viewerReactions,
                 apoc.map.fromPairs(collect([reactionValue, reactionCount])) AS reactionCounters

            ORDER BY deepReply.CreatedAt DESC

            WITH author, comment, viewerReactions, reactionCounters,
                 count(deepReply) AS deepReplyCount,
                 collect(DISTINCT deepReplyAuthor.Id)[0..3] AS lastDeepRepliesAuthorIds

            RETURN {
                Id: comment.Id,
                Content: comment.Content,
                AuthorId: author.Id,
                CreatedAt: comment.CreatedAt,
                ReplyCount: deepReplyCount,
                LastRepliesAuthorIds: lastDeepRepliesAuthorIds,
                ReactionCounters: reactionCounters,
                ViewerReactions: viewerReactions
            }
            """,
            new
            {
                commentId = commentId.ToString(),
                viewerId = viewerId?.ToString()
            });

        if (await data.PeekAsync() is null)
            throw new NotFoundException();

        var record = await data.SingleAsync();
        return record[0].Adapt<Comment>();
    }

    public async Task<Page<Comment>> SearchAsync(Guid? subjectId, Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var matchSelector = new StringBuilder("MATCH (comment:Comment)-[:AUTHORED_BY]->(author:Author)");

        if (subjectId is not null)
        {
            matchSelector.AppendLine("MATCH (comment)-[:REPLIES_TO]->(subject { Id: $subjectId })");
            matchSelector.AppendLine("WHERE (subject:Comment OR subject:Discussion)");
        }

        if (authorId is not null)
            matchSelector.AppendLine("WHERE author.Id = $authorId");

        var data = await runner.RunAsync(
            matchSelector +
            """
            OPTIONAL MATCH (deepReplyAuthor:Author)<-[:AUTHORED_BY]-(deepReply:Comment)-[:REPLIES_TO*]->(comment)

            OPTIONAL MATCH (comment)<-[reaction:REACTED_TO]-(:Author)
            OPTIONAL MATCH (comment)<-[viewerReaction:REACTED_TO]-(:Author { Id: $viewerId })

            WITH author, comment, deepReplyAuthor, deepReply, reaction,
                 collect(viewerReaction.Value) AS viewerReactions

            WITH author, comment, deepReplyAuthor, deepReply, viewerReactions,
                 reaction.Value AS reactionValue, count(reaction) AS reactionCount

            WITH author, comment, deepReplyAuthor, deepReply, viewerReactions,
                 apoc.map.fromPairs(collect([reactionValue, reactionCount])) AS reactionCounters
                 
            ORDER BY comment.CreatedAt, deepReply.CreatedAt DESC

            WITH author, comment, viewerReactions, reactionCounters,
                 COUNT(deepReply) AS deepReplyCount,
                 COLLECT(DISTINCT deepReplyAuthor.Id)[0..3] AS lastDeepRepliesAuthorIds

            RETURN {
                TotalCount: COUNT(comment),
                Items: COLLECT({
                    Id: comment.Id,
                    Content: comment.Content,
                    AuthorId: author.Id,
                    CreatedAt: comment.CreatedAt,
                    ReplyCount: deepReplyCount,
                    LastRepliesAuthorIds: lastDeepRepliesAuthorIds,
                    ReactionCounters: reactionCounters,
                    ViewerReactions: viewerReactions
                })[$offset..$offset + $count]
            }
            """,
            new
            {
                subjectId = subjectId?.ToString(),
                authorId = authorId?.ToString(),
                viewerId = viewerId?.ToString(),
                offset,
                count
            });

        if (await data.PeekAsync() is null)
        {
            return new Page<Comment>
            {
                TotalCount = 0,
                Items = []
            };
        }

        var record = await data.SingleAsync();
        return record[0].Adapt<Page<Comment>>();
    }

    public async Task<Guid> CreateAsync(Guid authorId, Guid discussionId, string content)
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
            RETURN comment.Id
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
        return record[0].Adapt<Guid>();
    }

    public async Task<Guid> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        var cursor = await runner.RunAsync(
            """
            MATCH (parent:Comment {Id: $parentCommentId})
            MERGE (replyAuthor:Author { Id: $authorId })
            CREATE (reply:Comment {
                Id: randomUUID(),
                Content: $content,
                CreatedAt: datetime()
            })
            CREATE (parent)<-[:REPLIES_TO]-(reply)-[:AUTHORED_BY]->(replyAuthor)
            RETURN reply.Id
            """,
            new
            {
                parentCommentId = parentCommentId.ToString(),
                authorId = authorId.ToString(),
                content
            });

        if (await cursor.PeekAsync() is null)
            throw new NotFoundException();

        var record = await cursor.SingleAsync();
        return record[0].Adapt<Guid>();
    }

    public async Task DeleteAsync(Guid commentId)
    {
        var data = await runner.RunAsync(
            """
            OPTIONAL MATCH (comment:Comment { Id: $commentId })
            OPTIONAL MATCH (reply:Comment)-[:REPLIES_TO*]->(comment)
            DETACH DELETE comment, reply
            RETURN COUNT(comment) = 0
            """,
            new { commentId = commentId.ToString() });

        var record = await data.SingleAsync();
        var notFount = record[0].As<bool>();

        if (notFount)
            throw new NotFoundException();
    }
}