namespace DogTeams.Api.DTOs;

public record CreateTeamRequest(
    string Name,
    string Description
);

public record UpdateTeamRequest(
    string Name,
    string Description
);

public record TeamResponse(
    string Id,
    string Name,
    string Description,
    DateTime CreatedAt
);
