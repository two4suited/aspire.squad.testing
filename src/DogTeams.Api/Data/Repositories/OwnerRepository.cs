using Microsoft.Azure.Cosmos;
using DogTeams.Api.Models;
using DogTeams.Api.Caching;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Owner CRUD operations using Cosmos DB with Redis caching.
/// Uses team ID as partition key for single-partition queries.
/// </summary>
public class OwnerRepository : IOwnerRepository
{
    private readonly CosmosDbContext _context;
    private readonly IRedisCacheService _cache;
    private const string CacheKeyPrefix = "owner:";
    private const string TeamOwnersKeyPrefix = "team:owners:";
    private const int CacheTtlMinutes = 5;

    public OwnerRepository(CosmosDbContext context, IRedisCacheService cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<IEnumerable<Owner>> GetAllAsync()
    {
        var container = _context.OwnersContainer;
        var owners = new List<Owner>();

        var queryDefinition = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<Owner>(queryDefinition);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            owners.AddRange(response);
        }

        return owners;
    }

    public async Task<Owner?> GetByIdAsync(string id)
    {
        var container = _context.OwnersContainer;

        try
        {
            var response = await container.ReadItemAsync<Owner>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Owner>> GetByTeamIdAsync(string teamId)
    {
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var cacheKey = $"{TeamOwnersKeyPrefix}{teamId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var container = _context.OwnersContainer;
            var owners = new List<Owner>();

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.teamId = @teamId")
                .WithParameter("@teamId", teamId);

            var iterator = container.GetItemQueryIterator<Owner>(queryDefinition, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(teamId) });

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                owners.AddRange(response);
            }

            return (IEnumerable<Owner>)owners;
        }, TimeSpan.FromMinutes(CacheTtlMinutes)) ?? Enumerable.Empty<Owner>();
    }

    public async Task<Owner?> GetByIdAndTeamAsync(string id, string teamId)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Owner ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var cacheKey = $"{CacheKeyPrefix}{id}:team:{teamId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var container = _context.OwnersContainer;

            try
            {
                var response = await container.ReadItemAsync<Owner>(id, new PartitionKey(teamId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }, TimeSpan.FromMinutes(CacheTtlMinutes));
    }

    public async Task<Owner> CreateAsync(Owner entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.OwnersContainer;
        var response = await container.CreateItemAsync(entity, new PartitionKey(entity.TeamId));
        
        // Invalidate team owners cache
        await _cache.RemoveAsync($"{TeamOwnersKeyPrefix}{entity.TeamId}");
        
        return response.Resource;
    }

    public async Task<Owner> UpdateAsync(Owner entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.OwnersContainer;
        var response = await container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.TeamId));
        
        // Invalidate caches
        await _cache.RemoveAsync($"{CacheKeyPrefix}{entity.Id}:team:{entity.TeamId}");
        await _cache.RemoveAsync($"{TeamOwnersKeyPrefix}{entity.TeamId}");
        
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID cannot be null or empty.", nameof(id));

        var container = _context.OwnersContainer;
        await container.DeleteItemAsync<Owner>(id, new PartitionKey(id));
        
        // Note: We can't fully invalidate team owners cache without knowing teamId
        // This would require a separate operation or storing teamId in the deletion
    }
}
