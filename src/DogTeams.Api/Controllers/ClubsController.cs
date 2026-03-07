using DogTeams.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DogTeams.Api.Controllers;

/// <summary>
/// Manages NAFA-registered Clubs. A Club fields one or more racing Teams.
/// Cosmos container: "clubs", partition key: /clubId.
/// </summary>
[ApiController]
[Route("clubs")]
public class ClubsController : ControllerBase
{
    /// <summary>Create a new Club.</summary>
    /// <param name="request">Club creation payload.</param>
    /// <returns>The created Club.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClubResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult CreateClub([FromBody] CreateClubRequest request)
    {
        // TODO: implement with CosmosDB — insert into "clubs" container
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>Get a Club by its ID.</summary>
    /// <param name="clubId">The Club's unique identifier.</param>
    /// <returns>The matching Club.</returns>
    [HttpGet("{clubId}")]
    [ProducesResponseType(typeof(ClubResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult GetClub(string clubId)
    {
        // TODO: implement with CosmosDB — point read by clubId (partition key)
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>List all Clubs.</summary>
    /// <returns>Collection of all Clubs.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClubResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult GetClubs()
    {
        // TODO: implement with CosmosDB — cross-partition query (fan-out, use sparingly)
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>Update an existing Club.</summary>
    /// <param name="clubId">The Club's unique identifier.</param>
    /// <param name="request">Club update payload.</param>
    /// <returns>The updated Club.</returns>
    [HttpPut("{clubId}")]
    [ProducesResponseType(typeof(ClubResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult UpdateClub(string clubId, [FromBody] UpdateClubRequest request)
    {
        // TODO: implement with CosmosDB — replace document by clubId
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>Delete a Club by its ID.</summary>
    /// <param name="clubId">The Club's unique identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{clubId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult DeleteClub(string clubId)
    {
        // TODO: implement with CosmosDB — delete document by clubId
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
