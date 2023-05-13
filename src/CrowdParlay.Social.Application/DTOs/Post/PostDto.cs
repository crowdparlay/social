using CrowdParlay.Social.Application.DTOs.Author;

namespace CrowdParlay.Social.Application.DTOs.Post;

public class PostDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public AuthorDto AuthorDto { get; set; } = default!;
}