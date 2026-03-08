using BCrypt.Net;

namespace DogTeams.Api.Auth;

/// <summary>
/// Seed data for test users. Contains test user(s) for manual testing.
/// Used during development to populate the auth system with known credentials.
/// </summary>
public static class UserSeedData
{
    /// <summary>
    /// Gets all seed users to be created during initialization.
    /// </summary>
    public static List<(string Email, string Password, string Name)> GetAll() => new()
    {
        ("test@example.com", "TestPassword123!", "Test User")
    };

    /// <summary>
    /// Creates User objects from seed data with hashed passwords.
    /// </summary>
    public static List<User> GetSeedUsers() => GetAll()
        .Select(u => new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = u.Email,
            Name = u.Name,
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(u.Password),
            CreatedAt = DateTime.UtcNow
        })
        .ToList();
}
