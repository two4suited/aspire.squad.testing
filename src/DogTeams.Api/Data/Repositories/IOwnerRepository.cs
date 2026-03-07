using DogTeams.Api.Models;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository interface for Owner CRUD operations scoped to a team.
/// </summary>
public interface IOwnerRepository : IRepository<Owner>
{
    /// <summary>Retrieves all owners for a specific team.</summary>
    Task<IEnumerable<Owner>> GetByTeamIdAsync(string teamId);

    /// <summary>Retrieves a specific owner within a team context.</summary>
    Task<Owner?> GetByIdAndTeamAsync(string id, string teamId);
}
