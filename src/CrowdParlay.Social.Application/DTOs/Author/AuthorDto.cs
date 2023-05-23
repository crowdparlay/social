using System.ComponentModel.DataAnnotations;

namespace CrowdParlay.Social.Application.DTOs.Author;

public class AuthorDto
{
    public Guid Id { get; set; }
    public string Alias { get; set; } = default!;
    public required string DisplayName { get; set; } = default!;
    public required string AvatarUrl { get; set; } = default!;
}