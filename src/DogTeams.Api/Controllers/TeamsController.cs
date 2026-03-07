using Microsoft.AspNetCore.Mvc;
using DogTeams.Api.Data.Repositories;
using DogTeams.Api.DTOs;
using DogTeams.Api.Models;

namespace DogTeams.Api.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly ITeamRepository _repository;

    public TeamsController(ITeamRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeamResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var teams = await _repository.GetAllAsync();
        var responses = teams.Select(t => new TeamResponse(t.Id, t.Name, t.Description, t.CreatedAt));
        return Ok(responses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        var response = new TeamResponse(team.Id, team.Name, team.Description, team.CreatedAt);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Team name is required.");

        var team = new Team
        {
            Name = request.Name,
            Description = request.Description
        };

        var created = await _repository.CreateAsync(team);
        var response = new TeamResponse(created.Id, created.Name, created.Description, created.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        team.Name = request.Name;
        team.Description = request.Description;

        var updated = await _repository.UpdateAsync(team);
        var response = new TeamResponse(updated.Id, updated.Name, updated.Description, updated.CreatedAt);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team == null)
            return NotFound();

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
