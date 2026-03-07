using Microsoft.AspNetCore.Mvc;
using DogTeams.Api.DTOs;

namespace DogTeams.Api.Controllers;

[ApiController]
[Route("owners/{ownerId}/dogs")]
public class DogsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DogResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll(string ownerId)
    {
        // TODO: implement with CosmosDB
        return Ok(Array.Empty<DogResponse>());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(string ownerId, string id)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(DogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create(string ownerId, [FromBody] CreateDogRequest request)
    {
        // TODO: implement with CosmosDB
        return Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(string ownerId, string id, [FromBody] UpdateDogRequest request)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(string ownerId, string id)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }
}
