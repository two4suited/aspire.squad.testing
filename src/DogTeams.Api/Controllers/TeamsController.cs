using Microsoft.AspNetCore.Mvc;
using DogTeams.Api.DTOs;

namespace DogTeams.Api.Controllers;

[ApiController]
[Route("teams")]
public class TeamsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeamResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        // TODO: implement with CosmosDB
        return Ok(Array.Empty<TeamResponse>());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(string id)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreateTeamRequest request)
    {
        // TODO: implement with CosmosDB
        return Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(string id, [FromBody] UpdateTeamRequest request)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(string id)
    {
        // TODO: implement with CosmosDB
        return NotFound();
    }
}
