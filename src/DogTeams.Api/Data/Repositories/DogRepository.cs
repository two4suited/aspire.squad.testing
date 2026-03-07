using Microsoft.Azure.Cosmos;
using DogTeams.Api.Models;
using DogTeams.Api.Caching;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository implementation for Dog CRUD operations using Cosmos DB with Redis caching.
/// Uses team ID as partition key for single-partition queries.
/// </summary>
public class DogRepository : IDogRepository
{
    private readonly CosmosDbContext _context;
    private readonly IRedisCacheService _cache;
    private const string CacheKeyPrefix = "dog:";
    private const string TeamDogsKeyPrefix = "team:dogs:";
    private const string OwnerDogsKeyPrefix = "owner:dogs:";
    private const int CacheTtlMinutes = 5;

    public DogRepository(CosmosDbContext context, IRedisCacheService cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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

        var cacheKey = $"{TeamDogsKeyPrefix}{teamId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
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

            return (IEnumerable<Dog>)dogs;
        }, TimeSpan.FromMinutes(CacheTtlMinutes)) ?? Enumerable.Empty<Dog>();
    }

    public async Task<IEnumerable<Dog>> GetByOwnerIdAsync(string ownerId, string teamId)
    {
        if (string.IsNullOrEmpty(ownerId))
            throw new ArgumentException("Owner ID cannot be null or empty.", nameof(ownerId));
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var cacheKey = $"{OwnerDogsKeyPrefix}{ownerId}:team:{teamId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
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

            return (IEnumerable<Dog>)dogs;
        }, TimeSpan.FromMinutes(CacheTtlMinutes)) ?? Enumerable.Empty<Dog>();
    }

    public async Task<Dog?> GetByIdAndTeamAsync(string id, string teamId)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Dog ID cannot be null or empty.", nameof(id));
        if (string.IsNullOrEmpty(teamId))
            throw new ArgumentException("Team ID cannot be null or empty.", nameof(teamId));

        var cacheKey = $"{CacheKeyPrefix}{id}:team:{teamId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
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
        }, TimeSpan.FromMinutes(CacheTtlMinutes));
    }

    public async Task<Dog> CreateAsync(Dog entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.DogsContainer;
        var response = await container.CreateItemAsync(entity, new PartitionKey(entity.TeamId));
        
        // Invalidate caches
        await _cache.RemoveAsync($"{TeamDogsKeyPrefix}{entity.TeamId}");
        await _cache.RemoveAsync($"{OwnerDogsKeyPrefix}{entity.OwnerId}:team:{entity.TeamId}");
        
        return response.Resource;
    }

    public async Task<Dog> UpdateAsync(Dog entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var container = _context.DogsContainer;
        var response = await container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.TeamId));
        
        // Invalidate caches
        await _cache.RemoveAsync($"{CacheKeyPrefix}{entity.Id}:team:{entity.TeamId}");
        await _cache.RemoveAsync($"{TeamDogsKeyPrefix}{entity.TeamId}");
        await _cache.RemoveAsync($"{OwnerDogsKeyPrefix}{entity.OwnerId}:team:{entity.TeamId}");
        
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
