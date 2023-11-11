namespace CrowdParlay.Social.Application.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public AuthorDto Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReplyCount { get; set; }
    public IEnumerable<AuthorDto> FirstRepliesAuthors { get; set; } = Enumerable.Empty<AuthorDto>();
}