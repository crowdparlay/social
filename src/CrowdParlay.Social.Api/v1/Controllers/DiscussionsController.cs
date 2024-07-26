using System.Net;
using System.Net.Mime;
using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using CrowdParlay.Social.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class DiscussionsController(IDiscussionsService discussionsService) : ControllerBase
{
    /// <summary>
    /// Returns discussion with the specified ID.
    /// </summary>
    [HttpGet("{discussionId:guid}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.NotFound)]
    public async Task<DiscussionDto> GetDiscussionById([FromRoute] Guid discussionId) =>
        await discussionsService.GetByIdAsync(discussionId);

    /// <summary>
    /// Returns all discussions created by author with the specified ID.
    /// </summary>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Page<DiscussionDto>), (int)HttpStatusCode.OK)]
    public async Task<Page<DiscussionDto>> GetDiscussions(
        [FromQuery] Guid? authorId,
        [FromQuery, BindRequired] int offset,
        [FromQuery, BindRequired] int count) => authorId is null
        ? await discussionsService.GetAllAsync(offset, count)
        : await discussionsService.GetByAuthorAsync(authorId.Value, offset, count);

    /// <summary>
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<DiscussionDto>> CreateDiscussion([FromBody] DiscussionRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await discussionsService.CreateAsync(authorId, request.Title, request.Description);
        return CreatedAtAction(nameof(GetDiscussionById), new { DiscussionId = response.Id }, response);
    }
}