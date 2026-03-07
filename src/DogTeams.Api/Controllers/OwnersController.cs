using Microsoft.AspNetCore.Mvc;
using DogTeams.Api.Data.Repositories;
using DogTeams.Api.DTOs;
using DogTeams.Api.Models;

namespace DogTeams.Api.Controllers;

[ApiController]
[Route("api/teams/{teamId}/owners")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerRepository _repository;

    public OwnersController(IOwnerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OwnerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string teamId)
    {
        var owners = await _repository.GetByTeamIdAsync(teamId);
        var responses = owners.Select(o => new OwnerResponse(o.Id, o.TeamId, o.UserId, o.Name, o.Email, o.CreatedAt));
        return Ok(responses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string teamId, string id)
    {
        var owner = await _repository.GetByIdAndTeamAsync(id, teamId);
        if (owner == null)
            return NotFound();

        var response = new OwnerResponse(owner.Id, owner.TeamId, owner.UserId, owner.Name, owner.Email, owner.CreatedAt);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OwnerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(string teamId, [FromBody] CreateOwnerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Owner name is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Owner email is required.");

        var owner = new Owner
        {
            TeamId = teamId,
            UserId = request.UserId,
            Name = request.Name,
            Email = request.Email
        };

        var created = await _repository.CreateAsync(owner);
        var response = new OwnerResponse(created.Id, created.TeamId, created.UserId, created.Name, created.Email, created.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { teamId, id = created.Id }, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(OwnerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string teamId, string id, [FromBody] UpdateOwnerRequest request)
    {
        var owner = await _repository.GetByIdAndTeamAsync(id, teamId);
        if (owner == null)
            return NotFound();

        owner.Name = request.Name;
        owner.Email = request.Email;

        var updated = await _repository.UpdateAsync(owner);
        var response = new OwnerResponse(updated.Id, updated.TeamId, updated.UserId, updated.Name, updated.Email, updated.CreatedAt);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string teamId, string id)
    {
        var owner = await _repository.GetByIdAndTeamAsync(id, teamId);
        if (owner == null)
            return NotFound();

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
