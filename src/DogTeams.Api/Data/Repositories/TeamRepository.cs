using Microsoft.Azure.Cosmos;
using DogTeams.Api.Models;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Team CRUD operations using Cosmos DB.
/// Uses team ID as partition key for single-partition queries.
/// </summary>
public class TeamRepository : ITeamRepository
{
    private readonly CosmosDbContext _context;

    public TeamRepository(CosmosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Team>> GetAllAsync()
    {
        var container = _context.TeamsContainer;
        var teams = new List<Team>();

        var queryDefinition = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<Team>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            teams.AddRange(response);
        }

        return teams;
    }

    public async Task<Team?> GetByIdAsync(string id)
    {
        var container = _context.TeamsContainer;

        try
        {
            var response = await container.ReadItemAsync<Team>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Team> CreateAsync(Team entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.TeamsContainer;
        var response = await container.CreateItemAsync(entity, new PartitionKey(entity.Id));
        return response.Resource;
    }

    public async Task<Team> UpdateAsync(Team entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.TeamsContainer;
        var response = await container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.Id));
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        var container = _context.TeamsContainer;
        await container.DeleteItemAsync<Team>(id, new PartitionKey(id));
    }
}
