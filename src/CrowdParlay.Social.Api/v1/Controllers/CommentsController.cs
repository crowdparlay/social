using System.Net;
using System.Net.Mime;
using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.Hubs;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.DTOs;
using CrowdParlay.Social.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class CommentsController(ICommentsService comments, IHubContext<CommentsHub> commentHub) : ControllerBase
{
    /// <summary>
    /// Returns comment with the specified ID.
    /// </summary>
    [HttpGet("{commentId:guid}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<CommentDto> GetCommentById([FromRoute] Guid commentId) =>
        await comments.GetByIdAsync(commentId, User.GetUserId());

    /// <summary>
    /// Get comments by filters.
    /// </summary>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Page<CommentDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<Page<CommentDto>> SearchComments(
        [FromQuery] Guid? discussionId,
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await comments.SearchAsync(discussionId, authorId, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a top-level comment in discussion.
    /// </summary>
    [HttpPost, Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CommentRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await comments.CreateAsync(authorId, request.DiscussionId, request.Content);

        _ = commentHub.Clients
            .Group(CommentsHub.GroupNames.NewCommentInDiscussion(request.DiscussionId))
            .SendCoreAsync(CommentsHub.Events.NewComment.ToString(), [response]);

        return CreatedAtAction(nameof(GetCommentById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Get replies to the comment with the specified ID.
    /// </summary>
    [HttpGet("{parentCommentId:guid}/replies")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Page<CommentDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<Page<CommentDto>> GetRepliesToComment(
        [FromRoute] Guid parentCommentId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await comments.GetRepliesToCommentAsync(parentCommentId, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a reply to the comment with the specified ID.
    /// </summary>
    [HttpPost("{parentCommentId:guid}/replies"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid parentCommentId, [FromBody] ReplyRequest request)
    {
        var response = await comments.ReplyToCommentAsync(User.GetRequiredUserId(), parentCommentId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// React to the comment.
    /// </summary>
    [HttpPost("{commentId:guid}/replies"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid commentId, [FromBody] string reaction)
    {
        var response = await comments.AddReactionAsync(User.GetRequiredUserId(), commentId, new Reaction(reaction));
        return CreatedAtAction(nameof(GetCommentById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Add a reaction to a comment
    /// </summary>
    [HttpPost("{commentId:guid}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<CommentDto> AddReaction([FromRoute] Guid commentId, [FromBody] string reaction) =>
        await comments.AddReactionAsync(User.GetRequiredUserId(), commentId, reaction);

    /// <summary>
    /// Remove a reaction from a comment
    /// </summary>
    [HttpDelete("{commentId:guid}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(CommentDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<CommentDto> RemoveReaction([FromRoute] Guid commentId, [FromBody] string reaction) =>
        await comments.AddReactionAsync(User.GetRequiredUserId(), commentId, reaction);
}