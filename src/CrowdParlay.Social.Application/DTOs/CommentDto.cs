namespace CrowdParlay.Social.Application.DTOs;

public class CommentDto
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
    public required AuthorDto? Author { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required int ReplyCount { get; set; }
    public required IEnumerable<AuthorDto> FirstRepliesAuthors { get; set; }
}