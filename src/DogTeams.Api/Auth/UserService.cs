using BCrypt.Net;

namespace DogTeams.Api.Auth;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TeamId { get; set; }
    public string? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Service for managing user authentication and persistence.
/// Currently uses in-memory storage; should be backed by Cosmos DB in production.
/// </summary>
public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User> CreateUserAsync(string email, string password, string name);
    Task<bool> VerifyPasswordAsync(string email, string password);
}

/// <summary>
/// In-memory user service for MVP. Replace with CosmosUserStore for production.
/// </summary>
public class InMemoryUserService : IUserService
{
    private static readonly Dictionary<string, User> Users = new();

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = Users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(string userId)
    {
        Users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(string email, string password, string name)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        var existingUser = Users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
            Name = name
        };

        Users[user.Id] = user;
        return Task.FromResult(user);
    }

    public async Task<bool> VerifyPasswordAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
            return false;

        return BCrypt.Net.BCrypt.EnhancedVerify(password, user.PasswordHash);
    }
}
