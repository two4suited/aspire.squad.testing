using Microsoft.Azure.Cosmos;
using DogTeams.Api.Models;
using DogTeams.Api.Caching;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Team CRUD operations using Cosmos DB with Redis caching.
/// Uses team ID as partition key for single-partition queries.
/// </summary>
public class TeamRepository : ITeamRepository
{
    private readonly CosmosDbContext _context;
    private readonly IRedisCacheService _cache;
    private const string CacheKeyPrefix = "team:";
    private const int CacheTtlMinutes = 10;

    public TeamRepository(CosmosDbContext context, IRedisCacheService cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        var cacheKey = $"{CacheKeyPrefix}{id}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
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
        }, TimeSpan.FromMinutes(CacheTtlMinutes));
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
        
        // Invalidate cache
        var cacheKey = $"{CacheKeyPrefix}{entity.Id}";
        await _cache.RemoveAsync(cacheKey);
        
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        var container = _context.TeamsContainer;
        await container.DeleteItemAsync<Team>(id, new PartitionKey(id));
        
        // Invalidate cache
        var cacheKey = $"{CacheKeyPrefix}{id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
