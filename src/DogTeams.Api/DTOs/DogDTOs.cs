using DogTeams.Api.Models;

namespace DogTeams.Api.DTOs;

/// <summary>Request payload for creating a new Dog.</summary>
/// <param name="Name">Dog's call name or registered name.</param>
/// <param name="DateOfBirth">Dog's date of birth.</param>
/// <param name="BreedId">Reference to Breed entity ID. Preferred over free-text Breed.</param>
/// <param name="Breed">Legacy free-text breed. Use BreedId for new records.</param>
/// <param name="NafaCrn">NAFA-assigned Competition Racing Number, if available.</param>
/// <param name="WithersHeightInches">Measured withers height in inches.</param>
/// <param name="JumpHeightInches">Calculated jump height in inches.</param>
/// <param name="MeasurementType">Classification of the height measurement.</param>
/// <param name="MeasurementDate">Date the measurement was taken.</param>
/// <param name="MeasurementExpiresAt">Expiry date for a Temporary measurement.</param>
public record CreateDogRequest(
    string Name,
    DateTime DateOfBirth,
    string? BreedId = null,
    string? Breed = null,
    string? NafaCrn = null,
    decimal? WithersHeightInches = null,
    decimal? JumpHeightInches = null,
    MeasurementType MeasurementType = MeasurementType.None,
    DateTime? MeasurementDate = null,
    DateTime? MeasurementExpiresAt = null
);

/// <summary>Request payload for updating an existing Dog.</summary>
/// <param name="Name">Dog's call name or registered name.</param>
/// <param name="DateOfBirth">Dog's date of birth.</param>
/// <param name="BreedId">Reference to Breed entity ID.</param>
/// <param name="Breed">Legacy free-text breed.</param>
/// <param name="NafaCrn">NAFA Competition Racing Number.</param>
/// <param name="WithersHeightInches">Measured withers height in inches.</param>
/// <param name="JumpHeightInches">Calculated jump height in inches.</param>
/// <param name="MeasurementType">Classification of the height measurement.</param>
/// <param name="MeasurementDate">Date the measurement was taken.</param>
/// <param name="MeasurementExpiresAt">Expiry date for a Temporary measurement.</param>
public record UpdateDogRequest(
    string Name,
    DateTime DateOfBirth,
    string? BreedId = null,
    string? Breed = null,
    string? NafaCrn = null,
    decimal? WithersHeightInches = null,
    decimal? JumpHeightInches = null,
    MeasurementType MeasurementType = MeasurementType.None,
    DateTime? MeasurementDate = null,
    DateTime? MeasurementExpiresAt = null
);

/// <summary>Response payload representing a Dog.</summary>
/// <param name="Id">Internal unique identifier.</param>
/// <param name="OwnerId">Owner's ID.</param>
/// <param name="TeamId">Denormalized team ID.</param>
/// <param name="Name">Dog's call name or registered name.</param>
/// <param name="Breed">Legacy free-text breed (may be empty).</param>
/// <param name="BreedId">Reference to Breed entity ID.</param>
/// <param name="DateOfBirth">Dog's date of birth.</param>
/// <param name="NafaCrn">NAFA Competition Racing Number.</param>
/// <param name="WithersHeightInches">Measured withers height in inches.</param>
/// <param name="JumpHeightInches">Calculated jump height in inches.</param>
/// <param name="MeasurementType">Classification of the height measurement.</param>
/// <param name="MeasurementDate">Date the measurement was taken.</param>
/// <param name="MeasurementExpiresAt">Expiry date for Temporary measurements.</param>
/// <param name="LifetimePoints">Materialized lifetime regular-class points.</param>
/// <param name="MultibreedLifetimePoints">Materialized lifetime multibreed-class points.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
public record DogResponse(
    string Id,
    string OwnerId,
    string TeamId,
    string Name,
#pragma warning disable CS0618
    string Breed,
#pragma warning restore CS0618
    string? BreedId,
    DateTime DateOfBirth,
    string? NafaCrn,
    decimal? WithersHeightInches,
    decimal? JumpHeightInches,
    MeasurementType MeasurementType,
    DateTime? MeasurementDate,
    DateTime? MeasurementExpiresAt,
    int LifetimePoints,
    int MultibreedLifetimePoints,
    DateTime CreatedAt
);
