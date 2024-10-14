using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using CrowdParlay.Social.Domain.ValueObjects;
using Mapster;

namespace CrowdParlay.Social.Application.Services;

public class DiscussionsService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IDiscussionsRepository discussionsRepository,
    IUsersService usersService)
    : IDiscussionsService
{
    public async Task<DiscussionDto> GetByIdAsync(Guid discussionId, Guid? viewerId)
    {
        var discussion = await discussionsRepository.GetByIdAsync(discussionId, viewerId);
        return await EnrichAsync(discussion);
    }

    public async Task<Page<DiscussionDto>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var page = await discussionsRepository.SearchAsync(authorId, viewerId, offset, count);
        return new Page<DiscussionDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<DiscussionDto> CreateAsync(Guid authorId, string title, string description)
    {
        Discussion discussion;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var discussionId = await unitOfWork.DiscussionsRepository.CreateAsync(authorId, title, description);
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, authorId);
        }

        return await EnrichAsync(discussion);
    }

    public async Task<DiscussionDto> AddReactionAsync(Guid authorId, Guid discussionId, Reaction reaction)
    {
        Discussion discussion;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            await unitOfWork.ReactionsRepository.AddAsync(authorId, discussionId, reaction);
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, authorId);
            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(discussion);
    }

    public async Task<DiscussionDto> RemoveReactionAsync(Guid authorId, Guid discussionId, Reaction reaction)
    {
        Discussion discussion;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            await unitOfWork.ReactionsRepository.RemoveAsync(authorId, discussionId, reaction);
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, authorId);
            await unitOfWork.CommitAsync();
        }

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
            CreatedAt = discussion.CreatedAt,
            ReactionCounters = discussion.ReactionCounters,
            ViewerReactions = discussion.ViewerReactions
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
            CreatedAt = discussion.CreatedAt,
            ReactionCounters = discussion.ReactionCounters,
            ViewerReactions = discussion.ViewerReactions
        });
    }
}