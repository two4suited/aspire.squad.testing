using Microsoft.Azure.Cosmos;
using DogTeams.Api.Configuration;
using DogTeams.Api.Models;
using Microsoft.Extensions.Options;

namespace DogTeams.Api.Data;

/// <summary>
/// Cosmos DB context that wraps the CosmosClient and provides typed container accessors.
/// </summary>
public class CosmosDbContext
{
    private readonly CosmosClient _client;
    private readonly CosmosDbOptions _options;
    private readonly string _databaseName;

    public CosmosDbContext(CosmosClient client, IOptions<CosmosDbOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _databaseName = _options.DatabaseName;
    }

    /// <summary>Gets the Teams container.</summary>
    public Container TeamsContainer =>
        _client.GetDatabase(_databaseName).GetContainer(_options.TeamsContainer);

    /// <summary>Gets the Owners container.</summary>
    public Container OwnersContainer =>
        _client.GetDatabase(_databaseName).GetContainer(_options.OwnersContainer);

    /// <summary>Gets the Dogs container.</summary>
    public Container DogsContainer =>
        _client.GetDatabase(_databaseName).GetContainer(_options.DogsContainer);

    /// <summary>Gets the Breeds container.</summary>
    public Container BreedsContainer =>
        _client.GetDatabase(_databaseName).GetContainer(_options.BreedsContainer);

    /// <summary>Gets the Clubs container.</summary>
    public Container ClubsContainer =>
        _client.GetDatabase(_databaseName).GetContainer(_options.ClubsContainer);

    /// <summary>Gets the Identity container for user authentication documents.</summary>
    public Container IdentityContainer =>
        _client.GetDatabase(_databaseName).GetContainer("identity");
}
