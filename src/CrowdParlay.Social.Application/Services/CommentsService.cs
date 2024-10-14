using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using CrowdParlay.Social.Domain.ValueObjects;
using Mapster;

namespace CrowdParlay.Social.Application.Services;

public class CommentsService(
    IUnitOfWorkFactory unitOfWorkFactory,
    ICommentsRepository commentsRepository,
    IUsersService usersService)
    : ICommentsService
{
    public async Task<CommentDto> GetByIdAsync(Guid commentId, Guid? viewerId)
    {
        var comment = await commentsRepository.GetByIdAsync(commentId, viewerId);
        return await EnrichAsync(comment);
    }

    public async Task<Page<CommentDto>> SearchAsync(Guid? discussionId, Guid? authorId, Guid? viewerId, int offset, int count)
    {
        var page = await commentsRepository.SearchAsync(discussionId, authorId, viewerId, offset, count);
        return new Page<CommentDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content)
    {
        Comment comment;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var commentId = await unitOfWork.CommentsRepository.CreateAsync(authorId, discussionId, content);
            comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
        }

        // TODO: notify clients via SignalR

        return await EnrichAsync(comment);
    }

    public async Task<Page<CommentDto>> GetRepliesToCommentAsync(Guid parentCommentId, Guid? viewerId, int offset, int count)
    {
        var page = await commentsRepository.SearchAsync(parentCommentId, authorId: null, viewerId, offset, count);
        return new Page<CommentDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        Comment comment;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            var commentId = await unitOfWork.CommentsRepository.ReplyToCommentAsync(authorId, parentCommentId, content);
            comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
        }

        return await EnrichAsync(comment);
    }

    public async Task<CommentDto> AddReactionAsync(Guid authorId, Guid commentId, Reaction reaction)
    {
        Comment comment;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            await unitOfWork.ReactionsRepository.AddAsync(authorId, commentId, reaction);
            comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(comment);
    }

    public async Task<CommentDto> RemoveReactionAsync(Guid authorId, Guid commentId, Reaction reaction)
    {
        Comment comment;
        await using (var unitOfWork = await unitOfWorkFactory.CreateAsync())
        {
            await unitOfWork.ReactionsRepository.RemoveAsync(authorId, commentId, reaction);
            comment = await unitOfWork.CommentsRepository.GetByIdAsync(commentId, authorId);
            await unitOfWork.CommitAsync();
        }

        return await EnrichAsync(comment);
    }

    public async Task DeleteAsync(Guid id) => await commentsRepository.DeleteAsync(id);

    private async Task<CommentDto> EnrichAsync(Comment comment)
    {
        var author = await usersService.GetByIdAsync(comment.AuthorId);
        var firstRepliesAuthors = await usersService.GetUsersAsync(comment.FirstRepliesAuthorIds);

        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = author.Adapt<AuthorDto>(),
            CreatedAt = comment.CreatedAt,
            ReplyCount = comment.ReplyCount,
            FirstRepliesAuthors = firstRepliesAuthors.Values.Adapt<IEnumerable<AuthorDto>>(),
            ReactionCounters = comment.ReactionCounters,
            ViewerReactions = comment.ViewerReactions
        };
    }

    private async Task<IEnumerable<CommentDto>> EnrichAsync(IReadOnlyList<Comment> comments)
    {
        var authorIds = comments.SelectMany(comment => comment.FirstRepliesAuthorIds.Append(comment.AuthorId)).ToHashSet();
        var authorsById = await usersService.GetUsersAsync(authorIds);

        return comments.Select(comment => new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = authorsById[comment.AuthorId].Adapt<AuthorDto>(),
            CreatedAt = comment.CreatedAt,
            ReplyCount = comment.ReplyCount,
            FirstRepliesAuthors = comment.FirstRepliesAuthorIds
                .Select(replyAuthorId => authorsById[replyAuthorId].Adapt<AuthorDto>()),
            ReactionCounters = comment.ReactionCounters,
            ViewerReactions = comment.ViewerReactions
        });
    }
}