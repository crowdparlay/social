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
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionRepository _discussions;

    public DiscussionsController(IDiscussionRepository discussions) => _discussions = discussions;

    /// <summary>
    /// Returns discussion with the specified ID.
    /// </summary>
    [HttpGet("{discussionId}")]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound, MediaTypeNames.Application.Json)]
    public async Task<DiscussionDto> GetDiscussionById([FromRoute] Guid discussionId) =>
        await _discussions.GetByIdAsync(discussionId);

    /// <summary>
    /// Returns all discussions created by author with the specified ID.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DiscussionDto>), (int)HttpStatusCode.OK, MediaTypeNames.Application.Json)]
    public async Task<IEnumerable<DiscussionDto>> GetDiscussions([FromQuery] Guid? authorId) => authorId is null
        ? await _discussions.GetAllAsync()
        : await _discussions.GetByAuthorAsync(authorId.Value);

    /// <summary>
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize]
    [ProducesResponseType(typeof(DiscussionDto), (int)HttpStatusCode.Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<DiscussionDto>> CreateDiscussion([FromBody] DiscussionRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _discussions.CreateAsync(authorId, request.Title, request.Description);
        return CreatedAtAction(nameof(GetDiscussionById), new { DiscussionId = response.Id }, response);
    }
}