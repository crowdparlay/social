namespace CrowdParlay.Social.Application.DTOs;

public class AuthorDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}