using SecureFileUploader.Data.Entities;

namespace SecureFileUploader.Data.Repositories;

/// <summary>
/// Concrete repository for managing users.
/// </summary>
public interface IUserRepository: IRepository<User>
{
    /// <summary>
    /// Retrieves a user by their username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetUserByUsernameAsync(string username);
}