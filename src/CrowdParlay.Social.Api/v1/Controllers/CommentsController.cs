using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

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
    /// Returns all comments created by author with the specified ID.
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<CommentDto>> GetCommentsByAuthor
        ([FromQuery] Guid authorId, [FromQuery] int page, [FromQuery] int size) =>
        await _comments.GetByAuthorAsync(authorId, page, size);

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
    /// Creates a reply to comment with the specified ID.
    /// </summary>
    [HttpPost("{targetCommentId}")]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid targetCommentId, [FromBody] ReplyRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _comments.ReplyToCommentAsync(authorId, targetCommentId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }
}