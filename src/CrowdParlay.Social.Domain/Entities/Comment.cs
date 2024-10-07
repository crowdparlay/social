using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Domain.Entities;

public class Comment
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
    public required Guid AuthorId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required int ReplyCount { get; set; }
    public required ISet<Guid> FirstRepliesAuthorIds { get; set; }
    public required ISet<ReactionCounter> Reactions { get; set; }
}