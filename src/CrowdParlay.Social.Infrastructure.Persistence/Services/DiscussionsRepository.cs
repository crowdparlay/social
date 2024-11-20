using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;
using Neo4j.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DiscussionsRepository(IAsyncQueryRunner runner) : IDiscussionsRepository
{
    public async Task<Discussion> GetByIdAsync(Guid discussionId, Guid? viewerId)
    {
        var data = await runner.RunAsync(
            """
            MATCH (discussion:Discussion { Id: $discussionId })-[:AUTHORED_BY]->(author:Author)

            OPTIONAL MATCH (deepReplyAuthor:Author)<-[:AUTHORED_BY]-(deepReply:Comment)-[:REPLIES_TO*]->(discussion)
            OPTIONAL MATCH (discussion)<-[reaction:REACTED_TO]-(:Author)
            OPTIONAL MATCH (discussion)<-[viewerReaction:REACTED_TO]-(:Author { Id: $viewerId })

            WITH author, discussion, deepReply, deepReplyAuthor, reaction,
                 collect(viewerReaction.Value) AS viewerReactions
                 
            WITH author, discussion, deepReply, deepReplyAuthor, viewerReactions,
                 reaction.Value AS reactionValue, count(reaction) AS reactionCount
                 
            WITH author, discussion, deepReply, deepReplyAuthor, viewerReactions,
                 apoc.map.fromPairs(collect([reactionValue, reactionCount])) AS reactionCounters
                 
            ORDER BY deepReply.CreatedAt DESC

            WITH author, discussion, viewerReactions, reactionCounters,
                 count(deepReply) AS deepReplyCount,
                 collect(DISTINCT deepReplyAuthor.Id)[0..3] AS lastDeepRepliesAuthorIds
                 
            RETURN {
                Id: discussion.Id,
                Title: discussion.Title,
                Description: discussion.Description,
                AuthorId: author.Id,
                CreatedAt: discussion.CreatedAt,
                CommentCount: deepReplyCount,
                LastCommentsAuthorIds: lastDeepRepliesAuthorIds,
                ReactionCounters: reactionCounters,
                ViewerReactions: viewerReactions
            }
            """,
            new
            {
                discussionId = discussionId.ToString(),
                viewerId = viewerId?.ToString()
            });

        if (await data.PeekAsync() is null)
            throw new NotFoundException();

        var record = await data.SingleAsync();
        return record[0].Adapt<Discussion>();
    }

    public async Task<Page<Discussion>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var matchSelector = authorId is not null
            ? "MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author { Id: $authorId })"
            : "MATCH (discussion:Discussion)-[:AUTHORED_BY]->(author:Author)";

        var data = await runner.RunAsync(
            matchSelector +
            """
            OPTIONAL MATCH (deepReplyAuthor:Author)<-[:AUTHORED_BY]-(deepReply:Comment)-[:REPLIES_TO*]->(discussion)

            OPTIONAL MATCH (discussion)<-[reaction:REACTED_TO]-(:Author)
            OPTIONAL MATCH (discussion)<-[viewerReaction:REACTED_TO]-(:Author { Id: $authorId })

            WITH author, discussion, deepReplyAuthor, deepReply, reaction,
                 collect(viewerReaction.Value) AS viewerReactions

            WITH author, discussion, deepReplyAuthor, deepReply, viewerReactions,
                 reaction.Value AS reactionValue, count(reaction) AS reactionCount

            WITH author, discussion, deepReplyAuthor, deepReply, viewerReactions,
                 apoc.map.fromPairs(collect([reactionValue, reactionCount])) AS reactionCounters

            ORDER BY discussion.CreatedAt, deepReply.CreatedAt DESC

            WITH author, discussion, viewerReactions, reactionCounters,
                 COUNT(deepReply) AS deepReplyCount,
                 COLLECT(DISTINCT deepReplyAuthor.Id)[0..3] AS lastDeepRepliesAuthorIds

            RETURN {
                TotalCount: COUNT(discussion),
                Items: COLLECT({
                    Id: discussion.Id,
                    Title: discussion.Title,
                    Description: discussion.Description,
                    AuthorId: author.Id,
                    CreatedAt: discussion.CreatedAt,
                    CommentCount: deepReplyCount,
                    LastCommentsAuthorIds: lastDeepRepliesAuthorIds,
                    ReactionCounters: reactionCounters,
                    ViewerReactions: viewerReactions
                })[$offset..$offset + $count]
            }
            """,
            new
            {
                authorId = authorId?.ToString(),
                viewerId = viewerId?.ToString(),
                offset,
                count
            });

        if (await data.PeekAsync() is null)
        {
            return new Page<Discussion>
            {
                TotalCount = 0,
                Items = []
            };
        }

        var record = await data.SingleAsync();
        return record[0].Adapt<Page<Discussion>>();
    }

    public async Task<Guid> CreateAsync(Guid authorId, string title, string description)
    {
        var data = await runner.RunAsync(
            """
            MERGE (author:Author { Id: $authorId })
            CREATE (discussion:Discussion {
                Id: randomUUID(),
                Title: $title,
                Description: $description,
                CreatedAt: datetime()
            })
            CREATE (discussion)-[:AUTHORED_BY]->(author)
            RETURN discussion.Id
            """,
            new
            {
                authorId = authorId.ToString(),
                title,
                description
            });

        var record = await data.SingleAsync();
        return record[0].Adapt<Guid>();
    }

    public async Task UpdateAsync(Guid discussionId, string? title, string? description)
    {
        var data = await runner.RunAsync(
            """
            MATCH (d:Discussion { Id: $discussionId })
            SET d.Title: COALESCE($title, d.Title)
            SET d.Description: COALESCE($description, d.Description)
            RETURN COUNT(*)
            """,
            new
            {
                discussionId = discussionId.ToString(),
                title,
                description
            });

        var record = await data.SingleAsync();
        var notFound = record[0].As<int>() == 0;

        if (notFound)
            throw new NotFoundException();
    }
}