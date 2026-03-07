namespace DogTeams.Api.Configuration;

/// <summary>Typed options for Cosmos DB container configuration.</summary>
public class CosmosDbOptions
{
    /// <summary>Cosmos DB database name.</summary>
    public string DatabaseName { get; set; } = "DogTeamsDb";

    /// <summary>Cosmos DB container name for Team documents.</summary>
    public string TeamsContainer { get; set; } = "Teams";

    /// <summary>Cosmos DB container name for Owner documents.</summary>
    public string OwnersContainer { get; set; } = "Owners";

    /// <summary>Cosmos DB container name for Dog documents.</summary>
    public string DogsContainer { get; set; } = "Dogs";

    /// <summary>Cosmos DB container name for Club documents.</summary>
    public string ClubsContainer { get; set; } = "clubs";

    /// <summary>Cosmos DB container name for Breed documents.</summary>
    public string BreedsContainer { get; set; } = "breeds";
}
