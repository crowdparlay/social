using CrowdParlay.Social.Application.DTOs.Author;
using CrowdParlay.Social.Application.Features.Authors.Commands;
using CrowdParlay.Social.Application.Features.Authors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<AuthorDto> Get([FromRoute] Guid authorId) =>
        await _mediator.Send(new GetAuthorByIdQuery(authorId));
    
    [HttpPost, FeatureGate("BackdoorEndpoints")]
    public async Task<AuthorDto> Create([FromBody] CreateAuthorCommand command) =>
        await _mediator.Send(command);

    [HttpDelete, FeatureGate("BackdoorEndpoints")]
    public async Task Delete([FromRoute] Guid authorId) =>
        await _mediator.Send(new DeleteAuthorCommand(authorId));
}