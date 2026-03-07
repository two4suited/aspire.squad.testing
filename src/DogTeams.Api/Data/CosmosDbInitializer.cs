using Microsoft.Azure.Cosmos;
using DogTeams.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DogTeams.Api.Data;

/// <summary>
/// Initializes the Cosmos DB schema and seeds data on application startup.
/// Creates database, containers, and initial data (e.g., seed breeds).
/// </summary>
public class CosmosDbInitializer
{
    private readonly CosmosClient _client;
    private readonly CosmosDbOptions _options;
    private readonly ILogger<CosmosDbInitializer> _logger;

    public CosmosDbInitializer(CosmosClient client, IOptions<CosmosDbOptions> options, ILogger<CosmosDbInitializer> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the Cosmos DB schema: creates database, containers, and seeds initial data.
    /// Safe to call multiple times (idempotent).
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting Cosmos DB schema initialization for database: {DatabaseName}", _options.DatabaseName);

            // Create database if it doesn't exist
            var databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(_options.DatabaseName);
            var database = databaseResponse.Database;
            _logger.LogInformation("Database {DatabaseName} is ready", _options.DatabaseName);

            // Create all required containers
            await CreateContainerIfNotExistsAsync(database, _options.TeamsContainer, "/id");
            await CreateContainerIfNotExistsAsync(database, _options.OwnersContainer, "/id");
            await CreateContainerIfNotExistsAsync(database, _options.DogsContainer, "/id");
            await CreateContainerIfNotExistsAsync(database, _options.BreedsContainer, "/id");
            await CreateContainerIfNotExistsAsync(database, _options.ClubsContainer, "/id");
            await CreateContainerIfNotExistsAsync(database, "identity", "/id");

            _logger.LogInformation("All containers created successfully");

            // Seed initial data
            await SeedBreedsAsync(database);

            _logger.LogInformation("Cosmos DB schema initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Cosmos DB schema initialization");
            throw;
        }
    }

    /// <summary>
    /// Creates a container if it doesn't already exist.
    /// </summary>
    private async Task CreateContainerIfNotExistsAsync(Database database, string containerName, string partitionKeyPath)
    {
        try
        {
            var containerResponse = await database.CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = containerName,
                    PartitionKeyPath = partitionKeyPath
                });
            _logger.LogInformation("Container {ContainerName} is ready", containerName);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogInformation("Container {ContainerName} already exists", containerName);
        }
    }

    /// <summary>
    /// Seeds the Breeds container with initial data if empty.
    /// </summary>
    private async Task SeedBreedsAsync(Database database)
    {
        var container = database.GetContainer(_options.BreedsContainer);

        try
        {
            // Check if container already has data
            var query = new QueryDefinition("SELECT COUNT(1) as count FROM c");
            var queryIterator = container.GetItemQueryIterator<BreedCountResult>(query);

            if (queryIterator.HasMoreResults)
            {
                var resultSet = await queryIterator.ReadNextAsync();
                var result = resultSet.FirstOrDefault();
                int count = result?.Count ?? 0;

                if (count > 0)
                {
                    _logger.LogInformation("Breeds container already populated with {Count} breeds", count);
                    return;
                }
            }

            // Container is empty, seed initial breeds
            var breeds = BreedSeedData.GetAll();
            _logger.LogInformation("Seeding {BreedCount} breeds into Breeds container", breeds.Count);

            foreach (var breed in breeds)
            {
                try
                {
                    await container.CreateItemAsync(breed, new PartitionKey(breed.Id));
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Breed already exists, skip
                    _logger.LogDebug("Breed {BreedName} already exists, skipping", breed.Name);
                }
            }

            _logger.LogInformation("Breeds seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding Breeds container");
            throw;
        }
    }

    /// <summary>Helper class for COUNT query results.</summary>
    private class BreedCountResult
    {
        public int Count { get; set; }
    }
}
