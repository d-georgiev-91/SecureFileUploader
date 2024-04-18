namespace SecureFileUploader.Services
{
    /// <inheritdoc cref="File" />
    public class FileSystem : IFileSystem
    {
        /// <inheritdoc cref="IFileSystem.ReadAllBytesAsync"/>
        public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) =>
            await File.ReadAllBytesAsync(path, cancellationToken);

        /// <inheritdoc cref="IFileSystem.WriteAllBytesAsync"/>
        public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);
        }

        /// <inheritdoc cref="IFileSystem.CreateDirectoryIfNotExists"/>
        public void CreateDirectoryIfNotExists(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }


        /// <inheritdoc cref="IFileSystem.Exists"/>
        public bool Exists(string? path) => File.Exists(path);

        /// <inheritdoc cref="IFileSystem.Delete"/>
        public void Delete(string path) => File.Delete(path);

        /// <inheritdoc cref="IFileSystem.DeleteIfExists"/>
        public void DeleteIfExists(string path)
        {
            if (Exists(path))
            {
                Delete(path);
            }
        }
    }
}
