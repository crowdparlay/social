namespace CrowdParlay.Social.Domain.Entities;

/// <summary>
/// A thread that includes posts in relationship "one-to-many".
/// </summary>
public class Discussion
{
    public int Id { get; set; }
    public List<Post> Posts { get; set; } = new();
    public string Title { get; set; } = default!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}