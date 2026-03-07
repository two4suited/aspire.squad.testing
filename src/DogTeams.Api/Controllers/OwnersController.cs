using Microsoft.AspNetCore.Mvc;
using DogTeams.Api.DTOs;

namespace DogTeams.Api.Controllers;

[ApiController]
[Route("teams/{teamId}/owners")]
public class OwnersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OwnerResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll(string teamId)
    {
        // TODO: implement with CosmosDB
        return Ok(Array.Empty<OwnerResponse>());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(string teamId, string id)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(OwnerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(string teamId, [FromBody] CreateOwnerRequest request)
    {
        // TODO: implement with CosmosDB
        return Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(string teamId, string id, [FromBody] UpdateOwnerRequest request)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(string teamId, string id)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }
}
