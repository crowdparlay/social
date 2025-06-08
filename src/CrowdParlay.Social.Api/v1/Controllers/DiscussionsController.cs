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
public class DiscussionsController(
    IDiscussionsService discussionsService,
    ICommentsService commentsService,
    IHubContext<CommentsHub> commentHub) : ControllerBase
{
    /// <summary>
    /// Returns discussion with the specified ID.
    /// </summary>
    [HttpGet("{discussionId}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<DiscussionResponse> GetById([FromRoute] string discussionId) =>
        await discussionsService.GetByIdAsync(discussionId, User.GetUserId());

    /// <summary>
    /// Returns all discussions created by author with the specified ID.
    /// </summary>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Page<DiscussionResponse>>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<Page<DiscussionResponse>> Search(
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await discussionsService.SearchAsync(authorId, User.GetUserId(), offset, count);

    /// <summary>
    /// Returns comments to the specified discussion.
    /// </summary>
    [HttpGet("{discussionId}/comments")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Page<DiscussionResponse>>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<Page<CommentResponse>> GetComments(
        [FromRoute] string discussionId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await commentsService.GetRepliesAsync(discussionId, false, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a top-level comment in discussion.
    /// </summary>
    [HttpPost("{discussionId}/comments"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<CommentResponse>> Reply([FromBody] CommentRequest request)
    {
        var response = await commentsService.ReplyToDiscussionAsync(request.DiscussionId, User.GetRequiredUserId(), request.Content);

        _ = commentHub.Clients
            .Group(CommentsHub.GroupNames.NewCommentInDiscussion(request.DiscussionId))
            .SendCoreAsync(CommentsHub.Events.NewComment.ToString(), [response]);

        return CreatedAtAction(nameof(CommentsController.GetById), "Comments", new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status201Created)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<DiscussionResponse>> Create([FromBody] DiscussionRequest request)
    {
        var response = await discussionsService.CreateAsync(User.GetRequiredUserId(), request.Title, request.Content);
        return CreatedAtAction(nameof(GetById), new { DiscussionId = response.Id }, response);
    }

    /// <summary>
    /// Modifies a discussion.
    /// </summary>
    [HttpPatch("{discussionId}"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<DiscussionResponse> Update([FromRoute] string discussionId,
        [FromBody] UpdateDiscussionRequest request) =>
        await discussionsService.UpdateAsync(discussionId, User.GetRequiredUserId(), request);

    /// <summary>
    /// Sets reactions to a discussion.
    /// </summary>
    [HttpPost("{discussionId}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<IActionResult> React([FromRoute] string discussionId, [FromBody] ISet<string> reactions)
    {
        await discussionsService.SetReactionsAsync(discussionId, User.GetRequiredUserId(), reactions);
        return NoContent();
    }
}