using System;
using System.Collections.Generic;

namespace AgenticRevit.Models
{
    /// <summary>
    /// Base class for ontology graph nodes
    /// </summary>
    public abstract class OntologyNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Label { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;
        public Dictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Physical element node in the ontology
    /// </summary>
    public class ElementNode : OntologyNode
    {
        public string RevitElementId { get; set; } = string.Empty;
        public string UniqueId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }

        public ElementNode()
        {
            NodeType = "Element";
        }
    }

    /// <summary>
    /// Spatial node (Room/Space/Level)
    /// </summary>
    public class SpatialNode : OntologyNode
    {
        public string SpatialType { get; set; } = string.Empty;  // Room, Space, Level, Zone
        public string Number { get; set; } = string.Empty;
        public double? Area { get; set; }
        public double? Volume { get; set; }
        public string Level { get; set; } = string.Empty;

        public SpatialNode()
        {
            NodeType = "Spatial";
        }
    }

    /// <summary>
    /// Task node for WBS
    /// </summary>
    public class TaskNode : OntologyNode
    {
        public string WBSCode { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public double Progress { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ParentTaskId { get; set; }

        public TaskNode()
        {
            NodeType = "Task";
        }
    }

    /// <summary>
    /// Cost node for CBS
    /// </summary>
    public class CostNode : OntologyNode
    {
        public string CBSCode { get; set; } = string.Empty;
        public string CostCategory { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "KRW";
        public string? ParentCostId { get; set; }

        public CostNode()
        {
            NodeType = "Cost";
        }
    }

    /// <summary>
    /// Document/Reference node
    /// </summary>
    public class DocumentNode : OntologyNode
    {
        public string DocumentType { get; set; } = string.Empty;  // Contract, Spec, Drawing, BOQ
        public string FilePath { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public DocumentNode()
        {
            NodeType = "Document";
        }
    }

    /// <summary>
    /// Represents a relationship between nodes
    /// </summary>
    public class OntologyRelationship
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SourceId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string RelationType { get; set; } = string.Empty;
        public Dictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Common relationship types in BIM ontology
    /// </summary>
    public static class RelationshipTypes
    {
        // Spatial relationships
        public const string LocatedIn = "LOCATED_IN";
        public const string Contains = "CONTAINS";
        public const string Adjacent = "ADJACENT_TO";
        public const string Above = "ABOVE";
        public const string Below = "BELOW";

        // Structural relationships
        public const string HostedBy = "HOSTED_BY";
        public const string Hosts = "HOSTS";
        public const string ConnectedTo = "CONNECTED_TO";
        public const string PartOf = "PART_OF";

        // Project relationships
        public const string AssignedTo = "ASSIGNED_TO";
        public const string HasCost = "HAS_COST";
        public const string DependsOn = "DEPENDS_ON";
        public const string ReferencedBy = "REFERENCED_BY";

        // Temporal relationships
        public const string Precedes = "PRECEDES";
        public const string Follows = "FOLLOWS";
        public const string Concurrent = "CONCURRENT_WITH";
    }
}
