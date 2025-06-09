using System.Net.Mime;
using CrowdParlay.Social.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Social.Api.v1.Controllers;

[ApiController, ApiRoute("[controller]")]
public class LookupController : ControllerBase
{
    /// <summary>
    /// Retrieves all available reactions.
    /// </summary>
    /// <returns>The complete set of allowed reaction values.</returns>
    [HttpGet("reactions"), Produces(MediaTypeNames.Application.Json)]
    public IReadOnlySet<string> GetAvailableReactions() => Reaction.AllowedValues;
}