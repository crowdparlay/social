using System.Net.Mime;
using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class CommentsController(ICommentsService commentsService) : ControllerBase
{
    /// <summary>
    /// Retrieves a comment by its unique identifier.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment to retrieve.</param>
    /// <returns>The requested comment details.</returns>
    [HttpGet("{commentId}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<CommentResponse> GetById([FromRoute] string commentId) =>
        await commentsService.GetByIdAsync(commentId, User.GetUserId());
    
    /// <summary>
    /// Retrieves replies to a specified comment with configurable tree traversal and pagination.
    /// </summary>
    /// <param name="commentId">The unique identifier of the parent comment.</param>
    /// <param name="flatten">When true, returns all nested replies in a flat structure; otherwise returns direct children only.</param>
    /// <param name="offset">The number of items to skip before starting to return results (pagination offset).</param>
    /// <param name="count">The maximum number of items to return (pagination limit).</param>
    /// <returns>A paginated list of comment replies.</returns>
    [HttpGet("{commentId}/replies")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<Page<CommentResponse>>(Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<Page<CommentResponse>> GetReplies(
        [FromRoute] string commentId,
        [FromQuery, BindRequired] bool flatten,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await commentsService.GetRepliesAsync(commentId, flatten, User.GetUserId(), offset, count);
    
    /// <summary>
    /// Creates a new reply to a specified comment. Requires authenticated user.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment being replied to.</param>
    /// <param name="request">The content of the reply.</param>
    /// <returns>The newly created reply comment details.</returns>
    [HttpPost("{commentId}/replies"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<CommentResponse>> Reply([FromRoute] string commentId, [FromBody] ReplyRequest request)
    {
        var response = await commentsService.ReplyToCommentAsync(commentId, User.GetRequiredUserId(), request.Content);
        return CreatedAtAction(nameof(GetById), new { commentId = response.Id }, response);
    }

    /// <summary>
    /// Updates reactions on a specified comment. Requires authenticated user.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment to react to.</param>
    /// <param name="reactions">The set of reaction identifiers to apply.</param>
    /// <returns>No content response upon successful update.</returns>
    [HttpPost("{commentId}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<IActionResult> React([FromRoute] string commentId, [FromBody] ISet<string> reactions)
    {
        await commentsService.SetReactionsAsync(commentId, User.GetRequiredUserId(), reactions);
        return NoContent();
    }
}