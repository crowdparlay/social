using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Features.Commands;
using CrowdParlay.Social.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]/[action]")]
public class AuthorController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateAuthorDto>> Create(CreateAuthorDto createAuthorDto)
    {
        var createdAuthor = await _mediator.Send(new CreateAuthorCommand(createAuthorDto));
        
        return CreatedAtAction(nameof(Get), new { createdAuthor }, createdAuthor);
    }

    [HttpGet]
    public async Task<ActionResult<CreateAuthorDto>> Get([FromQuery] GetAuthorByIdDto getAuthorByIdDto)
    {
        return Ok(await _mediator.Send(new GetAuthorByIdQuery(getAuthorByIdDto)));
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(DeleteAuthorByIdDto deleteAuthorByIdDto)
    {
        return Ok(await _mediator.Send(new DeleteAuthorCommand(deleteAuthorByIdDto)));
    }
}