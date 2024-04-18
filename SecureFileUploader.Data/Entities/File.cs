namespace SecureFileUploader.Data.Entities;

/// <summary>
/// Represents a file in the system.
/// </summary>
public class File : TimestampEntity
{
    /// <summary>
    /// Gets or sets the primary key for this file.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type of the file.
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// Gets or sets where the file is physically stored.
    /// </summary>
    public required string StoragePath { get; set; }

    /// <summary>
    /// Gets or sets the foreign key of the user that this file belongs to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user that this file belongs to.
    /// </summary>
    public required User User { get; set; }
}
