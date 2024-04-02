namespace CrowdParlay.Social.Domain.Entities;

public class Discussion
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required Guid AuthorId { get; set; }
}