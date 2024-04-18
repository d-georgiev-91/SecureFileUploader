namespace SecureFileUploader.Services;

/// <summary>
/// Wraps <see cref="File" />
/// </summary>
public interface IFileSystem
{
    /// <inheritdoc cref="File.ReadAllBytesAsync" />
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="File.WriteAllBytesAsync" />
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates all directories and subdirectories if given path doesn't exist.
    /// </summary>
    /// <param name="path">The directory to create.</param>
    void CreateDirectoryIfNotExists(string path);
    
    /// <inheritdoc cref="File.Exists" />
    bool Exists(string? path);

    /// <inheritdoc cref="File.Delete" />
    void Delete(string path);

    /// <summary>
    /// Deletes the file if it only exists on the file system.
    /// </summary>
    /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
    public void DeleteIfExists(string path);
}
