using DogTeams.Api.Models;

namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Repository interface for Team CRUD operations.
/// </summary>
public interface ITeamRepository : IRepository<Team>
{
    // No additional methods needed beyond base interface for now
}
