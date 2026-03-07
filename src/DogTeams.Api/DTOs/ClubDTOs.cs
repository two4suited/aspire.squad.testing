namespace DogTeams.Api.DTOs;

/// <summary>Request payload for creating a new Club.</summary>
/// <param name="Name">Club display name.</param>
/// <param name="Region">Geographic region.</param>
/// <param name="HomeRegion">Home region for NAFA regional championship qualification.</param>
/// <param name="NafaClubNumber">Optional NAFA-assigned club number.</param>
public record CreateClubRequest(
    string Name,
    string Region,
    string HomeRegion,
    string? NafaClubNumber = null
);

/// <summary>Request payload for updating an existing Club.</summary>
/// <param name="Name">Club display name.</param>
/// <param name="Region">Geographic region.</param>
/// <param name="HomeRegion">Home region for NAFA regional championship qualification.</param>
/// <param name="NafaClubNumber">Optional NAFA-assigned club number.</param>
public record UpdateClubRequest(
    string Name,
    string Region,
    string HomeRegion,
    string? NafaClubNumber = null
);

/// <summary>Response payload representing a Club.</summary>
/// <param name="Id">Internal unique identifier.</param>
/// <param name="ClubId">Partition key value (mirrors Id).</param>
/// <param name="NafaClubNumber">NAFA-assigned club number, if available.</param>
/// <param name="Name">Club display name.</param>
/// <param name="Region">Geographic region.</param>
/// <param name="HomeRegion">Home region for regional championship.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="UpdatedAt">UTC last-updated timestamp.</param>
public record ClubResponse(
    string Id,
    string ClubId,
    string? NafaClubNumber,
    string Name,
    string Region,
    string HomeRegion,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
