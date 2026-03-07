namespace DogTeams.Api.DTOs;

/// <summary>Request payload for creating a new Breed entry.</summary>
/// <param name="Name">Breed name (e.g., "Border Collie").</param>
/// <param name="AkcCode">Optional AKC breed code.</param>
public record CreateBreedRequest(
    string Name,
    string? AkcCode = null
);

/// <summary>Request payload for updating an existing Breed entry.</summary>
/// <param name="Name">Breed name.</param>
/// <param name="AkcCode">Optional AKC breed code.</param>
/// <param name="IsActive">Whether this breed is active/selectable.</param>
public record UpdateBreedRequest(
    string Name,
    string? AkcCode = null,
    bool IsActive = true
);

/// <summary>Response payload representing a Breed.</summary>
/// <param name="Id">Internal unique identifier.</param>
/// <param name="Name">Breed name.</param>
/// <param name="AkcCode">AKC breed code, if available.</param>
/// <param name="IsActive">Whether this breed is active/selectable.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
public record BreedResponse(
    string Id,
    string Name,
    string? AkcCode,
    bool IsActive,
    DateTime CreatedAt
);
