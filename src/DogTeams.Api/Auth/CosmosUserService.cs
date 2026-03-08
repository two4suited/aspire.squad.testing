using Microsoft.Azure.Cosmos;
using DogTeams.Api.Data;

namespace DogTeams.Api.Auth;

/// <summary>
/// Cosmos DB-backed user service for production use.
/// Stores users in the identity container with email as a secondary lookup.
/// </summary>
public class CosmosUserService : IUserService
{
    private readonly CosmosDbContext _context;
    private readonly ILogger<CosmosUserService> _logger;

    public CosmosUserService(CosmosDbContext context, ILogger<CosmosUserService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            var container = _context.IdentityContainer;
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE LOWER(c.Email) = LOWER(@email)")
                .WithParameter("@email", email);

            var iterator = container.GetItemQueryIterator<User>(query);
            
            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                return response.FirstOrDefault();
            }
            
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            var container = _context.IdentityContainer;
            var response = await container.ReadItemAsync<User>(userId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<User> CreateUserAsync(string email, string password, string name)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        // Check if user already exists
        var existingUser = await GetUserByEmailAsync(email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
            Name = name
        };

        try
        {
            var container = _context.IdentityContainer;
            var response = await container.CreateItemAsync(user, new PartitionKey(user.Id));
            _logger.LogInformation("Created user: {Email} with ID: {UserId}", email, user.Id);
            return response.Resource;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", email);
            throw;
        }
    }

    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
            return false;

        return BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash);
    }
}
