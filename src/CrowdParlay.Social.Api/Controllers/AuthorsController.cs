using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Features.Authors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{authorId}")]
    public async Task<AuthorDto> GetAuthorById([FromRoute] Guid authorId) =>
        await _mediator.Send(new GetAuthorByIdQuery(authorId));
}