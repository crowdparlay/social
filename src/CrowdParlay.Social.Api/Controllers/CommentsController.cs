using CrowdParlay.Social.Application.DTOs.Comment;
using CrowdParlay.Social.Application.Features.Comments.Commands;
using CrowdParlay.Social.Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{commentId}")]
    public async Task<CommentDto> GetCommentById([FromRoute] Guid commentId) =>
        await _mediator.Send(new GetCommentByIdQuery(commentId));

    [HttpGet]
    public async Task<IEnumerable<CommentDto>> GetCommentsByAuthor([FromQuery] Guid authorId) =>
        await _mediator.Send(new GetCommentsByAuthorQuery(authorId));

    [HttpPost, FeatureGate("BackdoorEndpoints")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentCommand command) =>
        Created("(GET by Comment ID)", await _mediator.Send(command));

    [HttpPost("{replyToCommentId}/reply")]
    public async Task<IActionResult> ReplyToComment([FromRoute] Guid replyToCommentId, [FromBody] CreateReplyToCommentCommand command) =>
        Created("(GET by Comment ID)", await _mediator.Send(command with
        {
            InReplyToCommentId = replyToCommentId
        }));
}