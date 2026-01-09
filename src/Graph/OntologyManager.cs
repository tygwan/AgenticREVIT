using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Serilog;
using AgenticRevit.Models;
using AgenticRevit.Core.Events;

namespace AgenticRevit.Graph
{
    /// <summary>
    /// Manages the BIM ontology graph structure
    /// </summary>
    public class OntologyManager : IDisposable
    {
        #region Events

        public event EventHandler<GraphUpdatedEventArgs>? GraphUpdated;

        #endregion

        #region Private Fields

        private readonly ConcurrentDictionary<string, OntologyNode> _nodes;
        private readonly ConcurrentDictionary<string, OntologyRelationship> _relationships;
        private readonly ConcurrentDictionary<string, string> _revitIdToNodeId;
        private readonly GraphDBConnector? _graphDbConnector;
        private readonly object _syncLock = new object();
        private bool _isDisposed;

        #endregion

        #region Constructor

        public OntologyManager(GraphDBConnector? graphDbConnector = null)
        {
            _nodes = new ConcurrentDictionary<string, OntologyNode>();
            _relationships = new ConcurrentDictionary<string, OntologyRelationship>();
            _revitIdToNodeId = new ConcurrentDictionary<string, string>();
            _graphDbConnector = graphDbConnector;

            Log.Debug("OntologyManager initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build initial graph from document
        /// </summary>
        public void BuildInitialGraph(Document? document)
        {
            if (document == null) return;

            Log.Information("Building initial ontology graph for: {DocTitle}", document.Title);

            try
            {
                // Clear existing data
                ClearGraph();

                // Build element nodes
                BuildElementNodes(document);

                // Build spatial nodes (Rooms, Spaces, Levels)
                BuildSpatialNodes(document);

                // Build relationships
                BuildRelationships(document);

                Log.Information("Ontology graph built: {NodeCount} nodes, {RelCount} relationships",
                    _nodes.Count, _relationships.Count);

                // Sync to GraphDB if connected
                SyncToGraphDB();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error building initial ontology graph");
            }
        }

        /// <summary>
        /// Update graph based on element changes
        /// </summary>
        public void UpdateGraph(
            Document document,
            ICollection<ElementId> addedIds,
            ICollection<ElementId> modifiedIds,
            ICollection<ElementId> deletedIds)
        {
            if (document == null) return;

            int nodesAdded = 0, nodesModified = 0, nodesDeleted = 0, relsUpdated = 0;

            try
            {
                // Process added elements
                foreach (var id in addedIds)
                {
                    var element = document.GetElement(id);
                    if (element != null && ShouldIncludeElement(element))
                    {
                        AddElementNode(element);
                        nodesAdded++;
                    }
                }

                // Process modified elements
                foreach (var id in modifiedIds)
                {
                    var element = document.GetElement(id);
                    if (element != null)
                    {
                        UpdateElementNode(element);
                        nodesModified++;
                    }
                }

                // Process deleted elements
                foreach (var id in deletedIds)
                {
                    RemoveElementNode(id.IntegerValue.ToString());
                    nodesDeleted++;
                }

                // Update relationships for affected elements
                relsUpdated = UpdateAffectedRelationships(document, addedIds, modifiedIds);

                // Raise event
                GraphUpdated?.Invoke(this, new GraphUpdatedEventArgs(
                    nodesAdded, nodesModified, nodesDeleted, relsUpdated));

                // Sync to GraphDB if connected
                SyncToGraphDB();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating ontology graph");
            }
        }

        /// <summary>
        /// Get all nodes of a specific type
        /// </summary>
        public IReadOnlyList<T> GetNodes<T>() where T : OntologyNode
        {
            return _nodes.Values.OfType<T>().ToList();
        }

        /// <summary>
        /// Get node by Revit element ID
        /// </summary>
        public OntologyNode? GetNodeByRevitId(string revitElementId)
        {
            if (_revitIdToNodeId.TryGetValue(revitElementId, out var nodeId))
            {
                if (_nodes.TryGetValue(nodeId, out var node))
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Get all relationships for a node
        /// </summary>
        public IReadOnlyList<OntologyRelationship> GetRelationships(string nodeId)
        {
            return _relationships.Values
                .Where(r => r.SourceId == nodeId || r.TargetId == nodeId)
                .ToList();
        }

        /// <summary>
        /// Query graph using a simple predicate
        /// </summary>
        public IReadOnlyList<OntologyNode> Query(Func<OntologyNode, bool> predicate)
        {
            return _nodes.Values.Where(predicate).ToList();
        }

        /// <summary>
        /// Get graph statistics
        /// </summary>
        public GraphStatistics GetStatistics()
        {
            return new GraphStatistics
            {
                TotalNodes = _nodes.Count,
                TotalRelationships = _relationships.Count,
                ElementNodes = _nodes.Values.Count(n => n is ElementNode),
                SpatialNodes = _nodes.Values.Count(n => n is SpatialNode),
                TaskNodes = _nodes.Values.Count(n => n is TaskNode),
                CostNodes = _nodes.Values.Count(n => n is CostNode),
                DocumentNodes = _nodes.Values.Count(n => n is DocumentNode)
            };
        }

        #endregion

        #region Private Methods - Graph Building

        private void BuildElementNodes(Document document)
        {
            var collector = new FilteredElementCollector(document)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                if (ShouldIncludeElement(element))
                {
                    AddElementNode(element);
                }
            }
        }

        private void BuildSpatialNodes(Document document)
        {
            // Build Level nodes
            var levels = new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Cast<Level>();

            foreach (var level in levels)
            {
                var node = new SpatialNode
                {
                    Label = level.Name,
                    SpatialType = "Level",
                    Level = level.Name
                };
                node.Properties["Elevation"] = level.Elevation;

                _nodes[node.Id] = node;
                _revitIdToNodeId[level.Id.IntegerValue.ToString()] = node.Id;
            }

            // Build Room nodes
            var rooms = new FilteredElementCollector(document)
                .OfClass(typeof(SpatialElement))
                .Cast<SpatialElement>();

            foreach (var room in rooms)
            {
                var node = new SpatialNode
                {
                    Label = room.Name ?? "Unnamed",
                    SpatialType = room.GetType().Name,
                    Number = GetParameterStringValue(room, BuiltInParameter.ROOM_NUMBER) ?? "",
                    Level = room.Level?.Name ?? ""
                };

                // Try to get area and volume
                node.Area = GetParameterDoubleValue(room, BuiltInParameter.ROOM_AREA);
                node.Volume = GetParameterDoubleValue(room, BuiltInParameter.ROOM_VOLUME);

                _nodes[node.Id] = node;
                _revitIdToNodeId[room.Id.IntegerValue.ToString()] = node.Id;
            }
        }

        private void BuildRelationships(Document document)
        {
            // Build element-to-level relationships
            foreach (var nodeKvp in _nodes)
            {
                if (nodeKvp.Value is ElementNode elementNode)
                {
                    var element = document.GetElement(new ElementId(int.Parse(elementNode.RevitElementId)));
                    if (element != null)
                    {
                        BuildElementRelationships(element, elementNode);
                    }
                }
            }
        }

        private void BuildElementRelationships(Element element, ElementNode elementNode)
        {
            try
            {
                // Level relationship
                var levelId = element.LevelId;
                if (levelId != ElementId.InvalidElementId)
                {
                    var levelNodeId = GetNodeIdByRevitId(levelId.IntegerValue.ToString());
                    if (levelNodeId != null)
                    {
                        CreateRelationship(elementNode.Id, levelNodeId, RelationshipTypes.LocatedIn);
                    }
                }

                // Host relationship (for hosted elements)
                if (element is FamilyInstance fi && fi.Host != null)
                {
                    var hostNodeId = GetNodeIdByRevitId(fi.Host.Id.IntegerValue.ToString());
                    if (hostNodeId != null)
                    {
                        CreateRelationship(elementNode.Id, hostNodeId, RelationshipTypes.HostedBy);
                    }
                }

                // Room relationship
                if (element is FamilyInstance instance)
                {
                    var room = instance.Room;
                    if (room != null)
                    {
                        var roomNodeId = GetNodeIdByRevitId(room.Id.IntegerValue.ToString());
                        if (roomNodeId != null)
                        {
                            CreateRelationship(elementNode.Id, roomNodeId, RelationshipTypes.LocatedIn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error building relationships for element: {ElementId}", elementNode.RevitElementId);
            }
        }

        private void AddElementNode(Element element)
        {
            var node = new ElementNode
            {
                Label = element.Name ?? "Unnamed",
                RevitElementId = element.Id.IntegerValue.ToString(),
                UniqueId = element.UniqueId,
                Category = element.Category?.Name ?? "Unknown",
                Family = GetFamilyName(element),
                Type = GetTypeName(element)
            };

            // Location
            if (element.Location is LocationPoint locPoint)
            {
                node.X = locPoint.Point.X;
                node.Y = locPoint.Point.Y;
                node.Z = locPoint.Point.Z;
            }

            _nodes[node.Id] = node;
            _revitIdToNodeId[node.RevitElementId] = node.Id;
        }

        private void UpdateElementNode(Element element)
        {
            var revitId = element.Id.IntegerValue.ToString();

            if (_revitIdToNodeId.TryGetValue(revitId, out var nodeId))
            {
                if (_nodes.TryGetValue(nodeId, out var existingNode) && existingNode is ElementNode elementNode)
                {
                    // Update properties
                    elementNode.Label = element.Name ?? "Unnamed";
                    elementNode.Category = element.Category?.Name ?? "Unknown";
                    elementNode.ModifiedAt = DateTime.Now;

                    // Update location
                    if (element.Location is LocationPoint locPoint)
                    {
                        elementNode.X = locPoint.Point.X;
                        elementNode.Y = locPoint.Point.Y;
                        elementNode.Z = locPoint.Point.Z;
                    }
                }
            }
        }

        private void RemoveElementNode(string revitElementId)
        {
            if (_revitIdToNodeId.TryRemove(revitElementId, out var nodeId))
            {
                _nodes.TryRemove(nodeId, out _);

                // Remove related relationships
                var relatedRels = _relationships.Values
                    .Where(r => r.SourceId == nodeId || r.TargetId == nodeId)
                    .Select(r => r.Id)
                    .ToList();

                foreach (var relId in relatedRels)
                {
                    _relationships.TryRemove(relId, out _);
                }
            }
        }

        private int UpdateAffectedRelationships(
            Document document,
            ICollection<ElementId> addedIds,
            ICollection<ElementId> modifiedIds)
        {
            int count = 0;

            foreach (var id in addedIds.Concat(modifiedIds))
            {
                var element = document.GetElement(id);
                if (element != null)
                {
                    var nodeId = GetNodeIdByRevitId(id.IntegerValue.ToString());
                    if (nodeId != null && _nodes.TryGetValue(nodeId, out var node) && node is ElementNode elementNode)
                    {
                        BuildElementRelationships(element, elementNode);
                        count++;
                    }
                }
            }

            return count;
        }

        #endregion

        #region Private Methods - Helpers

        private bool ShouldIncludeElement(Element element)
        {
            if (element == null || element.Category == null)
                return false;

            // Skip certain categories
            var excludedCategories = new[]
            {
                BuiltInCategory.OST_RvtLinks,
                BuiltInCategory.OST_Views,
                BuiltInCategory.OST_Sheets,
                BuiltInCategory.OST_Cameras
            };

            var categoryId = element.Category.Id.IntegerValue;

            foreach (var excluded in excludedCategories)
            {
                if (categoryId == (int)excluded)
                    return false;
            }

            return true;
        }

        private string? GetNodeIdByRevitId(string revitElementId)
        {
            _revitIdToNodeId.TryGetValue(revitElementId, out var nodeId);
            return nodeId;
        }

        private void CreateRelationship(string sourceId, string targetId, string relationType)
        {
            var relationship = new OntologyRelationship
            {
                SourceId = sourceId,
                TargetId = targetId,
                RelationType = relationType
            };

            _relationships[relationship.Id] = relationship;
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

        private string? GetParameterStringValue(Element element, BuiltInParameter param)
        {
            try
            {
                var p = element.get_Parameter(param);
                return p?.AsString();
            }
            catch
            {
                return null;
            }
        }

        private double? GetParameterDoubleValue(Element element, BuiltInParameter param)
        {
            try
            {
                var p = element.get_Parameter(param);
                return p?.AsDouble();
            }
            catch
            {
                return null;
            }
        }

        private void ClearGraph()
        {
            _nodes.Clear();
            _relationships.Clear();
            _revitIdToNodeId.Clear();
        }

        private void SyncToGraphDB()
        {
            if (_graphDbConnector == null || !_graphDbConnector.IsConnected)
                return;

            // TODO: Implement sync to external GraphDB (Neo4j)
            Log.Debug("GraphDB sync not yet implemented");
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            ClearGraph();
            _graphDbConnector?.Dispose();
            _isDisposed = true;

            Log.Debug("OntologyManager disposed");
        }

        #endregion
    }

    /// <summary>
    /// Graph statistics
    /// </summary>
    public class GraphStatistics
    {
        public int TotalNodes { get; set; }
        public int TotalRelationships { get; set; }
        public int ElementNodes { get; set; }
        public int SpatialNodes { get; set; }
        public int TaskNodes { get; set; }
        public int CostNodes { get; set; }
        public int DocumentNodes { get; set; }
    }
}
