using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorRepository _authors;

    public AuthorsController(IAuthorRepository authors) => _authors = authors;

    /// <summary>
    /// Returns author with the specified ID.
    /// </summary>
    [HttpGet("{authorId}")]
    public async Task<AuthorDto> GetAuthorById([FromRoute] Guid authorId) =>
        await _authors.GetByIdAsync(authorId);
}