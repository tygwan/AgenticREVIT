using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace AgenticRevit.Core.Events
{
    /// <summary>
    /// Event arguments for element changes in the document
    /// </summary>
    public class ElementChangedEventArgs : EventArgs
    {
        public Document Document { get; }
        public ICollection<ElementId> AddedElements { get; }
        public ICollection<ElementId> ModifiedElements { get; }
        public ICollection<ElementId> DeletedElements { get; }
        public DateTime Timestamp { get; }

        public ElementChangedEventArgs(
            Document document,
            ICollection<ElementId> added,
            ICollection<ElementId> modified,
            ICollection<ElementId> deleted)
        {
            Document = document;
            AddedElements = added ?? new List<ElementId>();
            ModifiedElements = modified ?? new List<ElementId>();
            DeletedElements = deleted ?? new List<ElementId>();
            Timestamp = DateTime.Now;
        }

        public int TotalChanges =>
            AddedElements.Count + ModifiedElements.Count + DeletedElements.Count;
    }

    /// <summary>
    /// Event arguments for backup checkpoint creation
    /// </summary>
    public class BackupCreatedEventArgs : EventArgs
    {
        public string BackupPath { get; }
        public string Description { get; }
        public DateTime Timestamp { get; }
        public int ElementCount { get; }

        public BackupCreatedEventArgs(string path, string description, int elementCount)
        {
            BackupPath = path;
            Description = description;
            Timestamp = DateTime.Now;
            ElementCount = elementCount;
        }
    }

    /// <summary>
    /// Event arguments for ontology graph updates
    /// </summary>
    public class GraphUpdatedEventArgs : EventArgs
    {
        public int NodesAdded { get; }
        public int NodesModified { get; }
        public int NodesDeleted { get; }
        public int RelationshipsUpdated { get; }
        public DateTime Timestamp { get; }

        public GraphUpdatedEventArgs(int added, int modified, int deleted, int relationships)
        {
            NodesAdded = added;
            NodesModified = modified;
            NodesDeleted = deleted;
            RelationshipsUpdated = relationships;
            Timestamp = DateTime.Now;
        }
    }
}
