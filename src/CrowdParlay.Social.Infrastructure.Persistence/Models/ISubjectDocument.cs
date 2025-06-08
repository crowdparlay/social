using MongoDB.Bson;

namespace CrowdParlay.Social.Infrastructure.Persistence.Models;

public interface ISubjectDocument
{
    public ObjectId Id { get; set; }
    public IDictionary<string, int> ReactionCounters { get; set; }
    public IDictionary<string, string[]> ReactionsByAuthorId { get; set; }
    public int CommentCount { get; set; }
    public IList<Guid> LastCommentsAuthorIds { get; set; }
}