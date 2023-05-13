namespace CrowdParlay.Social.Application.DTOs.Author;

public record AuthorDto
{
    public Guid Id { get; set; }
    public string Alias { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
}