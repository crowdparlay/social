namespace CrowdParlay.Social.Domain.Entities;

public class Discussion
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required Guid AuthorId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required int CommentCount { get; set; }
    public required IList<Guid> LastCommentsAuthorIds { get; set; }
    public required IDictionary<string, int> ReactionCounters { get; set; }
    public required IList<string> ViewerReactions { get; set; }
}