using SecureFileUploader.Data.Repositories;

namespace SecureFileUploader.Data;

/// <summary>
/// Defines the contract for a unit of work, which coordinates the commit of changes across multiple repositories, acting as a single commit point for the application.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the user repository that handles the lifecycle of user entities.
    /// </summary>
    IUserRepository UserRepository { get; }

    /// <summary>
    /// Gets the file repository that handles the lifecycle of file entities.
    /// </summary>
    IFileRepository FileRepository { get; }

    /// <summary>
    /// Completes all pending changes in this unit of work by persisting them to the underlying database.
    /// </summary>
    /// <returns>The number of entities that were written to the underlying database.</returns>
    Task<int> CommitAsync();
}