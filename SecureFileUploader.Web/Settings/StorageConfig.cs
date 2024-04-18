namespace SecureFileUploader.Web.Settings;

/// <summary>
/// Configuration settings for file storage, including limits on file size,
/// supported file extensions, and MIME types.
/// </summary>
public class StorageConfig
{
    /// <summary>
    /// Gets or sets the maximum file size in bytes that can be uploaded.
    /// </summary>
    /// <value>
    /// The maximum size of a file in bytes.
    /// </value>
    public int MaxFileSizeInBytes { get; set; }

    /// <summary>
    /// Gets or sets the list of supported file extensions for uploads.
    /// </summary>
    /// <value>
    /// An array of strings, where each string is a file extension that is allowed for upload.
    /// </value>
    public required string[] SupportedFileExtensions { get; set; }

    /// <summary>
    /// Gets or sets the list of supported MIME types for uploads.
    /// </summary>
    /// <value>
    /// An array of strings, where each string is a MIME type that is allowed for file uploads.
    /// </value>
    public required string[] SupportedMimeTypes { get; set; }
}
