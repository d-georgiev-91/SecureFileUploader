namespace SecureFileUploader.Services.Models;

/// <inheritdoc cref="FileBase" />
public class File : FileBase
{
    /// <summary>
    /// Gets or sets the Mime type of the file.
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content of the file.
    /// </summary>
    public required byte[] Bytes { get; set; }
}