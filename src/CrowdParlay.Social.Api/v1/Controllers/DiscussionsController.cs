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
public class DiscussionsController(IDiscussionsService discussionsService, IReactionsService reactionsService) : ControllerBase
{
    /// <summary>
    /// Returns discussion with the specified ID.
    /// </summary>
    [HttpGet("{discussionId:guid}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<DiscussionResponse> GetById([FromRoute] Guid discussionId) =>
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
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status201Created)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<ActionResult<DiscussionResponse>> Create([FromBody] DiscussionRequest request)
    {
        var response = await discussionsService.CreateAsync(User.GetRequiredUserId(), request.Title, request.Description);
        return CreatedAtAction(nameof(GetById), new { DiscussionId = response.Id }, response);
    }

    /// <summary>
    /// Modifies a discussion.
    /// </summary>
    [HttpPatch("{discussionId:guid}"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<DiscussionResponse>(Status200OK)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task<DiscussionResponse> Update([FromRoute] Guid discussionId, [FromBody] UpdateDiscussionRequest request) =>
        await discussionsService.UpdateAsync(discussionId, User.GetRequiredUserId(), request);

    /// <summary>
    /// Sets reactions to a discussion.
    /// </summary>
    [HttpPost("{discussionId:guid}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(Status500InternalServerError)]
    public async Task React([FromRoute] Guid discussionId, [FromBody] ISet<string> reactions) =>
        await reactionsService.SetAsync(discussionId, User.GetRequiredUserId(), reactions);
}