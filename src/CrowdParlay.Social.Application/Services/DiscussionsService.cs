using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.Entities;
using Mapster;

namespace CrowdParlay.Social.Application.Services;

public class DiscussionsService(IDiscussionsRepository discussionsRepository, IUsersService usersService) : IDiscussionsService
{
    public async Task<DiscussionDto> GetByIdAsync(Guid id)
    {
        var discussion = await discussionsRepository.GetByIdAsync(id);
        return await EnrichAsync(discussion);
    }

    public async Task<IEnumerable<DiscussionDto>> GetAllAsync()
    {
        var discussions = await discussionsRepository.GetAllAsync();
        return await EnrichAsync(discussions.ToArray());
    }

    public async Task<IEnumerable<DiscussionDto>> GetByAuthorAsync(Guid authorId)
    {
        var discussions = await discussionsRepository.GetByAuthorAsync(authorId);
        return await EnrichAsync(discussions.ToArray());
    }

    public async Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description)
    {
        var discussion = await discussionsRepository.CreateAsync(authorId, title, description);
        return await EnrichAsync(discussion);
    }

    private async Task<DiscussionDto> EnrichAsync(Discussion discussion)
    {
        var author = await usersService.GetByIdAsync(discussion.AuthorId);
        return new DiscussionDto
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Description = discussion.Description,
            Author = author.Adapt<AuthorDto>()
        };
    }

    private async Task<IEnumerable<DiscussionDto>> EnrichAsync(IReadOnlyList<Discussion> discussions)
    {
        var authorIds = discussions.Select(discussion => discussion.AuthorId);
        var authors = await usersService.GetUsersAsync(authorIds).ToArrayAsync();
        var authorsById = authors.ToDictionary(user => user.Id, user => user.Adapt<AuthorDto>());

        return discussions.Select(x => new DiscussionDto
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            Author = authorsById[x.AuthorId]
        });
    }
}