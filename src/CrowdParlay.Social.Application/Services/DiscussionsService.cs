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
    IUsersService usersService)
    : IDiscussionsService
{
    private readonly ISubjectsService _subjectsService = new SubjectsService(discussionsRepository);
    
    public async Task<DiscussionResponse> GetByIdAsync(string discussionId, Guid? viewerId)
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

    public async Task<DiscussionResponse> CreateAsync(Guid authorId, string title, string content)
    {
        Discussion discussion;
        
        using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var discussionId = await discussionsRepository.CreateAsync(authorId, title, content);
            discussion = await unitOfWork.DiscussionsRepository.GetByIdAsync(discussionId, authorId);
            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(discussion);
    }

    public async Task<DiscussionResponse> UpdateAsync(string discussionId, Guid viewerId,
        UpdateDiscussionRequest request)
    {
        var discussion = await discussionsRepository.GetByIdAsync(discussionId, viewerId);
        if (discussion.AuthorId != viewerId)
            throw new ForbiddenException("Cannot modify a discussion created by another user.");

        await discussionsRepository.UpdateAsync(discussionId, request.Title, request.Description);
        discussion = await discussionsRepository.GetByIdAsync(discussionId, viewerId);
        return await EnrichAsync(discussion);
    }

    public async Task<ISet<string>> GetReactionsAsync(string discussionId, Guid authorId) =>
        await _subjectsService.GetReactionsAsync(discussionId, authorId);

    public async Task SetReactionsAsync(string discussionId, Guid authorId, ISet<string> reactions) =>
        await _subjectsService.SetReactionsAsync(discussionId, authorId, reactions);

    private async Task<DiscussionResponse> EnrichAsync(Discussion discussion) => (await EnrichAsync([discussion])).First();

    private async Task<IEnumerable<DiscussionResponse>> EnrichAsync(IReadOnlyList<Discussion> discussions)
    {
        var authorIds = discussions
            .SelectMany(discussion => discussion.LastCommentsAuthorIds.Append(discussion.AuthorId))
            .ToHashSet();
        
        var authorsById = await usersService.GetUsersAsync(authorIds);
        return discussions.Select(discussion => new DiscussionResponse
        {
            Id = discussion.Id,
            Title = discussion.Title,
            Content = discussion.Content,
            Author = authorsById[discussion.AuthorId].Adapt<AuthorResponse>(),
            CreatedAt = discussion.CreatedAt,
            CommentCount = discussion.CommentCount,
            LastCommentsAuthors = discussion.LastCommentsAuthorIds
                .Select(replyAuthorId => authorsById[replyAuthorId].Adapt<AuthorResponse>()),
            ReactionCounters = discussion.ReactionCounters,
            ViewerReactions = discussion.ViewerReactions.ToHashSet()
        });
    }
}