//-----------------------------------------------------------------------
// <copyright file="BackupModels.cs" company="Luppes Consulting, Inc.">
// Copyright 2026, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Models used by the scheduled backup export process
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data.Models;

/// <summary>
/// Represents a snapshot of the current joke data, used to detect changes since the last backup export.
/// </summary>
public class JokeChangeSnapshot
{
    /// <summary>
    /// Gets or sets the most recent change date/time found in the joke table.
    /// </summary>
    /// <value>A <see cref="DateTime"/> value, or <see langword="null"/> when there are no jokes.</value>
    public DateTime? MaxChangeDateTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of active jokes.
    /// </summary>
    /// <value>An integer count of active jokes.</value>
    public int JokeCount { get; set; }

    /// <summary>
    /// Gets or sets the number of distinct categories in use.
    /// </summary>
    /// <value>An integer count of categories.</value>
    public int CategoryCount { get; set; }
}

/// <summary>
/// Represents the complete set of data included in a backup export.
/// </summary>
public class BackupData
{
    /// <summary>
    /// Gets or sets the exported jokes.
    /// </summary>
    /// <value>A list of <see cref="Joke"/> records.</value>
    public List<Joke> Jokes { get; set; } = [];

    /// <summary>
    /// Gets or sets the exported categories.
    /// </summary>
    /// <value>A list of <see cref="JokeCategory"/> records.</value>
    public List<JokeCategory> Categories { get; set; } = [];

    /// <summary>
    /// Gets or sets the exported ratings.
    /// </summary>
    /// <value>A list of <see cref="JokeRating"/> records.</value>
    public List<JokeRating> Ratings { get; set; } = [];
}

/// <summary>
/// Represents an audit record describing the outcome of a backup export run.
/// </summary>
[Table("BackupMetadata", Schema = "Dad")]
public class BackupMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier of the backup metadata record.
    /// </summary>
    /// <value>An integer key.</value>
    [Key]
    public int BackupMetadataId { get; set; }

    /// <summary>
    /// Gets or sets the type of export, for example "Weekly" or "Manual".
    /// </summary>
    /// <value>A string describing the export type.</value>
    [StringLength(50)]
    public string ExportType { get; set; } = BackupConstants.WeeklyExportType;

    /// <summary>
    /// Gets or sets the date and time the export ran.
    /// </summary>
    /// <value>A UTC <see cref="DateTime"/> value.</value>
    public DateTime LastExportedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the maximum joke change date/time included in the export.
    /// </summary>
    /// <value>A UTC <see cref="DateTime"/> value, or <see langword="null"/> when the export was skipped or failed.</value>
    public DateTime? LastExportedMaxChangeDateTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of jokes included in the export.
    /// </summary>
    /// <value>An integer count of exported jokes.</value>
    public int LastExportedJokeCount { get; set; }

    /// <summary>
    /// Gets or sets the URI of the backup blob that was created.
    /// </summary>
    /// <value>A string containing the blob URI, or <see langword="null"/> when no blob was written.</value>
    [StringLength(2048)]
    public string? BackupBlobUri { get; set; }

    /// <summary>
    /// Gets or sets the SHA256 checksum (hex string) of the uncompressed backup content.
    /// </summary>
    /// <value>A hexadecimal string, or <see langword="null"/> when no blob was written.</value>
    [StringLength(256)]
    public string? Checksum { get; set; }

    /// <summary>
    /// Gets or sets the status of the export.
    /// </summary>
    /// <value>One of "Success", "Skipped", or "Failed".</value>
    [StringLength(50)]
    public string Status { get; set; } = BackupConstants.StatusSuccess;

    /// <summary>
    /// Gets or sets an error message or the reason an export was skipped.
    /// </summary>
    /// <value>A string containing the message, or <see langword="null"/>.</value>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the date and time the record was created.
    /// </summary>
    /// <value>A UTC <see cref="DateTime"/> value.</value>
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>
/// Represents the minimum information needed about a stored backup blob to support retention rotation.
/// </summary>
/// <param name="Name">The full name (path) of the blob.</param>
/// <param name="CreatedOn">The date and time the blob was created.</param>
public record BackupBlobInfo(string Name, DateTimeOffset? CreatedOn);

/// <summary>
/// Constants shared by the backup export process.
/// </summary>
public static class BackupConstants
{
    /// <summary>The export type used by the scheduled weekly backup job.</summary>
    public const string WeeklyExportType = "Weekly";

    /// <summary>The export type used by manually triggered backups.</summary>
    public const string ManualExportType = "Manual";

    /// <summary>Status recorded when a backup was created successfully.</summary>
    public const string StatusSuccess = "Success";

    /// <summary>Status recorded when a backup was intentionally skipped.</summary>
    public const string StatusSkipped = "Skipped";

    /// <summary>Status recorded when a backup failed.</summary>
    public const string StatusFailed = "Failed";

    /// <summary>The prefix used for all backup blob file names.</summary>
    public const string BlobNamePrefix = "dadabase-backup-";

    /// <summary>The default blob container that holds the backups.</summary>
    public const string DefaultContainerName = "backup-data";

    /// <summary>The number of backups retained by the rotation process.</summary>
    public const int MaxBackupsToKeep = 10;
}
