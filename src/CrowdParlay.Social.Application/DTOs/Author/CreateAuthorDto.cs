namespace CrowdParlay.Social.Application.DTOs.Author;

public record CreateAuthorDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
    public string? Alias { get; set; }
}