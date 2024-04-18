namespace SecureFileUploader.Data.Repositories;

/// <summary>
/// Generic repository for handling entities of type T.
/// </summary>
/// <typeparam name="T">The type of entity this repository handles.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <returns>The entity found, or null if no entity is found.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all entities of type T.
    /// </summary>
    /// <returns>A collection of all entities of type T.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds a new entity to the context.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task AddAsync(T entity);

    /// <summary>
    /// Deletes an entity from the context.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Delete(T entity);

    /// <summary>
    /// Updates an entity in the context.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(T entity);
}