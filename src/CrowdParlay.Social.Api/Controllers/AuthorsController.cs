using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Features.Commands;
using CrowdParlay.Social.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]/[action]/{authorId:guid}")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<AuthorDto> Get([FromRoute] Guid authorId) =>
        await _mediator.Send(new GetAuthorByIdQuery((authorId)));

    [HttpDelete]
    public async Task<Unit> Delete([FromRoute] Guid authorId) =>
        await _mediator.Send(new DeleteAuthorCommand(authorId));
}