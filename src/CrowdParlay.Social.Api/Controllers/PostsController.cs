using CrowdParlay.Social.Application.DTOs.Post;
using CrowdParlay.Social.Application.Features.Commands;
using CrowdParlay.Social.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]/[action]")]
public class PostsController : ControllerBase
{
    private readonly ISender _sender;

    public PostsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{authorId:guid}")]
    public async Task<CreatedAtActionResult> Create([FromRoute] Guid authorId, [FromBody] string content)
    {
        var createdPost = await _sender.Send(new CreatePostCommand(authorId, content));
        return CreatedAtAction(nameof(Get), new { postId = createdPost.Id }, createdPost);
    }
    
    [HttpGet("{postId:guid}")]
    public async Task<ActionResult> Get([FromRoute] Guid postId)
    {
        return Ok(await _sender.Send(new GetPostByIdQuery(postId)));
    }

    [HttpGet("{offset:int}/{limit:int}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetAll(int offset, int limit)
    {
        return Ok(await _sender.Send(new GetAllPostsQuery(offset, limit)));
    }
}