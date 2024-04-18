using File = SecureFileUploader.Services.Models.File;

namespace SecureFileUploader.Services;

/// <summary>
/// Defines a contract for handling file-related operations.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Creates a file asynchronously based on the provided IFormFile and associated username.
    /// </summary>
    /// <param name="file">The file to be created and stored.</param>
    /// <param name="username">The username associated with the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created file.</returns>
    Task<Models.FileBase> CreateFileAsync(File file, string username);

    /// <summary>
    /// Updates an existing file based on the provided file data and file ID.
    /// </summary>
    /// <param name="file">The file to be created and stored.</param>
    /// <param name="fileId">The ID of the file to update.</param>
    /// <param name="username">The username associated with the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated file.</returns>
    Task<Models.FileBase> UpdateFileAsync(File file, int fileId, string username);

    /// <summary>
    /// Retrieves all files associated with a specified username.
    /// </summary>
    /// <param name="username">The username whose files are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of files.</returns>
    Task<IEnumerable<Models.FileBase>> GetFilesByUsernameAsync(string username);

    /// <summary>
    /// Retrieves a specific file by ID and username, including the file stream.
    /// </summary>
    /// <param name="id">The ID of the file to retrieve.</param>
    /// <param name="username">The username associated with the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains file .</returns>
    Task<File> GetFileByUsernameAndIdAsync(int id, string username);
}
