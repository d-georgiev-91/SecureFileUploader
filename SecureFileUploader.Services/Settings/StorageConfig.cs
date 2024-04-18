namespace SecureFileUploader.Services.Settings;

/// <summary>
/// Configuration settings for file storage
/// </summary>
public class StorageConfig
{
    /// <summary>
    /// Gets or sets the file base directory
    /// </summary>
    /// <value>
    /// The directory path.
    /// </value>
    public required string Directory { get; set; }
}