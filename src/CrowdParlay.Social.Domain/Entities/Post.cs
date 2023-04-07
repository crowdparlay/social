namespace CrowdParlay.Social.Domain.Entities;

/// <summary>
/// Post with text, timestamp, OP (author) and discussion id.
/// </summary>
public class Post
{
    public int Id { get; set; }
    public Discussion Discussion { get; set; } = default!;
    public string Content { get; set; } = default!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}