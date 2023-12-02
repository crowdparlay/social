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
    public async Task<DiscussionDto> GetDiscussionById([FromRoute] Guid discussionId) =>
        await _discussions.GetByIdAsync(discussionId);

    /// <summary>
    /// Returns all discussions.
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<DiscussionDto>> GetAllDiscussions() =>
        await _discussions.GetAllAsync();

    /// <summary>
    /// Returns all discussions created by author with the specified ID.
    /// </summary>
    [HttpGet("{authorId}")]
    public async Task<IEnumerable<DiscussionDto>> GetDiscussionsByAuthor([FromQuery] Guid authorId) =>
        await _discussions.GetByAuthorAsync(authorId);

    /// <summary>
    /// Creates a discussion.
    /// </summary>
    [HttpPost, Authorize]
    public async Task<ActionResult<DiscussionDto>> CreateDiscussion([FromBody] DiscussionRequest request)
    {
        var authorId =
            User.GetUserId()
            ?? throw new ForbiddenException();

        var response = await _discussions.CreateAsync(authorId, request.Title, request.Description);
        return CreatedAtAction(nameof(GetDiscussionById), new { DiscussionId = response.Id }, response);
    }
}