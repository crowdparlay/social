using System.Net;
using System.Net.Mime;
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
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound, MediaTypeNames.Application.Json)]
    public async Task<CommentDto> GetCommentById([FromRoute] Guid commentId) =>
        await _comments.GetByIdAsync(commentId);

    /// <summary>
    /// Get comments by filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Page<CommentDto>), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.Json)]
    public async Task<Page<CommentDto>> SearchComments(
        [FromQuery] Guid? discussionId,
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await _comments.SearchAsync(discussionId, authorId, offset, count);

    /// <summary>
    /// Creates a top-level comment in discussion.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CommentRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _comments.CreateAsync(authorId, request.DiscussionId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Get replies to the comment with the specified ID.
    /// </summary>
    [HttpGet("{parentCommentId}/replies")]
    [ProducesResponseType(typeof(Page<CommentDto>), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound, MediaTypeNames.Application.Json)]
    public async Task<Page<CommentDto>> GetRepliesToComment(
        [FromRoute] Guid parentCommentId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await _comments.GetRepliesToCommentAsync(parentCommentId, offset, count);

    /// <summary>
    /// Creates a reply to the comment with the specified ID.
    /// </summary>
    [HttpPost("{parentCommentId}/replies")]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid parentCommentId, [FromBody] ReplyRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _comments.ReplyToCommentAsync(authorId, parentCommentId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { commentId = response.Id }, response);
    }
}