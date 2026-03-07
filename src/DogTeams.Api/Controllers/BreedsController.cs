using DogTeams.Api.Data;
using DogTeams.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DogTeams.Api.Controllers;

/// <summary>
/// Manages the AKC breed list. Seeded with common flyball breeds; editable by admins.
/// Cosmos container: "breeds", partition key: /id.
/// </summary>
[ApiController]
[Route("breeds")]
public class BreedsController : ControllerBase
{
    /// <summary>List all active Breeds. Falls back to seed data until Cosmos is wired.</summary>
    /// <returns>Collection of Breeds.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BreedResponse>), StatusCodes.Status200OK)]
    public IActionResult GetBreeds()
    {
        // TODO: replace with CosmosDB query once repository is implemented
        var breeds = BreedSeedData.GetAll()
            .Select(b => new BreedResponse(b.Id, b.Name, b.AkcCode, b.IsActive, b.CreatedAt));
        return Ok(breeds);
    }

    /// <summary>Get a Breed by its ID.</summary>
    /// <param name="breedId">The Breed's unique identifier.</param>
    /// <returns>The matching Breed.</returns>
    [HttpGet("{breedId}")]
    [ProducesResponseType(typeof(BreedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult GetBreed(string breedId)
    {
        // TODO: implement with CosmosDB — point read by breedId (partition key = id)
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>Create a new Breed entry (admin only).</summary>
    /// <param name="request">Breed creation payload.</param>
    /// <returns>The created Breed.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BreedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult CreateBreed([FromBody] CreateBreedRequest request)
    {
        // TODO: implement with CosmosDB — insert into "breeds" container
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>Update an existing Breed entry (admin only).</summary>
    /// <param name="breedId">The Breed's unique identifier.</param>
    /// <param name="request">Breed update payload.</param>
    /// <returns>The updated Breed.</returns>
    [HttpPut("{breedId}")]
    [ProducesResponseType(typeof(BreedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult UpdateBreed(string breedId, [FromBody] UpdateBreedRequest request)
    {
        // TODO: implement with CosmosDB — replace document by breedId
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    /// <summary>Delete a Breed entry (admin only).</summary>
    /// <param name="breedId">The Breed's unique identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{breedId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult DeleteBreed(string breedId)
    {
        // TODO: implement with CosmosDB — delete document by breedId
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
