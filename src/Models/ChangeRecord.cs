using System;
using System.Collections.Generic;

namespace AgenticRevit.Models
{
    /// <summary>
    /// Type of change made to an element
    /// </summary>
    public enum ChangeType
    {
        Created,
        Modified,
        Deleted
    }

    /// <summary>
    /// Records a single change to an element
    /// </summary>
    public class ChangeRecord
    {
        public string ElementId { get; set; } = string.Empty;
        public string? UniqueId { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime Timestamp { get; set; }
        public string DocumentId { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? FamilyName { get; set; }
        public string? TypeName { get; set; }
        public string? UserId { get; set; }

        /// <summary>
        /// State before the change (for Modified type)
        /// </summary>
        public Dictionary<string, object?>? PreviousState { get; set; }

        /// <summary>
        /// Current state after the change
        /// </summary>
        public Dictionary<string, object?>? CurrentState { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {ChangeType}: {Category}/{FamilyName} ({ElementId})";
        }
    }

    /// <summary>
    /// Backup data for an entire document
    /// </summary>
    public class BackupData
    {
        public string? DocumentPath { get; set; }
        public string? DocumentTitle { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<ElementBackup> Elements { get; set; } = new List<ElementBackup>();
    }

    /// <summary>
    /// Backup data for a single element
    /// </summary>
    public class ElementBackup
    {
        public string ElementId { get; set; } = string.Empty;
        public string? UniqueId { get; set; }
        public string? Category { get; set; }
        public string? FamilyName { get; set; }
        public string? TypeName { get; set; }
        public double? LocationX { get; set; }
        public double? LocationY { get; set; }
        public double? LocationZ { get; set; }
        public Dictionary<string, object?> Parameters { get; set; } = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Information about a backup file
    /// </summary>
    public class BackupInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ElementCount { get; set; }
    }

    /// <summary>
    /// Difference between two backups
    /// </summary>
    public class BackupDiff
    {
        public DateTime OlderTimestamp { get; set; }
        public DateTime NewerTimestamp { get; set; }
        public List<ElementBackup> AddedElements { get; set; } = new List<ElementBackup>();
        public List<ElementBackup> DeletedElements { get; set; } = new List<ElementBackup>();
        public List<ElementModification> ModifiedElements { get; set; } = new List<ElementModification>();

        public int TotalChanges => AddedElements.Count + DeletedElements.Count + ModifiedElements.Count;
    }

    /// <summary>
    /// Modification details for a single element
    /// </summary>
    public class ElementModification
    {
        public string ElementId { get; set; } = string.Empty;
        public ElementBackup? Before { get; set; }
        public ElementBackup? After { get; set; }
    }
}
