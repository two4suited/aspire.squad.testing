namespace DogTeams.Api.Data.Repositories;

/// <summary>
/// Generic base repository interface for CRUD operations on Cosmos DB documents.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Retrieves all documents from the container.</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Retrieves a document by its ID.</summary>
    Task<T?> GetByIdAsync(string id);

    /// <summary>Creates a new document in the container.</summary>
    Task<T> CreateAsync(T entity);

    /// <summary>Updates an existing document in the container.</summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>Deletes a document from the container by its ID.</summary>
    Task DeleteAsync(string id);
}
