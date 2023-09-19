using CrowdParlay.Social.Application.Abstractions;
using CrowdParlay.Social.Application.DTOs.Author;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorRepository _authors;

    public AuthorsController(IAuthorRepository authors) => _authors = authors;

    [HttpGet("{authorId}")]
    public async Task<AuthorDto> GetAuthorById([FromRoute] Guid authorId) =>
        await _authors.FindAsync(authorId);
}