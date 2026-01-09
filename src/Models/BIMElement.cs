using System;
using System.Collections.Generic;

namespace AgenticRevit.Models
{
    /// <summary>
    /// Represents a BIM element with ontology-ready properties
    /// </summary>
    public class BIMElement
    {
        #region Identity

        public string ElementId { get; set; } = string.Empty;
        public string UniqueId { get; set; } = string.Empty;
        public string? Name { get; set; }

        #endregion

        #region Classification

        public string? Category { get; set; }
        public string? FamilyName { get; set; }
        public string? TypeName { get; set; }
        public string? Level { get; set; }
        public string? Phase { get; set; }

        #endregion

        #region Geometry

        public BIMLocation? Location { get; set; }
        public BIMBoundingBox? BoundingBox { get; set; }
        public double? Volume { get; set; }
        public double? Area { get; set; }
        public double? Length { get; set; }

        #endregion

        #region Material & Assembly

        public string? Material { get; set; }
        public List<string> Materials { get; set; } = new List<string>();
        public string? AssemblyCode { get; set; }

        #endregion

        #region Project Data (CBS/WBS)

        public string? CBSCode { get; set; }
        public string? WBSCode { get; set; }
        public string? WorkPackage { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalCost { get; set; }
        public double? Quantity { get; set; }
        public string? Unit { get; set; }

        #endregion

        #region Scheduling

        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public double? Progress { get; set; }

        #endregion

        #region Parameters

        public Dictionary<string, object?> InstanceParameters { get; set; } = new Dictionary<string, object?>();
        public Dictionary<string, object?> TypeParameters { get; set; } = new Dictionary<string, object?>();

        #endregion

        #region Relationships

        public string? HostElementId { get; set; }
        public List<string> HostedElementIds { get; set; } = new List<string>();
        public string? RoomId { get; set; }
        public string? SpaceId { get; set; }
        public List<string> ConnectedElementIds { get; set; } = new List<string>();

        #endregion

        #region Metadata

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        #endregion
    }

    /// <summary>
    /// Location data for an element
    /// </summary>
    public class BIMLocation
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double? Rotation { get; set; }

        // For curve-based elements
        public BIMPoint? StartPoint { get; set; }
        public BIMPoint? EndPoint { get; set; }
    }

    /// <summary>
    /// 3D point
    /// </summary>
    public class BIMPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    /// <summary>
    /// Bounding box for an element
    /// </summary>
    public class BIMBoundingBox
    {
        public BIMPoint? Min { get; set; }
        public BIMPoint? Max { get; set; }
    }

    /// <summary>
    /// Represents a space/room in the model
    /// </summary>
    public class BIMSpace
    {
        public string ElementId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Number { get; set; }
        public string? Level { get; set; }
        public double? Area { get; set; }
        public double? Volume { get; set; }
        public string? Department { get; set; }
        public string? Function { get; set; }
        public List<string> ContainedElementIds { get; set; } = new List<string>();
        public List<string> BoundaryElementIds { get; set; } = new List<string>();
    }
}
