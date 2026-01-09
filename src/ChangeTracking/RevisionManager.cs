using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Serilog;
using AgenticRevit.Models;
using AgenticRevit.Core.Events;

namespace AgenticRevit.ChangeTracking
{
    /// <summary>
    /// Manages hourly backup checkpoints and revision history
    /// </summary>
    public class RevisionManager : IDisposable
    {
        #region Events

        public event EventHandler<BackupCreatedEventArgs>? BackupCreated;

        #endregion

        #region Private Fields

        private readonly ConcurrentDictionary<string, DocumentBackupState> _documentStates;
        private readonly string _backupRootPath;
        private readonly TimeSpan _backupInterval;
        private readonly Dictionary<string, Timer> _backupTimers;
        private readonly object _timerLock = new object();
        private bool _isDisposed;

        #endregion

        #region Configuration

        private const int DefaultBackupIntervalMinutes = 60;  // 1 hour
        private const int MaxBackupsPerDay = 24;
        private const int RetainDays = 7;

        #endregion

        #region Constructor

        public RevisionManager(string? backupPath = null, int backupIntervalMinutes = DefaultBackupIntervalMinutes)
        {
            _documentStates = new ConcurrentDictionary<string, DocumentBackupState>();
            _backupTimers = new Dictionary<string, Timer>();
            _backupInterval = TimeSpan.FromMinutes(backupIntervalMinutes);

            // Set backup root path
            _backupRootPath = backupPath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AgenticRevit", "Backups");

            // Ensure backup directory exists
            Directory.CreateDirectory(_backupRootPath);

            Log.Information("RevisionManager initialized. Backup path: {Path}, Interval: {Interval}min",
                _backupRootPath, backupIntervalMinutes);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize backup tracking for a document
        /// </summary>
        public void InitializeForDocument(Document? document)
        {
            if (document == null) return;

            var docKey = GetDocumentKey(document);
            var docBackupPath = GetDocumentBackupPath(document);

            // Create document-specific backup directory
            Directory.CreateDirectory(docBackupPath);

            // Initialize state
            var state = new DocumentBackupState
            {
                DocumentPath = document.PathName,
                DocumentTitle = document.Title,
                BackupPath = docBackupPath,
                LastBackupTime = DateTime.MinValue,
                BackupCount = 0
            };

            _documentStates[docKey] = state;

            // Start backup timer
            StartBackupTimer(document);

            // Create initial checkpoint
            CreateCheckpoint(document, "Session Start");

            Log.Information("Initialized backup tracking for: {DocTitle}", document.Title);
        }

        /// <summary>
        /// Create a backup checkpoint
        /// </summary>
        public void CreateCheckpoint(Document? document, string description)
        {
            if (document == null) return;

            var docKey = GetDocumentKey(document);

            if (!_documentStates.TryGetValue(docKey, out var state))
            {
                Log.Warning("No backup state found for document: {DocKey}", docKey);
                return;
            }

            try
            {
                var timestamp = DateTime.Now;
                var backup = CreateBackupData(document, description, timestamp);

                // Save backup file
                var backupFileName = $"backup_{timestamp:yyyy-MM-dd_HH-mm-ss}.json";
                var backupFilePath = Path.Combine(state.BackupPath, backupFileName);

                var json = JsonConvert.SerializeObject(backup, Formatting.Indented);
                File.WriteAllText(backupFilePath, json);

                // Update state
                state.LastBackupTime = timestamp;
                state.BackupCount++;
                state.LastBackupPath = backupFilePath;

                // Raise event
                BackupCreated?.Invoke(this, new BackupCreatedEventArgs(
                    backupFilePath, description, backup.Elements.Count));

                Log.Information("Created checkpoint: {Description} at {Path}",
                    description, backupFilePath);

                // Cleanup old backups
                CleanupOldBackups(state.BackupPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create checkpoint for document: {DocKey}", docKey);
            }
        }

        /// <summary>
        /// Get all available backups for a document
        /// </summary>
        public IReadOnlyList<BackupInfo> GetAvailableBackups(Document document)
        {
            var docKey = GetDocumentKey(document);
            var backups = new List<BackupInfo>();

            if (!_documentStates.TryGetValue(docKey, out var state))
            {
                return backups;
            }

            try
            {
                var backupFiles = Directory.GetFiles(state.BackupPath, "backup_*.json")
                    .OrderByDescending(f => f);

                foreach (var file in backupFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var backup = JsonConvert.DeserializeObject<BackupData>(json);

                        if (backup != null)
                        {
                            backups.Add(new BackupInfo
                            {
                                FilePath = file,
                                Timestamp = backup.Timestamp,
                                Description = backup.Description,
                                ElementCount = backup.Elements?.Count ?? 0
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to read backup file: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get available backups");
            }

            return backups;
        }

        /// <summary>
        /// Get backup data from a specific backup file
        /// </summary>
        public BackupData? GetBackupData(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    Log.Warning("Backup file not found: {Path}", backupFilePath);
                    return null;
                }

                var json = File.ReadAllText(backupFilePath);
                return JsonConvert.DeserializeObject<BackupData>(json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read backup data from: {Path}", backupFilePath);
                return null;
            }
        }

        /// <summary>
        /// Compare two backups and get differences
        /// </summary>
        public BackupDiff CompareBackups(string olderBackupPath, string newerBackupPath)
        {
            var diff = new BackupDiff();

            try
            {
                var older = GetBackupData(olderBackupPath);
                var newer = GetBackupData(newerBackupPath);

                if (older == null || newer == null)
                {
                    Log.Warning("Cannot compare backups - one or both not found");
                    return diff;
                }

                var olderElements = older.Elements.ToDictionary(e => e.ElementId);
                var newerElements = newer.Elements.ToDictionary(e => e.ElementId);

                // Find added elements
                foreach (var kvp in newerElements)
                {
                    if (!olderElements.ContainsKey(kvp.Key))
                    {
                        diff.AddedElements.Add(kvp.Value);
                    }
                }

                // Find deleted elements
                foreach (var kvp in olderElements)
                {
                    if (!newerElements.ContainsKey(kvp.Key))
                    {
                        diff.DeletedElements.Add(kvp.Value);
                    }
                }

                // Find modified elements
                foreach (var kvp in newerElements)
                {
                    if (olderElements.TryGetValue(kvp.Key, out var olderElement))
                    {
                        if (!AreElementsEqual(olderElement, kvp.Value))
                        {
                            diff.ModifiedElements.Add(new ElementModification
                            {
                                ElementId = kvp.Key,
                                Before = olderElement,
                                After = kvp.Value
                            });
                        }
                    }
                }

                diff.OlderTimestamp = older.Timestamp;
                diff.NewerTimestamp = newer.Timestamp;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to compare backups");
            }

            return diff;
        }

        #endregion

        #region Private Methods

        private string GetDocumentKey(Document document)
        {
            return document.PathName ?? document.Title ?? Guid.NewGuid().ToString();
        }

        private string GetDocumentBackupPath(Document document)
        {
            var sanitizedName = SanitizeFileName(document.Title ?? "Untitled");
            return Path.Combine(_backupRootPath, sanitizedName);
        }

        private string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }

        private void StartBackupTimer(Document document)
        {
            var docKey = GetDocumentKey(document);

            lock (_timerLock)
            {
                if (_backupTimers.ContainsKey(docKey))
                {
                    return;
                }

                // Create timer that fires hourly
                var timer = new Timer(
                    callback: _ => OnBackupTimerElapsed(document),
                    state: null,
                    dueTime: _backupInterval,
                    period: _backupInterval);

                _backupTimers[docKey] = timer;

                Log.Debug("Started backup timer for: {DocKey}", docKey);
            }
        }

        private void StopBackupTimer(Document document)
        {
            var docKey = GetDocumentKey(document);

            lock (_timerLock)
            {
                if (_backupTimers.TryGetValue(docKey, out var timer))
                {
                    timer.Dispose();
                    _backupTimers.Remove(docKey);
                    Log.Debug("Stopped backup timer for: {DocKey}", docKey);
                }
            }
        }

        private void OnBackupTimerElapsed(Document document)
        {
            try
            {
                var description = $"Hourly Backup - {DateTime.Now:HH:mm}";
                CreateCheckpoint(document, description);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during scheduled backup");
            }
        }

        private BackupData CreateBackupData(Document document, string description, DateTime timestamp)
        {
            var backup = new BackupData
            {
                DocumentPath = document.PathName,
                DocumentTitle = document.Title,
                Description = description,
                Timestamp = timestamp,
                Elements = new List<ElementBackup>()
            };

            // Collect all elements
            var collector = new FilteredElementCollector(document)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                try
                {
                    var elementBackup = CreateElementBackup(element);
                    if (elementBackup != null)
                    {
                        backup.Elements.Add(elementBackup);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to backup element: {ElementId}", element.Id);
                }
            }

            return backup;
        }

        private ElementBackup? CreateElementBackup(Element element)
        {
            if (element == null || element.Id == null) return null;

            var backup = new ElementBackup
            {
                ElementId = element.Id.Value.ToString(),
                UniqueId = element.UniqueId,
                Category = element.Category?.Name ?? "Unknown",
                FamilyName = GetFamilyName(element),
                TypeName = GetTypeName(element),
                Parameters = new Dictionary<string, object?>()
            };

            // Capture location
            if (element.Location is LocationPoint locPoint)
            {
                backup.LocationX = locPoint.Point.X;
                backup.LocationY = locPoint.Point.Y;
                backup.LocationZ = locPoint.Point.Z;
            }
            else if (element.Location is LocationCurve locCurve)
            {
                var start = locCurve.Curve.GetEndPoint(0);
                backup.LocationX = start.X;
                backup.LocationY = start.Y;
                backup.LocationZ = start.Z;
            }

            // Capture key parameters
            foreach (Parameter param in element.Parameters)
            {
                if (param.HasValue && param.Definition != null)
                {
                    var key = param.Definition.Name;
                    backup.Parameters[key] = GetParameterValue(param);
                }
            }

            return backup;
        }

        private object? GetParameterValue(Parameter param)
        {
            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.AsString();
                case StorageType.Integer:
                    return param.AsInteger();
                case StorageType.Double:
                    return param.AsDouble();
                case StorageType.ElementId:
                    return param.AsElementId()?.Value;
                default:
                    return null;
            }
        }

        private string GetFamilyName(Element element)
        {
            if (element is FamilyInstance fi)
            {
                return fi.Symbol?.FamilyName ?? "Unknown";
            }
            return element.GetType().Name;
        }

        private string GetTypeName(Element element)
        {
            if (element is FamilyInstance fi)
            {
                return fi.Symbol?.Name ?? "Unknown";
            }
            return element.Name ?? "Unknown";
        }

        private bool AreElementsEqual(ElementBackup a, ElementBackup b)
        {
            // Compare key properties
            if (a.Category != b.Category ||
                a.FamilyName != b.FamilyName ||
                a.TypeName != b.TypeName)
            {
                return false;
            }

            // Compare location (with tolerance)
            const double tolerance = 0.001;
            if (Math.Abs((a.LocationX ?? 0) - (b.LocationX ?? 0)) > tolerance ||
                Math.Abs((a.LocationY ?? 0) - (b.LocationY ?? 0)) > tolerance ||
                Math.Abs((a.LocationZ ?? 0) - (b.LocationZ ?? 0)) > tolerance)
            {
                return false;
            }

            // Compare parameter count (simple check)
            if (a.Parameters.Count != b.Parameters.Count)
            {
                return false;
            }

            return true;
        }

        private void CleanupOldBackups(string backupPath)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-RetainDays);
                var backupFiles = Directory.GetFiles(backupPath, "backup_*.json");

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        Log.Debug("Deleted old backup: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error during backup cleanup");
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            lock (_timerLock)
            {
                foreach (var timer in _backupTimers.Values)
                {
                    timer.Dispose();
                }
                _backupTimers.Clear();
            }

            _documentStates.Clear();
            _isDisposed = true;

            Log.Debug("RevisionManager disposed");
        }

        #endregion
    }

    #region Supporting Classes

    internal class DocumentBackupState
    {
        public string? DocumentPath { get; set; }
        public string? DocumentTitle { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public DateTime LastBackupTime { get; set; }
        public string? LastBackupPath { get; set; }
        public int BackupCount { get; set; }
    }

    #endregion
}
