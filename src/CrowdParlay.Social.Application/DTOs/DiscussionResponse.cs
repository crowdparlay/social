namespace CrowdParlay.Social.Application.DTOs;

public class DiscussionResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required AuthorResponse? Author { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required int CommentCount { get; set; }
    public required IEnumerable<AuthorResponse> LastCommentsAuthors { get; set; }
    public required IDictionary<string, int> ReactionCounters { get; set; }
    public required ISet<string> ViewerReactions { get; set; }
}