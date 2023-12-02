using CrowdParlay.Social.Api.Routing;
using CrowdParlay.Social.Api.v1.DTOs;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs.Comment;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _comments;

    public CommentsController(ICommentRepository comments) => _comments = comments;

    /// <summary>
    /// Returns comment with the specified ID.
    /// </summary>
    [HttpGet("{commentId}")]
    public async Task<CommentDto> GetCommentById([FromRoute] Guid commentId) =>
        await _comments.FindAsync(commentId);

    /// <summary>
    /// Returns all comments created by author with the specified ID.
    /// </summary>
    [HttpGet]
    public async Task<IEnumerable<CommentDto>> GetCommentsByAuthor
        ([FromQuery] Guid authorId, [FromQuery] int page, [FromQuery] int size) =>
        await _comments.FindByAuthorAsync(authorId, page, size);
    
    /// <summary>
    /// Creates a comment.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CommentRequest request)
    {
        var userIdHeaderValue = Request.Headers["X-UserId"].Single()!;
        var authorId = Guid.Parse(userIdHeaderValue);

        var response = await _comments.CreateAsync(authorId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }

    /// <summary>
    /// Creates a reply to comment with the specified ID.
    /// </summary>
    [HttpPost("{targetCommentId}/reply")]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid targetCommentId, [FromBody] CommentRequest request)
    {
        var userIdHeaderValue = Request.Headers["X-UserId"].Single()!;
        var authorId = Guid.Parse(userIdHeaderValue);

        var response = await _comments.ReplyAsync(authorId, targetCommentId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }
}