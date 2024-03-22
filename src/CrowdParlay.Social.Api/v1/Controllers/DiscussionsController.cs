using System.Net;
using System.Net.Mime;
using CrowdParlay.Social.Api.Extensions;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using CrowdParlay.Social.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class DiscussionsController(IDiscussionRepository discussions) : ControllerBase
{
    /// <summary>
    /// Returns discussion with the specified ID.
    /// </summary>
    [HttpGet("{discussionId:guid}"), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task<DiscussionDto> GetDiscussionById([FromRoute] Guid discussionId) =>
        await discussions.GetByIdAsync(discussionId);

    /// <summary>
    /// Returns all discussions created by author with the specified ID.
    /// </summary>
    [HttpGet, Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<DiscussionDto>), (int)HttpStatusCode.OK)]
    public async Task<IEnumerable<DiscussionDto>> GetDiscussions([FromQuery] Guid? authorId) => authorId is null
        ? await discussions.GetAllAsync()
        : await discussions.GetByAuthorAsync(authorId.Value);

    /// <summary>
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize, Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<DiscussionDto>> CreateDiscussion([FromBody] DiscussionRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await discussions.CreateAsync(authorId, request.Title, request.Description);
        return CreatedAtAction(nameof(GetDiscussionById), new { DiscussionId = response.Id }, response);
    }
}