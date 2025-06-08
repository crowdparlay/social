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
    /// Returns comment with the specified ID.
    /// </summary>
    [HttpGet("{commentId}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CommentResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<CommentResponse> GetById([FromRoute] string commentId) =>
        await commentsService.GetByIdAsync(commentId, User.GetUserId());
    
    /// <summary>
    /// Get replies to the comment with the specified ID.
    /// </summary>
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
    /// Creates a reply to the comment with the specified ID.
    /// </summary>
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
    /// Sets reactions to a comment.
    /// </summary>
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