namespace SecureFileUploader.Data.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : TimestampEntity
{
    /// <summary>
    /// Gets or sets the identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the hashed password for the user.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets files for the user.
    /// </summary>
    public ICollection<File> Files { get; } = new HashSet<File>();
}