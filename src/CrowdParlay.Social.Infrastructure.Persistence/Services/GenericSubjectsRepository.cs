using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Infrastructure.Persistence.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CrowdParlay.Social.Infrastructure.Persistence.Services;

public class GenericSubjectsRepository<TDocument>(IClientSessionHandle session, IMongoDatabase database, string collection)
    : ISubjectsRepository where TDocument : ISubjectDocument
{
    private readonly IMongoCollection<TDocument> _subjects = database.GetCollection<TDocument>(collection);

    public async Task<ISet<string>> GetReactionsAsync(string subjectId, Guid authorId)
    {
        var pipeline = _subjects
            .Find(session, subject => subject.Id == ObjectId.Parse(subjectId))
            .Project(subject => subject.ReactionsByAuthorId.ContainsKey(authorId.ToString())
                    ? subject.ReactionsByAuthorId[authorId.ToString()]
                    : new string[] { });

        var reactions = await pipeline.FirstOrDefaultAsync() ?? throw new NotFoundException();
        return reactions.ToHashSet();
    }

    public async Task SetReactionsAsync(string subjectId, Guid authorId, ISet<string> reactions)
    {
        var filter = Builders<TDocument>.Filter.Eq(
            subject => subject.Id,
            ObjectId.Parse(subjectId));

        var update = Builders<TDocument>.Update.Set<IEnumerable<string>>(
            subject => subject.ReactionsByAuthorId[authorId.ToString()],
            reactions);

        var result = await _subjects.UpdateOneAsync(session, filter, update);
        if (result.MatchedCount == 0)
            throw new NotFoundException();
    }

    public async Task UpdateReactionCountersAsync(string subjectId, IEnumerable<string> reactionsToAdd, IEnumerable<string> reactionsToRemove)
    {
        var increments = reactionsToAdd.Select(reaction =>
            Builders<TDocument>.Update.Inc(subject => subject.ReactionCounters[reaction], 1));

        var decrements = reactionsToRemove.Select(reaction =>
            Builders<TDocument>.Update.Inc(subject => subject.ReactionCounters[reaction], -1));

        var filter = Builders<TDocument>.Filter.Eq(subject => subject.Id, ObjectId.Parse(subjectId));
        var update = Builders<TDocument>.Update.Combine(increments.Union(decrements));
        var result = await _subjects.UpdateOneAsync(session, filter, update);

        if (result.MatchedCount == 0)
            throw new NotFoundException();
    }

    public async Task IncludeCommentInMetadataAsync(string subjectId, Guid authorId)
    {
        var oldLastCommentsAuthorIds =
            await _subjects
                .Find(session, subject => subject.Id == ObjectId.Parse(subjectId))
                .Project(subject => subject.LastCommentsAuthorIds)
                .FirstOrDefaultAsync()
            ?? throw new NotFoundException();

        var newLastCommentAuthorIds = oldLastCommentsAuthorIds
            .Except([authorId])
            .Append(authorId)
            .TakeLast(3)
            .ToList();

        var filter = Builders<TDocument>.Filter.Eq(subject => subject.Id, ObjectId.Parse(subjectId));
        var update = Builders<TDocument>.Update
            .Set(subject => subject.LastCommentsAuthorIds, newLastCommentAuthorIds)
            .Inc(subject => subject.CommentCount, 1);

        await _subjects.UpdateOneAsync(session, filter, update);
    }
    
    public async Task ExcludeCommentFromMetadataAsync(string subjectId)
    {
        var filter = Builders<TDocument>.Filter.Eq(subject => subject.Id, ObjectId.Parse(subjectId));
        var update = Builders<TDocument>.Update.Inc(subject => subject.CommentCount, -1);
        await _subjects.UpdateOneAsync(session, filter, update);
    }
}