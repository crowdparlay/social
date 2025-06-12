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
using static MongoDB.Driver.Builders<CrowdParlay.Social.Infrastructure.Persistence.Models.CommentDocument>;
using static MongoDB.Driver.PipelineDefinition<CrowdParlay.Social.Infrastructure.Persistence.Models.CommentDocument,
    MongoDB.Driver.AggregateCountResult>;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class CommentsRepository(IClientSessionHandle session, IMongoDatabase database) : ICommentsRepository
{
    private readonly IMongoCollection<CommentDocument> _comments = database.GetCollection<CommentDocument>(Collections.Comments);
    private readonly ISubjectsRepository _subjectsRepository = new GenericSubjectsRepository<CommentDocument>(session, database, Collections.Comments);

    public async Task<Comment> GetByIdAsync(string commentId, Guid? viewerId)
    {
        var pipeline = _comments
            .Find(session, comment => comment.Id == ObjectId.Parse(commentId))
            .Project(CreateCommentProjectionExpression(viewerId));

        return await pipeline.FirstOrDefaultAsync() ?? throw new NotFoundException();
    }

    public async Task<Page<Comment>> GetRepliesAsync(string subjectId, bool flatten, Guid? viewerId, int offset, int count)
    {
        var matchPipeline = _comments.Aggregate(session);

        if (flatten)
        {
            matchPipeline = matchPipeline
                .Limit(1)
                .GraphLookup<CommentDocument, CommentDocument, ObjectId, ObjectId, ObjectId, CommentDocument, IList<CommentDocument>, CommentDocument>(
                    from: _comments,
                    startWith: _ => ObjectId.Parse(subjectId),
                    connectFromField: comment => comment.Id,
                    connectToField: comment => comment.SubjectId,
                    @as: comment => comment.Ascendants!,
                    depthField: comment => comment.Depth!.Value)
                .Unwind<CommentDocument, CommentDocument>(comment => comment.Ascendants)
                .ReplaceRoot<CommentDocument>("$ascendants");
        }
        else
        {
            matchPipeline = matchPipeline
                .Match(comment => comment.SubjectId == ObjectId.Parse(subjectId));
        }

        var pipeline = matchPipeline.Facet(
            new AggregateFacet<CommentDocument, Comment>(
                "items",
                PipelineDefinition<CommentDocument, Comment>.Create(
                [
                    Sort(Builders<CommentDocument>.Sort.Ascending(comment => comment.Id)),
                    Skip<CommentDocument>(offset),
                    Limit<CommentDocument>(count),
                    Project(CreateCommentProjectionExpression(viewerId))
                ])
            ),
            new AggregateFacet<CommentDocument, AggregateCountResult>(
                "totalCount",
                Create([Count<CommentDocument>()])
            )
        );

        var result = await pipeline.FirstOrDefaultAsync() ?? throw new NotFoundException();
        return new Page<Comment>
        {
            Items = result.Facets[0].Output<Comment>() ?? Enumerable.Empty<Comment>(),
            TotalCount = (int)(result.Facets[1].Output<AggregateCountResult>().FirstOrDefault()?.Count ?? 0)
        };
    }

    public async Task<string> CreateAsync(string? subjectId, Guid authorId, string content)
    {
        var reply = new CommentDocument
        {
            Id = ObjectId.GenerateNewId(),
            CreatedAt = DateTimeOffset.UtcNow,
            AuthorId = authorId,
            SubjectId = ObjectId.Parse(subjectId),
            Content = content,
            CommentCount = 0,
            LastCommentsAuthorIds = [],
            ReactionCounters = new Dictionary<string, int>(),
            ReactionsByAuthorId = new Dictionary<string, string[]>()
        };

        await _comments.InsertOneAsync(session, reply);
        return reply.Id.ToString();
    }

    public async Task<IList<Comment>> GetAncestorsAsync(string commentId, Guid? viewerId) => await _comments.Aggregate(session)
        .Match(comment => comment.Id == ObjectId.Parse(commentId))
        .GraphLookup<CommentDocument, CommentDocument, ObjectId, ObjectId, ObjectId, CommentDocument, IList<CommentDocument>, CommentDocument>(
            from: _comments,
            startWith: comment => comment.SubjectId,
            connectFromField: comment => comment.SubjectId,
            connectToField: comment => comment.Id,
            @as: comment => comment.Ancestors!,
            depthField: comment => comment.Depth!.Value)
        .Unwind<CommentDocument, CommentDocument>(comment => comment.Ancestors)
        .ReplaceRoot<CommentDocument>("$ancestors")
        .SortBy(comment => comment.Depth)
        .Project(CreateCommentProjectionExpression(viewerId))
        .ToListAsync();

    public async Task IncludeCommentInAncestorsMetadataAsync(IEnumerable<Comment> ancestors, Guid authorId)
    {
        var updates = ancestors.Select(ancestor =>
            {
                var newLastCommentsAuthorIds = ancestor.LastCommentsAuthorIds
                    .Except([authorId])
                    .Append(authorId)
                    .TakeLast(3)
                    .ToList();

                var filter = Filter.Eq(comment => comment.Id, ObjectId.Parse(ancestor.Id));
                var update = Update
                    .Set(comment => comment.LastCommentsAuthorIds, newLastCommentsAuthorIds)
                    .Inc(comment => comment.CommentCount, 1);

                return new UpdateOneModel<CommentDocument>(filter, update);
            })
            .ToList();

        if (updates.Any())
            await _comments.BulkWriteAsync(session, updates);
    }

    public async Task ExcludeCommentFromAncestorsMetadataAsync(IEnumerable<Comment> ancestors)
    {
        var filter = Filter.In(comment => comment.Id, ancestors.Select(comment => ObjectId.Parse(comment.Id)));
        var update = Update.Inc(comment => comment.CommentCount, -1);
        await _comments.UpdateManyAsync(session, filter, update);
    }

    public async Task IncludeCommentInMetadataAsync(string discussionId, Guid authorId) =>
        await _subjectsRepository.IncludeCommentInMetadataAsync(discussionId, authorId);

    public async Task ExcludeCommentFromMetadataAsync(string discussionId) =>
        await _subjectsRepository.ExcludeCommentFromMetadataAsync(discussionId);

    public async Task DeleteAsync(string commentId)
    {
        var deleteResult = await _comments.DeleteOneAsync(session, comment => comment.Id == ObjectId.Parse(commentId));
        if (deleteResult.DeletedCount == 0)
            throw new NotFoundException();
    }

    public async Task<ISet<string>> GetReactionsAsync(string commentId, Guid authorId) =>
        await _subjectsRepository.GetReactionsAsync(commentId, authorId);

    public async Task SetReactionsAsync(string commentId, Guid authorId, ISet<string> reactions) =>
        await _subjectsRepository.SetReactionsAsync(commentId, authorId, reactions);

    public async Task UpdateReactionCountersAsync(string commentId, IDictionary<string, int> reactionsDiff) =>
        await _subjectsRepository.UpdateReactionCountersAsync(commentId, reactionsDiff);

    private static Expression<Func<CommentDocument, Comment>> CreateCommentProjectionExpression(Guid? viewerId) => comment => new Comment
    {
        Id = comment.Id.ToString(),
        SubjectId = comment.SubjectId.ToString(),
        CreatedAt = comment.CreatedAt,
        AuthorId = comment.AuthorId,
        Content = comment.Content,
        CommentCount = comment.CommentCount,
        LastCommentsAuthorIds = comment.LastCommentsAuthorIds,
        ReactionCounters = comment.ReactionCounters,
        ViewerReactions =
            viewerId.HasValue &&
            comment.ReactionsByAuthorId.ContainsKey(viewerId.Value.ToString())
                ? comment.ReactionsByAuthorId[viewerId.Value.ToString()]
                : Array.Empty<string>()
    };
}