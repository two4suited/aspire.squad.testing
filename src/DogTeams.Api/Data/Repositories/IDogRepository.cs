using DogTeams.Api.Models;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository interface for Dog CRUD operations scoped to a team.
/// </summary>
public interface IDogRepository : IRepository<Dog>
{
    /// <summary>Retrieves all dogs for a specific team.</summary>
    Task<IEnumerable<Dog>> GetByTeamIdAsync(string teamId);

    /// <summary>Retrieves all dogs owned by a specific owner.</summary>
    Task<IEnumerable<Dog>> GetByOwnerIdAsync(string ownerId, string teamId);

    /// <summary>Retrieves a specific dog within a team context.</summary>
    Task<Dog?> GetByIdAndTeamAsync(string id, string teamId);
}
