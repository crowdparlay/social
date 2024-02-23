namespace CrowdParlay.Social.Application.DTOs;

public class DiscussionDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required AuthorDto Author { get; set; }
}