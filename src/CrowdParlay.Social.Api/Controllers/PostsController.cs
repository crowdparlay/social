using System.ComponentModel.DataAnnotations;
using CrowdParlay.Social.Application.DTOs.Post;
using CrowdParlay.Social.Application.Features.Commands;
using CrowdParlay.Social.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly ISender _sender;

    public PostsController(ISender sender) => _sender = sender;

    [HttpPost("{authorId:guid}")]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<CreatedAtActionResult> Create([FromRoute] Guid authorId, [FromBody] string content)
    {
        var createdPost = await _sender.Send(new CreatePostCommand(authorId, content.Trim()));
        return CreatedAtAction(nameof(Get), new { postId = createdPost.Id }, createdPost);
    }

    [HttpGet("{postId:guid}")]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    public async Task<PostDto> Get([FromRoute] Guid postId) =>
        await _sender.Send(new GetPostByIduQuery(postId));

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<PostDto>> GetAll([FromQuery, Required] int offset, [FromQuery, Required] int limit) =>
        await _sender.Send(new GetAllPostsQuery(offset, limit));
}