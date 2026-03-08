using Microsoft.Azure.Cosmos;
using DogTeams.Api.Configuration;
using DogTeams.Api.Auth;
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
    /// Includes a timeout to prevent indefinite waits if Cosmos isn't responding.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting Cosmos DB schema initialization for database: {DatabaseName}", _options.DatabaseName);

            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                try
                {
                    // Create database if it doesn't exist
                    var databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(_options.DatabaseName, cancellationToken: cts.Token);
                    var database = databaseResponse.Database;
                    _logger.LogInformation("Database {DatabaseName} is ready", _options.DatabaseName);

                    // Create all required containers
                    await CreateContainerIfNotExistsAsync(database, _options.TeamsContainer, "/id", cts.Token);
                    await CreateContainerIfNotExistsAsync(database, _options.OwnersContainer, "/id", cts.Token);
                    await CreateContainerIfNotExistsAsync(database, _options.DogsContainer, "/id", cts.Token);
                    await CreateContainerIfNotExistsAsync(database, _options.BreedsContainer, "/id", cts.Token);
                    await CreateContainerIfNotExistsAsync(database, _options.ClubsContainer, "/id", cts.Token);
                    await CreateContainerIfNotExistsAsync(database, "identity", "/id", cts.Token);

                    _logger.LogInformation("All containers created successfully");

                    // Seed initial data
                    await SeedBreedsAsync(database, cts.Token);
                    await SeedUsersAsync(database, cts.Token);

                    _logger.LogInformation("Cosmos DB schema initialization completed successfully");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogError("Cosmos DB initialization timed out after 30 seconds - ensure Cosmos Emulator/service is accessible and responsive");
                    throw new TimeoutException("Cosmos DB initialization timed out after 30 seconds");
                }
            }
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
    private async Task CreateContainerIfNotExistsAsync(Database database, string containerName, string partitionKeyPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerResponse = await database.CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    Id = containerName,
                    PartitionKeyPath = partitionKeyPath
                },
                cancellationToken: cancellationToken);
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
    private async Task SeedBreedsAsync(Database database, CancellationToken cancellationToken = default)
    {
        var container = database.GetContainer(_options.BreedsContainer);

        try
        {
            // Check if container already has data using a simple query
            var query = new QueryDefinition("SELECT TOP 1 c.id FROM c");
            var queryIterator = container.GetItemQueryIterator<dynamic>(query);

            if (queryIterator.HasMoreResults)
            {
                var resultSet = await queryIterator.ReadNextAsync(cancellationToken);
                if (resultSet.Any())
                {
                    _logger.LogInformation("Breeds container already populated");
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
                    await container.CreateItemAsync(breed, new PartitionKey(breed.Id), cancellationToken: cancellationToken);
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

    /// <summary>
    /// Seeds the Identity container with test users if empty.
    /// </summary>
    private async Task SeedUsersAsync(Database database, CancellationToken cancellationToken = default)
    {
        var container = database.GetContainer("identity");

        try
        {
            // Check if container already has data using a simple query
            var query = new QueryDefinition("SELECT TOP 1 c.id FROM c");
            var queryIterator = container.GetItemQueryIterator<dynamic>(query);

            if (queryIterator.HasMoreResults)
            {
                var resultSet = await queryIterator.ReadNextAsync(cancellationToken);
                if (resultSet.Any())
                {
                    _logger.LogInformation("Identity container already populated");
                    return;
                }
            }

            // Container is empty, seed test users
            var users = UserSeedData.GetSeedUsers();
            _logger.LogInformation("Seeding {UserCount} test users into Identity container", users.Count);

            foreach (var user in users)
            {
                try
                {
                    await container.CreateItemAsync(user, new PartitionKey(user.Id), cancellationToken: cancellationToken);
                    _logger.LogInformation("Seeded user: {Email} with ID: {UserId}", user.Email, user.Id);
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // User already exists, skip
                    _logger.LogDebug("User {Email} already exists, skipping", user.Email);
                }
            }

            _logger.LogInformation("Users seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding Identity container");
            throw;
        }
    }
}
