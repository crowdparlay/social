namespace CrowdParlay.Social.Application.DTOs;

public class CommentResponse
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
    public required AuthorResponse? Author { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required int ReplyCount { get; set; }
    public required IEnumerable<AuthorResponse> FirstRepliesAuthors { get; set; }
    public required IDictionary<string, int> ReactionCounters { get; set; }
    public required ISet<string> ViewerReactions { get; set; }
}