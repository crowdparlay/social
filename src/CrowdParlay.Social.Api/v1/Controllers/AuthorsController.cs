using System.Net;
using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class AuthorsController(IAuthorRepository authors) : ControllerBase
{
    /// <summary>
    /// Returns author with the specified ID.
    /// </summary>
    [HttpGet("{authorId:guid}")]
    [ProducesResponseType(typeof(AuthorDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task<AuthorDto> GetAuthorById([FromRoute] Guid authorId) =>
        await authors.GetByIdAsync(authorId);
}