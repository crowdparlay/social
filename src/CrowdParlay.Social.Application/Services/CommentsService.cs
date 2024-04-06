using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.Abstractions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.Entities;
using Mapster;

namespace CrowdParlay.Social.Application.Services;

public class CommentsService(ICommentRepository commentRepository, IUsersService usersService) : ICommentsService
{
    public async Task<CommentDto> GetByIdAsync(Guid id)
    {
        var comment = await commentRepository.GetByIdAsync(id);
        return await EnrichAsync(comment);
    }

    public async Task<Page<CommentDto>> SearchAsync(Guid? discussionId, Guid? authorId, int offset, int count)
    {
        var page = await commentRepository.SearchAsync(discussionId, authorId, offset, count);
        return new Page<CommentDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentDto> CreateAsync(Guid authorId, Guid discussionId, string content)
    {
        var comment = await commentRepository.CreateAsync(authorId, discussionId, content);
        var result = await EnrichAsync(comment);

        // TODO: notify clients via SignalR

        return result;
    }

    public async Task<Page<CommentDto>> GetRepliesToCommentAsync(Guid parentCommentId, int offset, int count)
    {
        var page = await commentRepository.GetRepliesToCommentAsync(parentCommentId, offset, count);
        return new Page<CommentDto>
        {
            TotalCount = page.TotalCount,
            Items = await EnrichAsync(page.Items.ToArray())
        };
    }

    public async Task<CommentDto> ReplyToCommentAsync(Guid authorId, Guid parentCommentId, string content)
    {
        var comment = await commentRepository.ReplyToCommentAsync(authorId, parentCommentId, content);
        return await EnrichAsync(comment);
    }

    public async Task DeleteAsync(Guid id) => await commentRepository.DeleteAsync(id);

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
            FirstRepliesAuthors = firstRepliesAuthors.Values.Adapt<IEnumerable<AuthorDto>>()
        };
    }

    private async Task<IEnumerable<CommentDto>> EnrichAsync(IReadOnlyList<Comment> comments)
    {
        var authorIds = comments.SelectMany(comment => comment.FirstRepliesAuthorIds.Append(comment.AuthorId)).ToHashSet();
        var authorsById = await usersService.GetUsersAsync(authorIds);

        return comments.Select(comment =>
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Author = authorsById[comment.AuthorId].Adapt<AuthorDto>(),
                CreatedAt = comment.CreatedAt,
                ReplyCount = comment.ReplyCount,
                FirstRepliesAuthors = comment.FirstRepliesAuthorIds
                    .Select(replyAuthorId => authorsById[replyAuthorId].Adapt<AuthorDto>())
            };
        });
    }
}