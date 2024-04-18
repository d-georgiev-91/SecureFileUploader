namespace SecureFileUploader.Data.Entities;

public class TimestampEntity
{
    /// <summary>
    /// Gets or sets the date and time when entity was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when entity was last updated.
    /// </summary>
    public DateTime UpdatedOn { get; set; }
}
