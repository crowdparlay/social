namespace CrowdParlay.Social.Application.DTOs;

public class CommentDto
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
    public required AuthorDto Author { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public int ReplyCount { get; set; }
    public IEnumerable<AuthorDto> FirstRepliesAuthors { get; set; } = Enumerable.Empty<AuthorDto>();
}