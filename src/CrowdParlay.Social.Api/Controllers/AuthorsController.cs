using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Features.Commands;
using CrowdParlay.Social.Application.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<AuthorDto> Get([FromRoute] Guid authorId) =>
        await _mediator.Send(new GetAuthorById((authorId)));

    [HttpDelete]
    public async Task Delete([FromRoute] Guid authorId) =>
        await _mediator.Send(new DeleteAuthor(authorId));
}