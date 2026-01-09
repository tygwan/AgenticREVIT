using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using Serilog;
using AgenticRevit.Models;

namespace AgenticRevit.Graph
{
    /// <summary>
    /// Connector for Neo4j GraphDB
    /// </summary>
    public class GraphDBConnector : IDisposable
    {
        #region Private Fields

        private IDriver? _driver;
        private readonly string _uri;
        private readonly string _username;
        private readonly string _password;
        private bool _isConnected;
        private bool _isDisposed;

        #endregion

        #region Properties

        public bool IsConnected => _isConnected && _driver != null;

        #endregion

        #region Constructor

        public GraphDBConnector(string uri, string username, string password)
        {
            _uri = uri;
            _username = username;
            _password = password;
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Connect to Neo4j database
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                _driver = GraphDatabase.Driver(_uri, AuthTokens.Basic(_username, _password));

                // Verify connection
                await using var session = _driver.AsyncSession();
                var result = await session.RunAsync("RETURN 1");
                await result.ConsumeAsync();

                _isConnected = true;
                Log.Information("Connected to Neo4j at {Uri}", _uri);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to connect to Neo4j at {Uri}", _uri);
                _isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnect from database
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_driver != null)
            {
                await _driver.CloseAsync();
                _isConnected = false;
                Log.Information("Disconnected from Neo4j");
            }
        }

        #endregion

        #region Node Operations

        /// <summary>
        /// Create or update an element node
        /// </summary>
        public async Task UpsertElementNodeAsync(ElementNode node)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (n:Element {id: $id})
                        SET n.label = $label,
                            n.revitElementId = $revitElementId,
                            n.uniqueId = $uniqueId,
                            n.category = $category,
                            n.family = $family,
                            n.type = $type,
                            n.x = $x,
                            n.y = $y,
                            n.z = $z,
                            n.modifiedAt = datetime()
                    ";

                    await tx.RunAsync(query, new
                    {
                        id = node.Id,
                        label = node.Label,
                        revitElementId = node.RevitElementId,
                        uniqueId = node.UniqueId,
                        category = node.Category,
                        family = node.Family,
                        type = node.Type,
                        x = node.X,
                        y = node.Y,
                        z = node.Z
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upsert element node: {NodeId}", node.Id);
            }
        }

        /// <summary>
        /// Create or update a spatial node
        /// </summary>
        public async Task UpsertSpatialNodeAsync(SpatialNode node)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (n:Spatial {id: $id})
                        SET n.label = $label,
                            n.spatialType = $spatialType,
                            n.number = $number,
                            n.area = $area,
                            n.volume = $volume,
                            n.level = $level,
                            n.modifiedAt = datetime()
                    ";

                    await tx.RunAsync(query, new
                    {
                        id = node.Id,
                        label = node.Label,
                        spatialType = node.SpatialType,
                        number = node.Number,
                        area = node.Area,
                        volume = node.Volume,
                        level = node.Level
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upsert spatial node: {NodeId}", node.Id);
            }
        }

        /// <summary>
        /// Create or update a task node
        /// </summary>
        public async Task UpsertTaskNodeAsync(TaskNode node)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (n:Task {id: $id})
                        SET n.label = $label,
                            n.wbsCode = $wbsCode,
                            n.taskName = $taskName,
                            n.plannedStart = $plannedStart,
                            n.plannedEnd = $plannedEnd,
                            n.actualStart = $actualStart,
                            n.actualEnd = $actualEnd,
                            n.progress = $progress,
                            n.status = $status,
                            n.modifiedAt = datetime()
                    ";

                    await tx.RunAsync(query, new
                    {
                        id = node.Id,
                        label = node.Label,
                        wbsCode = node.WBSCode,
                        taskName = node.TaskName,
                        plannedStart = node.PlannedStart?.ToString("o"),
                        plannedEnd = node.PlannedEnd?.ToString("o"),
                        actualStart = node.ActualStart?.ToString("o"),
                        actualEnd = node.ActualEnd?.ToString("o"),
                        progress = node.Progress,
                        status = node.Status
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upsert task node: {NodeId}", node.Id);
            }
        }

        /// <summary>
        /// Create or update a cost node
        /// </summary>
        public async Task UpsertCostNodeAsync(CostNode node)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (n:Cost {id: $id})
                        SET n.label = $label,
                            n.cbsCode = $cbsCode,
                            n.costCategory = $costCategory,
                            n.unitCost = $unitCost,
                            n.quantity = $quantity,
                            n.unit = $unit,
                            n.totalCost = $totalCost,
                            n.currency = $currency,
                            n.modifiedAt = datetime()
                    ";

                    await tx.RunAsync(query, new
                    {
                        id = node.Id,
                        label = node.Label,
                        cbsCode = node.CBSCode,
                        costCategory = node.CostCategory,
                        unitCost = (double)node.UnitCost,
                        quantity = node.Quantity,
                        unit = node.Unit,
                        totalCost = (double)node.TotalCost,
                        currency = node.Currency
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upsert cost node: {NodeId}", node.Id);
            }
        }

        /// <summary>
        /// Delete a node
        /// </summary>
        public async Task DeleteNodeAsync(string nodeId)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (n {id: $id})
                        DETACH DELETE n
                    ";

                    await tx.RunAsync(query, new { id = nodeId });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete node: {NodeId}", nodeId);
            }
        }

        #endregion

        #region Relationship Operations

        /// <summary>
        /// Create a relationship between nodes
        /// </summary>
        public async Task CreateRelationshipAsync(OntologyRelationship relationship)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    // Dynamic relationship type requires string interpolation
                    // Note: In production, validate relationType to prevent injection
                    var query = $@"
                        MATCH (source {{id: $sourceId}})
                        MATCH (target {{id: $targetId}})
                        MERGE (source)-[r:{relationship.RelationType}]->(target)
                        SET r.id = $relId,
                            r.createdAt = datetime()
                    ";

                    await tx.RunAsync(query, new
                    {
                        sourceId = relationship.SourceId,
                        targetId = relationship.TargetId,
                        relId = relationship.Id
                    });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create relationship: {RelId}", relationship.Id);
            }
        }

        /// <summary>
        /// Delete a relationship
        /// </summary>
        public async Task DeleteRelationshipAsync(string relationshipId)
        {
            if (!IsConnected) return;

            try
            {
                await using var session = _driver!.AsyncSession();
                await session.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH ()-[r {id: $id}]-()
                        DELETE r
                    ";

                    await tx.RunAsync(query, new { id = relationshipId });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete relationship: {RelId}", relationshipId);
            }
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Execute a Cypher query
        /// </summary>
        public async Task<List<Dictionary<string, object>>> QueryAsync(string cypherQuery, object? parameters = null)
        {
            var results = new List<Dictionary<string, object>>();

            if (!IsConnected) return results;

            try
            {
                await using var session = _driver!.AsyncSession();
                var result = await session.RunAsync(cypherQuery, parameters);

                await result.ForEachAsync(record =>
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var key in record.Keys)
                    {
                        dict[key] = record[key];
                    }
                    results.Add(dict);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to execute query: {Query}", cypherQuery);
            }

            return results;
        }

        /// <summary>
        /// Get all elements in a spatial region
        /// </summary>
        public async Task<List<ElementNode>> GetElementsInSpatialAsync(string spatialId)
        {
            var elements = new List<ElementNode>();

            if (!IsConnected) return elements;

            try
            {
                var query = @"
                    MATCH (e:Element)-[:LOCATED_IN]->(s:Spatial {id: $spatialId})
                    RETURN e
                ";

                var results = await QueryAsync(query, new { spatialId });

                foreach (var result in results)
                {
                    if (result.TryGetValue("e", out var nodeObj) && nodeObj is INode node)
                    {
                        var elementNode = new ElementNode
                        {
                            Id = node.Properties["id"].As<string>(),
                            Label = node.Properties["label"].As<string>(),
                            RevitElementId = node.Properties["revitElementId"].As<string>(),
                            Category = node.Properties["category"].As<string>(),
                            Family = node.Properties["family"].As<string>(),
                            Type = node.Properties["type"].As<string>()
                        };
                        elements.Add(elementNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get elements in spatial: {SpatialId}", spatialId);
            }

            return elements;
        }

        /// <summary>
        /// Get all tasks for an element
        /// </summary>
        public async Task<List<TaskNode>> GetElementTasksAsync(string elementId)
        {
            var tasks = new List<TaskNode>();

            if (!IsConnected) return tasks;

            try
            {
                var query = @"
                    MATCH (e:Element {id: $elementId})-[:ASSIGNED_TO]->(t:Task)
                    RETURN t
                ";

                var results = await QueryAsync(query, new { elementId });

                foreach (var result in results)
                {
                    if (result.TryGetValue("t", out var nodeObj) && nodeObj is INode node)
                    {
                        var taskNode = new TaskNode
                        {
                            Id = node.Properties["id"].As<string>(),
                            Label = node.Properties["label"].As<string>(),
                            WBSCode = node.Properties["wbsCode"].As<string>(),
                            TaskName = node.Properties["taskName"].As<string>(),
                            Progress = node.Properties["progress"].As<double>(),
                            Status = node.Properties["status"].As<string>()
                        };
                        tasks.Add(taskNode);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to get tasks for element: {ElementId}", elementId);
            }

            return tasks;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;

            _driver?.Dispose();
            _isConnected = false;
            _isDisposed = true;

            Log.Debug("GraphDBConnector disposed");
        }

        #endregion
    }
}
