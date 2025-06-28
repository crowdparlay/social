using System.Diagnostics;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Aspects;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;
using Metalama.Framework.Code;

// ReSharper disable ExplicitCallerInfoArgument

namespace CrowdParlay.Social.Application.Services;

[TraceMethods(Accessibility.Public, Accessibility.Private)]
public class CommentsService(
    IUnitOfWorkFactory unitOfWorkFactory,
    ICommentsRepository commentsRepository,
    IDiscussionsRepository discussionsRepository,
    IUsersService usersService)
    : ICommentsService
{
    private readonly ISubjectsService _subjectsService = new SubjectsService(commentsRepository);
    private readonly ActivitySource _activitySource = new(typeof(CommentsService).FullName!);

    public async Task<CommentResponse> GetByIdAsync(string commentId, Guid? viewerId)
    {
        var comment = await commentsRepository.GetByIdAsync(commentId, viewerId);
        return await EnrichAsync(comment);
    }

    public async Task<Page<CommentResponse>> GetRepliesAsync(string subjectId, bool flatten, Guid? viewerId, int offset, int count)
    {
        var page = await commentsRepository.GetRepliesAsync(subjectId, flatten, viewerId, offset, count);
        return new Page<CommentResponse>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentResponse> ReplyToDiscussionAsync(string discussionId, Guid authorId, string content)
    {
        await discussionsRepository.GetByIdAsync(discussionId, null);
        return await CreateAsync(discussionId, authorId, content);
    }

    public async Task<CommentResponse> ReplyToCommentAsync(string commentId, Guid authorId, string content)
    {
        await commentsRepository.GetByIdAsync(commentId, null);
        return await CreateAsync(commentId, authorId, content);
    }

    private async Task<CommentResponse> CreateAsync(string subjectId, Guid authorId, string content)
    {
        Comment comment;
        using (_activitySource.StartActivity("Save and retrieve entity"))
        {
            using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
            {
                var commentId = await unitOfWork.CommentsRepository.CreateAsync(subjectId, authorId, content);
                comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
                await unitOfWork.CommitAsync();
            }
        }

        using (_activitySource.StartActivity("Update ancestors"))
        {
            var ancestors = await commentsRepository.GetAncestorsAsync(comment.Id, null);
            await commentsRepository.IncludeCommentInAncestorsMetadataAsync(ancestors, authorId);

            var discussionId = ancestors.LastOrDefault()?.SubjectId ?? subjectId;
            await discussionsRepository.IncludeCommentInMetadataAsync(discussionId, authorId);
        }

        return await EnrichAsync(comment);
    }

    public async Task<ISet<string>> GetReactionsAsync(string commentId, Guid authorId) =>
        await _subjectsService.GetReactionsAsync(commentId, authorId);

    public async Task SetReactionsAsync(string commentId, Guid authorId, ISet<string> reactions) =>
        await _subjectsService.SetReactionsAsync(commentId, authorId, reactions);

    public async Task DeleteAsync(string commentId)
    {
        var comment = await commentsRepository.GetByIdAsync(commentId, null);
        using (_activitySource.StartActivity("Update ancestors", ActivityKind.Server))
        {
            var ancestors = await commentsRepository.GetAncestorsAsync(commentId, null);
            await commentsRepository.ExcludeCommentFromAncestorsMetadataAsync(ancestors);

            var discussionId = ancestors.LastOrDefault()?.SubjectId ?? comment.SubjectId;
            await discussionsRepository.ExcludeCommentFromMetadataAsync(discussionId);
        }

        using (_activitySource.StartActivity("Delete entity", ActivityKind.Server))
            await commentsRepository.DeleteAsync(commentId);
    }

    private async Task<CommentResponse> EnrichAsync([TraceIgnore] Comment comment) => (await EnrichAsync([comment])).First();

    private async Task<IEnumerable<CommentResponse>> EnrichAsync([TraceIgnore] IReadOnlyList<Comment> comments)
    {
        var authorIds = comments
            .SelectMany(comment => comment.LastCommentsAuthorIds.Append(comment.AuthorId))
            .ToHashSet();

        var authorsById = await usersService.GetUsersAsync(authorIds);
        return comments.Select(comment => new CommentResponse
        {
            Id = comment.Id,
            SubjectId = comment.SubjectId,
            Content = comment.Content,
            Author = authorsById[comment.AuthorId].Adapt<AuthorResponse>(),
            CreatedAt = comment.CreatedAt,
            CommentCount = comment.CommentCount,
            LastCommentsAuthors = comment.LastCommentsAuthorIds
                .Select(replyAuthorId => authorsById[replyAuthorId].Adapt<AuthorResponse>()),
            ReactionCounters = comment.ReactionCounters,
            ViewerReactions = comment.ViewerReactions.ToHashSet()
        });
    }
}