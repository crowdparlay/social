using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs.Comment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _comments;

    public CommentsController(ICommentRepository comments) => _comments = comments;

    [HttpGet("{commentId}")]
    public async Task<CommentDto> GetCommentById([FromRoute] Guid commentId) =>
        await _comments.FindAsync(commentId);

    [HttpGet]
    public async Task<IEnumerable<CommentDto>> GetCommentsByAuthor([FromQuery] Guid authorId) =>
        await _comments.FindByAuthorAsync(authorId);

    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] string content)
    {
        var userIdHeaderValue = Request.Headers["X-UserId"].Single()!;
        var authorId = Guid.Parse(userIdHeaderValue);

        var response = await _comments.CreateAsync(authorId, content);
        return CreatedAtAction(nameof(GetCommentById), response);
    }

    [HttpPost("{targetCommentId}/reply")]
    public async Task<IActionResult> ReplyToComment([FromRoute] Guid targetCommentId, [FromBody] string content)
    {
        var userIdHeaderValue = Request.Headers["X-UserId"].Single()!;
        var authorId = Guid.Parse(userIdHeaderValue);

        var response = await _comments.ReplyAsync(authorId, targetCommentId, content);
        return CreatedAtAction(nameof(GetCommentById), response);
    }
}