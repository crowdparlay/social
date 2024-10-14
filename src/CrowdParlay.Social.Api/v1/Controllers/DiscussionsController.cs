using System.Net;
using System.Net.Mime;
using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class DiscussionsController(IDiscussionsService discussions) : ControllerBase
{
    /// <summary>
    /// Returns discussion with the specified ID.
    /// </summary>
    [HttpGet("{discussionId:guid}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<DiscussionDto> GetDiscussionById([FromRoute] Guid discussionId) =>
        await discussions.GetByIdAsync(discussionId, User.GetUserId());

    /// <summary>
    /// Returns all discussions created by author with the specified ID.
    /// </summary>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Page<DiscussionDto>), (int)HttpStatusCode.OK)]
    public async Task<Page<DiscussionDto>> SearchDiscussions(
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) =>
        await discussions.SearchAsync(authorId, User.GetUserId(), offset, count);

    /// <summary>
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<DiscussionDto>> CreateDiscussion([FromBody] DiscussionRequest request)
    {
        var response = await discussions.CreateAsync(User.GetRequiredUserId(), request.Title, request.Description);
        return CreatedAtAction(nameof(GetDiscussionById), new { DiscussionId = response.Id }, response);
    }

    /// <summary>
    /// Add a reaction to a comment
    /// </summary>
    [HttpPost("{discussionId:guid}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<DiscussionDto> AddReaction([FromRoute] Guid discussionId, [FromBody] string reaction) =>
        await discussions.AddReactionAsync(User.GetRequiredUserId(), discussionId, reaction);

    /// <summary>
    /// Remove a reaction from a comment
    /// </summary>
    [HttpDelete("{commentId:guid}/reactions"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<DiscussionDto> RemoveReaction([FromRoute] Guid discussionId, [FromBody] string reaction) =>
        await discussions.AddReactionAsync(User.GetRequiredUserId(), discussionId, reaction);
}