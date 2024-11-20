using System.Diagnostics;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;

namespace CrowdParlay.Social.Application.Services;

public class CommentsService(
    IUnitOfWorkFactory unitOfWorkFactory,
    ICommentsRepository commentsRepository,
    IUsersService usersService)
    : ICommentsService
{
    public async Task<CommentResponse> GetByIdAsync(Guid commentId, Guid? viewerId)
    {
        var comment = await commentsRepository.GetByIdAsync(commentId, viewerId);
        return await EnrichAsync(comment);
    }

    public async Task<Page<CommentResponse>> SearchAsync(Guid? discussionId, Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var page = await commentsRepository.SearchAsync(discussionId, authorId, viewerId, offset, count);
        return new Page<CommentResponse>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentResponse> CreateAsync(Guid authorId, Guid discussionId, string content)
    {
        var source = new ActivitySource("test source");
        using var activity = source.CreateActivity("Create comment", ActivityKind.Server);

        Comment comment;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var commentId = await unitOfWork.CommentsRepository.CreateAsync(authorId, discussionId, content);
            comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
            await unitOfWork.CommitAsync();
        }

        // TODO: notify clients via SignalR

        return await EnrichAsync(comment);
    }

    public async Task<Page<CommentResponse>> GetRepliesToCommentAsync(Guid parentCommentId, Guid? viewerId, int offset, int count)
    {
        var page = await commentsRepository.SearchAsync(parentCommentId, authorId: null, viewerId, offset, count);
        return new Page<CommentResponse>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentResponse> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        Comment comment;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var commentId = await unitOfWork.CommentsRepository.ReplyToCommentAsync(authorId, parentCommentId, content);
            comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(comment);
    }

    public async Task DeleteAsync(Guid id) => await commentsRepository.DeleteAsync(id);

    private async Task<CommentResponse> EnrichAsync(Comment comment) => (await EnrichAsync([comment])).First();

    private async Task<IEnumerable<CommentResponse>> EnrichAsync(IReadOnlyList<Comment> comments)
    {
        var authorIds = comments.SelectMany(comment => comment.LastRepliesAuthorIds.Append(comment.AuthorId)).ToHashSet();
        var authorsById = await usersService.GetUsersAsync(authorIds);

        return comments.Select(comment => new CommentResponse
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = authorsById[comment.AuthorId].Adapt<AuthorResponse>(),
            CreatedAt = comment.CreatedAt,
            ReplyCount = comment.ReplyCount,
            LastRepliesAuthors = comment.LastRepliesAuthorIds
                .Select(replyAuthorId => authorsById[replyAuthorId].Adapt<AuthorResponse>()),
            ReactionCounters = comment.ReactionCounters,
            ViewerReactions = comment.ViewerReactions
        });
    }
}