namespace CrowdParlay.Social.Application.DTOs.Post;

public record CreatePostDto
{
    public string Content { get; set; } = default!;
    public Guid AuthorId { get; set; }
}