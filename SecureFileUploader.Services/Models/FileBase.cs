namespace SecureFileUploader.Services.Models;

/// <summary>
/// Represents a file in the system.
/// </summary>
public class FileBase
{
    /// <summary>
    /// Gets or sets the primary key for this file.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public required string FileName { get; set; }
}