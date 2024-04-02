namespace CrowdParlay.Social.Application.DTOs;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public required string? AvatarUrl { get; set; }
}