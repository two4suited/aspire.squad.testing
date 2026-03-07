namespace DogTeams.Api.DTOs;

public record CreateDogRequest(
    string Name,
    string Breed,
    DateTime DateOfBirth
);

public record UpdateDogRequest(
    string Name,
    string Breed,
    DateTime DateOfBirth
);

public record DogResponse(
    string Id,
    string OwnerId,
    string TeamId,
    string Name,
    string Breed,
    DateTime DateOfBirth,
    DateTime CreatedAt
);
