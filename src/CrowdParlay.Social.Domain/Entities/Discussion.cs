using CrowdParlay.Social.Domain.DTOs;

namespace CrowdParlay.Social.Domain.Entities;

public class Discussion
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Guid AuthorId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required ISet<ReactionCounter> Reactions { get; set; }
}