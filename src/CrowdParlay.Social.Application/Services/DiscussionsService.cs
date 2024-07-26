using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
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

    public async Task<Page<DiscussionDto>> GetAllAsync(int offset, int count)
    {
        var page = await discussionsRepository.GetAllAsync(offset, count);
        return new Page<DiscussionDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<Page<DiscussionDto>> GetByAuthorAsync(Guid authorId, int offset, int count)
    {
        var page = await discussionsRepository.GetByAuthorAsync(authorId, offset, count);
        return new Page<DiscussionDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
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
            Author = author.Adapt<AuthorDto>(),
            CreatedAt = discussion.CreatedAt
        };
    }

    private async Task<IEnumerable<DiscussionDto>> EnrichAsync(IReadOnlyList<Discussion> discussions)
    {
        var authorIds = discussions.Select(discussion => discussion.AuthorId);
        var authorsById = await usersService.GetUsersAsync(authorIds.ToHashSet());

        return discussions.Select(discussion => new DiscussionDto
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Description = discussion.Description,
            Author = authorsById[discussion.AuthorId].Adapt<AuthorDto>(),
            CreatedAt = discussion.CreatedAt
        });
    }
}