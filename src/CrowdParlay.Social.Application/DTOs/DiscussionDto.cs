namespace CrowdParlay.Social.Application.DTOs;

public class DiscussionDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required AuthorDto? Author { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required IDictionary<string, int> ReactionCounters { get; set; }
    public required ISet<string> ViewerReactions { get; set; }
}