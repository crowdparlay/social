using CrowdParlay.Social.Application.DTOs.Post;
using CrowdParlay.Social.Application.Features.Commands;
using CrowdParlay.Social.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]/[action]")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreatePostDto createPostDto)
    {
        var createdPost = await _mediator.Send(new CreatePostCommand(createPostDto));
        return CreatedAtAction(nameof(Get), new { createdPost });
    }
    
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] GetPostByIdDto getPostByIdDto)
    {
        return Ok(await _mediator.Send(new GetPostByIdQuery(getPostByIdDto)));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetAll([FromQuery] GetAllPostsDto getAllPostsDto)
    {
        return Ok(await _mediator.Send(new GetAllPostsQuery(getAllPostsDto)));
    }
}