namespace DogTeams.Api.DTOs;

public record CreateOwnerRequest(
    string UserId,
    string Name,
    string Email
);

public record UpdateOwnerRequest(
    string Name,
    string Email
);

public record OwnerResponse(
    string Id,
    string TeamId,
    string UserId,
    string Name,
    string Email,
    DateTime CreatedAt
);
