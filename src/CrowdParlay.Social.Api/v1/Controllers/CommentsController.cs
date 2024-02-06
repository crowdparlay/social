using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _comments;

    public CommentsController(ICommentRepository comments) => _comments = comments;

    /// <summary>
    /// Returns comment with the specified ID.
    /// </summary>
    [HttpGet("{commentId}")]
    public async Task<CommentDto> GetCommentById([FromRoute] Guid commentId) =>
        await _comments.GetByIdAsync(commentId);

    /// <summary>
    /// Get comments by filters.
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<CommentDto>> SearchComments(
        [FromQuery] Guid? discussionId,
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int page,
        [FromQuery, BindRequired] int size) =>
        await _comments.SearchAsync(discussionId, authorId, page, size);

    /// <summary>
    /// Creates a top-level comment in discussion.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CommentRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _comments.CreateAsync(authorId, request.DiscussionId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }

    /// <summary>
    /// Get replies to the comment with the specified ID.
    /// </summary>
    [HttpGet("{parentCommentId}/replies")]
    public async Task<IEnumerable<CommentDto>> GetRepliesToComment(
        [FromRoute] Guid parentCommentId,
        [FromQuery, BindRequired] int page,
        [FromQuery, BindRequired] int size) =>
        await _comments.GetRepliesToCommentAsync(parentCommentId, page, size);

    /// <summary>
    /// Creates a reply to the comment with the specified ID.
    /// </summary>
    [HttpPost("{parentCommentId}/replies")]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid parentCommentId, [FromBody] ReplyRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _comments.ReplyToCommentAsync(authorId, parentCommentId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }
}