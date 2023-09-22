using CrowdParlay.Social.Api.DTOs;
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
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CommentRequest request)
    {
        var userIdHeaderValue = Request.Headers["X-UserId"].Single()!;
        var authorId = Guid.Parse(userIdHeaderValue);

        var response = await _comments.CreateAsync(authorId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }

    [HttpPost("{targetCommentId}/reply")]
    public async Task<ActionResult<CommentDto>> ReplyToComment([FromRoute] Guid targetCommentId, [FromBody] CommentRequest request)
    {
        var userIdHeaderValue = Request.Headers["X-UserId"].Single()!;
        var authorId = Guid.Parse(userIdHeaderValue);

        var response = await _comments.ReplyAsync(authorId, targetCommentId, request.Content);
        return CreatedAtAction(nameof(GetCommentById), new { CommentId = response.Id }, response);
    }
}