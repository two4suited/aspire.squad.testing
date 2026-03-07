namespace DogTeams.Api.Configuration;

public class CosmosDbOptions
{
    public string DatabaseName { get; set; } = "DogTeamsDb";
    public string TeamsContainer { get; set; } = "Teams";
    public string OwnersContainer { get; set; } = "Owners";
    public string DogsContainer { get; set; } = "Dogs";
}
