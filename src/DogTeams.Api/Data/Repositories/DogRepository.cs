using Microsoft.Azure.Cosmos;
using DogTeams.Api.Models;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Dog CRUD operations using Cosmos DB.
/// Uses team ID as partition key for single-partition queries.
/// </summary>
public class DogRepository : IDogRepository
{
    private readonly CosmosDbContext _context;

    public DogRepository(CosmosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Dog>> GetAllAsync()
    {
        var container = _context.DogsContainer;
        var dogs = new List<Dog>();

        var queryDefinition = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<Dog>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            dogs.AddRange(response);
        }

        return dogs;
    }

    public async Task<Dog?> GetByIdAsync(string id)
    {
        var container = _context.DogsContainer;

        try
        {
            var response = await container.ReadItemAsync<Dog>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Dog>> GetByTeamIdAsync(string teamId)
    {
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var container = _context.DogsContainer;
        var dogs = new List<Dog>();

        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.teamId = @teamId")
            .WithParameter("@teamId", teamId);

        var iterator = container.GetItemQueryIterator<Dog>(queryDefinition, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(teamId) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            dogs.AddRange(response);
        }

        return dogs;
    }

    public async Task<IEnumerable<Dog>> GetByOwnerIdAsync(string ownerId, string teamId)
    {
        if (string.IsNullOrEmpty(ownerId))
            throw new ArgumentException("Owner ID cannot be null or empty.", nameof(ownerId));
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var container = _context.DogsContainer;
        var dogs = new List<Dog>();

        var queryDefinition = new QueryDefinition(
            "SELECT * FROM c WHERE c.ownerId = @ownerId AND c.teamId = @teamId")
            .WithParameter("@ownerId", ownerId)
            .WithParameter("@teamId", teamId);

        var iterator = container.GetItemQueryIterator<Dog>(queryDefinition, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(teamId) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            dogs.AddRange(response);
        }

        return dogs;
    }

    public async Task<Dog?> GetByIdAndTeamAsync(string id, string teamId)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Dog ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var container = _context.DogsContainer;

        try
        {
            var response = await container.ReadItemAsync<Dog>(id, new PartitionKey(teamId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Dog> CreateAsync(Dog entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.DogsContainer;
        var response = await container.CreateItemAsync(entity, new PartitionKey(entity.TeamId));
        return response.Resource;
    }

    public async Task<Dog> UpdateAsync(Dog entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.DogsContainer;
        var response = await container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.TeamId));
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        var container = _context.DogsContainer;
        await container.DeleteItemAsync<Dog>(id, new PartitionKey(id));
    }
}
