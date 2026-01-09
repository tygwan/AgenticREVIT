using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Serilog;
using AgenticRevit.Models;
using AgenticRevit.Core.Events;
using ModelChangeType = AgenticRevit.Models.ChangeType;

namespace AgenticRevit.ChangeTracking
{
    /// <summary>
    /// Monitors and records all changes made to Revit documents
    /// </summary>
    public class ChangeMonitor : IDisposable
    {
        #region Events

        public event EventHandler<ElementChangedEventArgs>? ElementsChanged;

        #endregion

        #region Private Fields

        private readonly ConcurrentDictionary<string, DocumentChangeTracker> _documentTrackers;
        private readonly ConcurrentQueue<ChangeRecord> _changeQueue;
        private readonly object _syncLock = new object();
        private bool _isDisposed;

        #endregion

        #region Constructor

        public ChangeMonitor()
        {
            _documentTrackers = new ConcurrentDictionary<string, DocumentChangeTracker>();
            _changeQueue = new ConcurrentQueue<ChangeRecord>();

            Log.Debug("ChangeMonitor initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start monitoring changes for a document
        /// </summary>
        public void StartMonitoring(Document? document)
        {
            if (document == null) return;

            var docKey = GetDocumentKey(document);

            if (_documentTrackers.ContainsKey(docKey))
            {
                Log.Warning("Already monitoring document: {DocKey}", docKey);
                return;
            }

            var tracker = new DocumentChangeTracker(document);
            _documentTrackers[docKey] = tracker;

            Log.Information("Started monitoring document: {DocTitle}", document.Title);
        }

        /// <summary>
        /// Stop monitoring changes for a document
        /// </summary>
        public void StopMonitoring(Document? document)
        {
            if (document == null) return;

            var docKey = GetDocumentKey(document);

            if (_documentTrackers.TryRemove(docKey, out var tracker))
            {
                tracker.Dispose();
                Log.Information("Stopped monitoring document: {DocTitle}", document.Title);
            }
        }

        /// <summary>
        /// Process changes from DocumentChanged event
        /// </summary>
        public void ProcessChanges(
            Document document,
            ICollection<ElementId> addedIds,
            ICollection<ElementId> modifiedIds,
            ICollection<ElementId> deletedIds)
        {
            if (document == null) return;

            var docKey = GetDocumentKey(document);

            if (!_documentTrackers.TryGetValue(docKey, out var tracker))
            {
                Log.Warning("No tracker found for document: {DocKey}", docKey);
                return;
            }

            // Create change records
            var timestamp = DateTime.Now;

            // Process added elements
            foreach (var id in addedIds)
            {
                var element = document.GetElement(id);
                if (element != null)
                {
                    var record = CreateChangeRecord(element, ModelChangeType.Created, timestamp);
                    _changeQueue.Enqueue(record);
                    tracker.AddChange(record);
                }
            }

            // Process modified elements
            foreach (var id in modifiedIds)
            {
                var element = document.GetElement(id);
                if (element != null)
                {
                    var record = CreateChangeRecord(element, ModelChangeType.Modified, timestamp);
                    _changeQueue.Enqueue(record);
                    tracker.AddChange(record);
                }
            }

            // Process deleted elements
            foreach (var id in deletedIds)
            {
                var record = new ChangeRecord
                {
                    ElementId = id.Value.ToString(),
                    ChangeType = ModelChangeType.Deleted,
                    Timestamp = timestamp,
                    DocumentId = docKey
                };
                _changeQueue.Enqueue(record);
                tracker.AddChange(record);
            }

            // Raise event
            ElementsChanged?.Invoke(this, new ElementChangedEventArgs(
                document, addedIds, modifiedIds, deletedIds));

            Log.Debug("Processed {Total} changes (A:{Added}, M:{Modified}, D:{Deleted})",
                addedIds.Count + modifiedIds.Count + deletedIds.Count,
                addedIds.Count, modifiedIds.Count, deletedIds.Count);
        }

        /// <summary>
        /// Get all changes for a document
        /// </summary>
        public IReadOnlyList<ChangeRecord> GetChanges(Document document)
        {
            var docKey = GetDocumentKey(document);

            if (_documentTrackers.TryGetValue(docKey, out var tracker))
            {
                return tracker.GetAllChanges();
            }

            return new List<ChangeRecord>();
        }

        /// <summary>
        /// Get changes within a time range
        /// </summary>
        public IReadOnlyList<ChangeRecord> GetChanges(Document document, DateTime from, DateTime to)
        {
            var docKey = GetDocumentKey(document);

            if (_documentTrackers.TryGetValue(docKey, out var tracker))
            {
                return tracker.GetChangesByTimeRange(from, to);
            }

            return new List<ChangeRecord>();
        }

        /// <summary>
        /// Clear all change history for a document
        /// </summary>
        public void ClearHistory(Document document)
        {
            var docKey = GetDocumentKey(document);

            if (_documentTrackers.TryGetValue(docKey, out var tracker))
            {
                tracker.ClearHistory();
                Log.Information("Cleared change history for: {DocKey}", docKey);
            }
        }

        #endregion

        #region Private Methods

        private string GetDocumentKey(Document document)
        {
            return document.PathName ?? document.Title ?? Guid.NewGuid().ToString();
        }

        private ChangeRecord CreateChangeRecord(Element element, ModelChangeType changeType, DateTime timestamp)
        {
            var record = new ChangeRecord
            {
                ElementId = element.Id.Value.ToString(),
                UniqueId = element.UniqueId,
                ChangeType = changeType,
                Timestamp = timestamp,
                Category = element.Category?.Name ?? "Unknown",
                FamilyName = GetFamilyName(element),
                TypeName = GetTypeName(element),
                DocumentId = GetDocumentKey(element.Document)
            };

            // Capture current state for modified/created elements
            if (changeType != ModelChangeType.Deleted)
            {
                record.CurrentState = CaptureElementState(element);
            }

            return record;
        }

        private Dictionary<string, object?> CaptureElementState(Element element)
        {
            var state = new Dictionary<string, object?>();

            try
            {
                // Basic properties
                state["Name"] = element.Name;
                state["Category"] = element.Category?.Name;

                // Location
                if (element.Location is LocationPoint locPoint)
                {
                    state["Location_X"] = locPoint.Point.X;
                    state["Location_Y"] = locPoint.Point.Y;
                    state["Location_Z"] = locPoint.Point.Z;
                }
                else if (element.Location is LocationCurve locCurve)
                {
                    var start = locCurve.Curve.GetEndPoint(0);
                    var end = locCurve.Curve.GetEndPoint(1);
                    state["Start_X"] = start.X;
                    state["Start_Y"] = start.Y;
                    state["Start_Z"] = start.Z;
                    state["End_X"] = end.X;
                    state["End_Y"] = end.Y;
                    state["End_Z"] = end.Z;
                }

                // Parameters (instance parameters only for now)
                foreach (Parameter param in element.Parameters)
                {
                    if (param.HasValue && !param.IsReadOnly)
                    {
                        var key = param.Definition?.Name ?? "Unknown";
                        state[$"Param_{key}"] = GetParameterValue(param);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error capturing state for element {ElementId}", element.Id);
            }

            return state;
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
            try
            {
                if (element is FamilyInstance fi)
                {
                    return fi.Symbol?.FamilyName ?? "Unknown";
                }
                return element.GetType().Name;
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetTypeName(Element element)
        {
            try
            {
                if (element is FamilyInstance fi)
                {
                    return fi.Symbol?.Name ?? "Unknown";
                }
                return element.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            foreach (var tracker in _documentTrackers.Values)
            {
                tracker.Dispose();
            }
            _documentTrackers.Clear();

            _isDisposed = true;
            Log.Debug("ChangeMonitor disposed");
        }

        #endregion
    }

    /// <summary>
    /// Tracks changes for a single document
    /// </summary>
    internal class DocumentChangeTracker : IDisposable
    {
        private readonly Document _document;
        private readonly List<ChangeRecord> _changes;
        private readonly object _lock = new object();
        private bool _isDisposed;

        public DocumentChangeTracker(Document document)
        {
            _document = document;
            _changes = new List<ChangeRecord>();
        }

        public void AddChange(ChangeRecord record)
        {
            lock (_lock)
            {
                _changes.Add(record);
            }
        }

        public IReadOnlyList<ChangeRecord> GetAllChanges()
        {
            lock (_lock)
            {
                return _changes.ToList();
            }
        }

        public IReadOnlyList<ChangeRecord> GetChangesByTimeRange(DateTime from, DateTime to)
        {
            lock (_lock)
            {
                return _changes
                    .Where(c => c.Timestamp >= from && c.Timestamp <= to)
                    .ToList();
            }
        }

        public void ClearHistory()
        {
            lock (_lock)
            {
                _changes.Clear();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _changes.Clear();
            _isDisposed = true;
        }
    }
}
