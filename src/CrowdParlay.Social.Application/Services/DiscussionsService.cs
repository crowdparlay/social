using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;

namespace CrowdParlay.Social.Application.Services;

public class DiscussionsService(
    IUnitOfWorkFactory unitOfWorkFactory,
    IDiscussionsRepository discussionsRepository,
    IReactionsService reactionsService,
    IUsersService usersService)
    : IDiscussionsService
{
    public async Task<DiscussionResponse> GetByIdAsync(Guid discussionId, Guid? viewerId)
    {
        var discussion = await discussionsRepository.GetByIdAsync(discussionId, viewerId);
        return await EnrichAsync(discussion);
    }

    public async Task<Page<DiscussionResponse>> SearchAsync(Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var page = await discussionsRepository.SearchAsync(authorId, viewerId, offset, count);
        return new Page<DiscussionResponse>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<DiscussionResponse> CreateAsync(Guid authorId, string title, string description)
    {
        Discussion discussion;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var discussionId = await unitOfWork.DiscussionsRepository.CreateAsync(authorId, title, description);
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, authorId);
            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(discussion);
    }

    public async Task<DiscussionResponse> UpdateAsync(Guid discussionId, Guid viewerId, UpdateDiscussionRequest request)
    {
        Discussion discussion;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, viewerId);
            if (discussion.AuthorId != viewerId)
                throw new ForbiddenException("Cannot modify a discussion created by another user.");

            await unitOfWork.DiscussionsRepository.UpdateAsync(discussionId, request.Title, request.Description);
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, viewerId);

            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(discussion);
    }

    private async Task<DiscussionResponse> EnrichAsync(Discussion discussion)
    {
        var author = await usersService.GetByIdAsync(discussion.AuthorId);
        return new DiscussionResponse
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Description = discussion.Description,
            Author = author.Adapt<AuthorResponse>(),
            CreatedAt = discussion.CreatedAt,
            ReactionCounters = discussion.ReactionCounters,
            ViewerReactions = discussion.ViewerReactions
        };
    }

    private async Task<IEnumerable<DiscussionResponse>> EnrichAsync(IReadOnlyList<Discussion> discussions)
    {
        var authorIds = discussions.Select(discussion => discussion.AuthorId);
        var authorsById = await usersService.GetUsersAsync(authorIds.ToHashSet());

        return discussions.Select(discussion => new DiscussionResponse
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Description = discussion.Description,
            Author = authorsById[discussion.AuthorId].Adapt<AuthorResponse>(),
            CreatedAt = discussion.CreatedAt,
            ReactionCounters = discussion.ReactionCounters,
            ViewerReactions = discussion.ViewerReactions
        });
    }
}