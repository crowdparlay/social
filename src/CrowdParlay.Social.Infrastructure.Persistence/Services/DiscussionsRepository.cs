using System.Linq.Expressions;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using CrowdParlay.Social.Infrastructure.Persistence.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using static CrowdParlay.Social.Infrastructure.Persistence.MongoDbConstants;
using static MongoDB.Driver.PipelineStageDefinitionBuilder;
using static MongoDB.Driver.Builders<CrowdParlay.Social.Infrastructure.Persistence.Models.DiscussionDocument>;
using static MongoDB.Driver.PipelineDefinition<CrowdParlay.Social.Infrastructure.Persistence.Models.DiscussionDocument,
    MongoDB.Driver.AggregateCountResult>;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class DiscussionsRepository(IClientSessionHandle session, IMongoDatabase database) : IDiscussionsRepository
{
    private readonly IMongoCollection<DiscussionDocument> _discussions = database.GetCollection<DiscussionDocument>(Collections.Discussions);
    private readonly ISubjectsRepository _subjectsRepository = new GenericSubjectsRepository<DiscussionDocument>(session, database, Collections.Discussions);

    public async Task<Discussion> GetByIdAsync(string discussionId, Guid? viewerId)
    {
        var pipeline = _discussions
            .Find(session, discussion => discussion.Id == ObjectId.Parse(discussionId))
            .Project(CreateDiscussionProjectionExpression(viewerId));

        return await pipeline.FirstOrDefaultAsync() ?? throw new NotFoundException();
    }

    public async Task<Page<Discussion>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var pipeline = _discussions.Aggregate(session)
            .Match(authorId.HasValue
                ? Filter.Eq(discussion => discussion.AuthorId, authorId.Value)
                : FilterDefinition<DiscussionDocument>.Empty)
            .Facet(
                new AggregateFacet<DiscussionDocument, Discussion>(
                    "items",
                    PipelineDefinition<DiscussionDocument, Discussion>.Create(
                    [
                        Sort(Builders<DiscussionDocument>.Sort.Ascending(discussion => discussion.Id)),
                        Skip<DiscussionDocument>(offset),
                        Limit<DiscussionDocument>(count),
                        Project(CreateDiscussionProjectionExpression(viewerId))
                    ])
                ),
                new AggregateFacet<DiscussionDocument, AggregateCountResult>(
                    "totalCount",
                    Create([Count<DiscussionDocument>()])
                )
            );

        var result = await pipeline.FirstOrDefaultAsync() ?? throw new NotFoundException();
        return new Page<Discussion>
        {
            Items = result.Facets[0].Output<Discussion>() ?? Enumerable.Empty<Discussion>(),
            TotalCount = (int)(result.Facets[1].Output<AggregateCountResult>().FirstOrDefault()?.Count ?? 0)
        };
    }

    public async Task<string> CreateAsync(Guid authorId, string title, string content)
    {
        var discussion = new DiscussionDocument
        {
            Id = ObjectId.Empty,
            AuthorId = authorId,
            CreatedAt = DateTimeOffset.UtcNow,
            Title = title,
            Content = content,
            CommentCount = 0,
            LastCommentsAuthorIds = [],
            ReactionCounters = new Dictionary<string, int>(),
            ReactionsByAuthorId = new Dictionary<string, string[]>()
        };

        await _discussions.InsertOneAsync(session, discussion);
        return discussion.Id.ToString();
    }

    public async Task UpdateAsync(string discussionId, string? title, string? content)
    {
        var updates = new List<UpdateDefinition<DiscussionDocument>>(2);

        if (title is not null)
            updates.Add(Update.Set(discussion => discussion.Title, title));

        if (content is not null)
            updates.Add(Update.Set(discussion => discussion.Content, content));

        if (updates.Count == 0)
            return;

        var filter = Filter.Eq(discussion => discussion.Id, ObjectId.Parse(discussionId));
        var update = Update.Combine(updates);
        var result = await _discussions.UpdateOneAsync(session, filter, update);

        if (result.MatchedCount == 0)
            throw new NotFoundException();
    }

    public async Task<ISet<string>> GetReactionsAsync(string discussionId, Guid authorId) =>
        await _subjectsRepository.GetReactionsAsync(discussionId, authorId);

    public async Task SetReactionsAsync(string discussionId, Guid authorId, ISet<string> reactions) =>
        await _subjectsRepository.SetReactionsAsync(discussionId, authorId, reactions);

    public async Task UpdateReactionCountersAsync(string subjectId, IEnumerable<string> reactionsToAdd, IEnumerable<string> reactionsToRemove) =>
        await _subjectsRepository.UpdateReactionCountersAsync(subjectId, reactionsToAdd, reactionsToRemove);

    public async Task IncludeCommentInMetadataAsync(string discussionId, Guid authorId) =>
        await _subjectsRepository.IncludeCommentInMetadataAsync(discussionId, authorId);

    public async Task ExcludeCommentFromMetadataAsync(string discussionId) =>
        await _subjectsRepository.ExcludeCommentFromMetadataAsync(discussionId);

    private static Expression<Func<DiscussionDocument, Discussion>> CreateDiscussionProjectionExpression(Guid? viewerId) => discussion => new Discussion
    {
        Id = discussion.Id.ToString(),
        Title = discussion.Title,
        Content = discussion.Content,
        AuthorId = discussion.AuthorId,
        CreatedAt = discussion.CreatedAt,
        CommentCount = discussion.CommentCount,
        LastCommentsAuthorIds = discussion.LastCommentsAuthorIds,
        ReactionCounters = discussion.ReactionCounters,
        ViewerReactions =
            viewerId.HasValue &&
            discussion.ReactionsByAuthorId.ContainsKey(viewerId.Value.ToString())
                ? discussion.ReactionsByAuthorId[viewerId.Value.ToString()]
                : Array.Empty<string>()
    };
}