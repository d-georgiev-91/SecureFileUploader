namespace SecureFileUploader.Data.Repositories;

/// <summary>
/// Defines a repository for managing file entities.
/// </summary>
public interface IFileRepository : IRepository<Entities.File>
{
    /// <summary>
    /// Retrieves all files associated with a given user.
    /// </summary>
    /// <param name="username">The username whose files are to be retrieved. This identifier is used to filter files belonging to a specific user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of <see cref="Entities.File"/> that belong to the specified user.</returns>
    Task<IEnumerable<Entities.File>> GetFilesByUsername(string username);

    /// <summary>
    /// Retrieves a file by ID and username.
    /// </summary>
    /// <param name="id">The ID of the file.</param>
    /// <param name="username">The username owner of the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file.</returns>
    Task<Entities.File?> GetByIdAndUsernameAsync(int id, string username);
}