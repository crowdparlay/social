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
    /// Retrieves a discussion by its unique identifier.
    /// </summary>
    /// <param name="discussionId">The unique identifier of the discussion to retrieve.</param>
    /// <returns>The requested discussion details.</returns>
    [HttpGet("{discussionId}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<DiscussionResponse> GetById([FromRoute] string discussionId) =>
        await discussionsService.GetByIdAsync(discussionId, User.GetUserId());

    /// <summary>
    /// Searches discussions filtered by author identifier with pagination.
    /// </summary>
    /// <param name="authorId">Optional author identifier to filter discussions (when null, returns all discussions).</param>
    /// <param name="offset">The number of items to skip before starting to return results (pagination offset).</param>
    /// <param name="count">The maximum number of items to return (pagination limit).</param>
    /// <returns>A paginated list of matching discussions.</returns>
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
    /// Retrieves comments for a specified discussion with configurable tree traversal and pagination.
    /// </summary>
    /// <param name="discussionId">The unique identifier of the discussion.</param>
    /// <param name="flatten">When true, returns all nested replies in a flat structure; otherwise returns direct children only.</param>
    /// <param name="offset">The number of items to skip before starting to return results (pagination offset).</param>
    /// <param name="count">The maximum number of items to return (pagination limit).</param>
    /// <returns>A paginated list of top-level comments in the discussion.</returns>
    [HttpGet("{discussionId}/comments")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Page<CommentResponse>>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<Page<CommentResponse>> GetComments(
        [FromRoute] string discussionId,
        [FromQuery, BindRequired] bool flatten,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await commentsService.GetRepliesAsync(discussionId, flatten, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a new top-level comment in a discussion. Requires authenticated user and triggers real-time notifications.
    /// </summary>
    /// <param name="discussionId">The unique identifier of the discussion being commented.</param>
    /// <param name="request">The comment content and target discussion identifier.</param>
    /// <returns>The newly created top-level comment details.</returns>
    [HttpPost("{discussionId}/comments"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<CommentResponse>> Comment([FromRoute] string discussionId, [FromBody] CommentRequest request)
    {
        var response = await commentsService.ReplyToDiscussionAsync(discussionId, User.GetRequiredUserId(), request.Content);

        // TODO: handle exceptions and move to a better place
        _ = commentHub.Clients
            .Group(CommentsHub.GroupNames.NewCommentInDiscussion(discussionId))
            .SendCoreAsync(CommentsHub.Events.NewComment.ToString(), [response]);

        return CreatedAtAction(nameof(CommentsController.GetById), "Comments", new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Creates a new discussion. Requires authenticated user.
    /// </summary>
    /// <param name="request">The discussion title and content.</param>
    /// <returns>The newly created discussion details.</returns>
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
    /// Modifies an existing discussion. Requires authenticated user and ownership permissions.
    /// </summary>
    /// <param name="discussionId">The unique identifier of the discussion to update.</param>
    /// <param name="request">The updated discussion data.</param>
    /// <returns>The modified discussion details.</returns>
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
    /// Updates reactions on a specified discussion. Requires authenticated user.
    /// </summary>
    /// <param name="discussionId">The unique identifier of the discussion to react to.</param>
    /// <param name="reactions">The set of reaction identifiers to apply.</param>
    /// <returns>No content response upon successful update.</returns>
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