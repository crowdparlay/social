using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Features.Authors.Commands;
using CrowdParlay.Social.Application.Features.Authors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<AuthorDto> Get([FromRoute] Guid authorId) =>
        await _mediator.Send(new GetAuthorByIdQuery((authorId)));

    [HttpDelete]
    public async Task Delete([FromRoute] Guid authorId) =>
        await _mediator.Send(new DeleteAuthorCommand(authorId));
}