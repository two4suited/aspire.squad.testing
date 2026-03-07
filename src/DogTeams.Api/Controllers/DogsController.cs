using Microsoft.AspNetCore.Mvc;
using DogTeams.Api.Data.Repositories;
using DogTeams.Api.DTOs;
using DogTeams.Api.Models;

namespace DogTeams.Api.Controllers;

#pragma warning disable CS0618

[ApiController]
[Route("api/teams/{teamId}/owners/{ownerId}/dogs")]
public class DogsController : ControllerBase
{
    private readonly IDogRepository _repository;

    public DogsController(IDogRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string teamId, string ownerId)
    {
        var dogs = await _repository.GetByOwnerIdAsync(ownerId, teamId);
        var responses = dogs.Select(d => new DogResponse(
            d.Id, d.OwnerId, d.TeamId, d.Name, d.Breed, d.BreedId, d.DateOfBirth,
            d.NafaCrn, d.WithersHeightInches, d.JumpHeightInches, d.MeasurementType,
            d.MeasurementDate, d.MeasurementExpiresAt, d.LifetimePoints,
            d.MultibreedLifetimePoints, d.CreatedAt));
        return Ok(responses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string teamId, string ownerId, string id)
    {
        var dog = await _repository.GetByIdAndTeamAsync(id, teamId);
        if (dog == null || dog.OwnerId != ownerId)
            return NotFound();

        var response = new DogResponse(
            dog.Id, dog.OwnerId, dog.TeamId, dog.Name, dog.Breed, dog.BreedId, dog.DateOfBirth,
            dog.NafaCrn, dog.WithersHeightInches, dog.JumpHeightInches, dog.MeasurementType,
            dog.MeasurementDate, dog.MeasurementExpiresAt, dog.LifetimePoints,
            dog.MultibreedLifetimePoints, dog.CreatedAt);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(string teamId, string ownerId, [FromBody] CreateDogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Dog name is required.");

        var dog = new Dog
        {
            OwnerId = ownerId,
            TeamId = teamId,
            Name = request.Name,
            DateOfBirth = request.DateOfBirth,
            BreedId = request.BreedId,
            Breed = request.Breed ?? string.Empty,
            NafaCrn = request.NafaCrn,
            WithersHeightInches = request.WithersHeightInches,
            JumpHeightInches = request.JumpHeightInches,
            MeasurementType = request.MeasurementType,
            MeasurementDate = request.MeasurementDate,
            MeasurementExpiresAt = request.MeasurementExpiresAt
        };

        var created = await _repository.CreateAsync(dog);
        var response = new DogResponse(
            created.Id, created.OwnerId, created.TeamId, created.Name, created.Breed, created.BreedId,
            created.DateOfBirth, created.NafaCrn, created.WithersHeightInches, created.JumpHeightInches,
            created.MeasurementType, created.MeasurementDate, created.MeasurementExpiresAt,
            created.LifetimePoints, created.MultibreedLifetimePoints, created.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { teamId, ownerId, id = created.Id }, response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string teamId, string ownerId, string id, [FromBody] UpdateDogRequest request)
    {
        var dog = await _repository.GetByIdAndTeamAsync(id, teamId);
        if (dog == null || dog.OwnerId != ownerId)
            return NotFound();

        dog.Name = request.Name;
        dog.DateOfBirth = request.DateOfBirth;
        dog.BreedId = request.BreedId;
        dog.Breed = request.Breed ?? string.Empty;
        dog.NafaCrn = request.NafaCrn;
        dog.WithersHeightInches = request.WithersHeightInches;
        dog.JumpHeightInches = request.JumpHeightInches;
        dog.MeasurementType = request.MeasurementType;
        dog.MeasurementDate = request.MeasurementDate;
        dog.MeasurementExpiresAt = request.MeasurementExpiresAt;

        var updated = await _repository.UpdateAsync(dog);
        var response = new DogResponse(
            updated.Id, updated.OwnerId, updated.TeamId, updated.Name, updated.Breed, updated.BreedId,
            updated.DateOfBirth, updated.NafaCrn, updated.WithersHeightInches, updated.JumpHeightInches,
            updated.MeasurementType, updated.MeasurementDate, updated.MeasurementExpiresAt,
            updated.LifetimePoints, updated.MultibreedLifetimePoints, updated.CreatedAt);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string teamId, string ownerId, string id)
    {
        var dog = await _repository.GetByIdAndTeamAsync(id, teamId);
        if (dog == null || dog.OwnerId != ownerId)
            return NotFound();

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}

#pragma warning restore CS0618
