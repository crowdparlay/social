using System.Net.Mime;
using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.Hubs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class CommentsController(
    ICommentsService commentsService,
    IReactionsService reactionsService,
    IHubContext<CommentsHub> commentHub) : ControllerBase
{
    /// <summary>
    /// Returns comment with the specified ID.
    /// </summary>
    [HttpGet("{commentId:guid}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<CommentResponse> GetById([FromRoute] Guid commentId) =>
        await commentsService.GetByIdAsync(commentId, User.GetUserId());

    /// <summary>
    /// Get comments by filters.
    /// </summary>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Page<CommentResponse>>(Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<Page<CommentResponse>> Search(
        [FromQuery] Guid? discussionId,
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await commentsService.SearchAsync(discussionId, authorId, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a top-level comment in discussion.
    /// </summary>
    [HttpPost, Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<CommentResponse>> Create([FromBody] CommentRequest request)
    {
        var response = await commentsService.CreateAsync(User.GetRequiredUserId(), request.DiscussionId, request.Content);

        _ = commentHub.Clients
            .Group(CommentsHub.GroupNames.NewCommentInDiscussion(request.DiscussionId))
            .SendCoreAsync(CommentsHub.Events.NewComment.ToString(), [response]);

        return CreatedAtAction(nameof(GetById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Get replies to the comment with the specified ID.
    /// </summary>
    [HttpGet("{parentCommentId:guid}/replies")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Page<CommentResponse>>(Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<Page<CommentResponse>> GetReplies(
        [FromRoute] Guid parentCommentId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await commentsService.GetRepliesToCommentAsync(parentCommentId, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a reply to the comment with the specified ID.
    /// </summary>
    [HttpPost("{parentCommentId:guid}/replies"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<CommentResponse>> Reply([FromRoute] Guid parentCommentId, [FromBody] ReplyRequest request)
    {
        var response = await commentsService.ReplyToCommentAsync(User.GetRequiredUserId(), parentCommentId, request.Content);
        return CreatedAtAction(nameof(GetById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Sets reactions to a comment.
    /// </summary>
    [HttpPost("{commentId:guid}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<IActionResult> React([FromRoute] Guid commentId, [FromBody] ISet<string> reactions)
    {
        await reactionsService.SetAsync(commentId, User.GetRequiredUserId(), reactions);
        return NoContent();
    }
}