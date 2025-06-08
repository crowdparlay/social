using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CrowdParlay.Social.Infrastructure.Persistence.Models;

public class CommentDocument : ISubjectDocument
{
    [BsonId] public required ObjectId Id { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required Guid AuthorId { get; set; }
    public required ObjectId SubjectId { get; set; }
    public required string Content { get; set; }
    public required int CommentCount { get; set; }
    public required IList<Guid> LastCommentsAuthorIds { get; set; }
    public required IDictionary<string, int> ReactionCounters { get; set; }
    public required IDictionary<string, string[]> ReactionsByAuthorId { get; set; }
    [BsonIgnoreIfNull] public IList<CommentDocument>? Ancestors { get; set; }
    [BsonIgnoreIfNull] public IList<CommentDocument>? Ascendants { get; set; }
}