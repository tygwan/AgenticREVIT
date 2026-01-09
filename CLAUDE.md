# AgenticREVIT - Revit 2025 Agentic BIM Plugin

## Project Overview

| Item | Description |
|------|-------------|
| **Project Name** | AgenticREVIT |
| **Target Platform** | Autodesk Revit 2025 |
| **Tech Stack** | C# .NET Framework 4.8, WPF, Revit API 2025, LangGraph, Neo4j/GraphDB |
| **Architecture** | Plugin-based with Ontology-driven data model |
| **GitHub** | https://github.com/tygwan/AgenticREVIT.git |

## Core Objectives

### 1. GraphDB/Ontology-based Information Structuring
- Convert 3D Revit project data to LangGraph-based GraphDB
- Ontology modeling of BIM parameters
- Real-time knowledge graph updates

### 2. Real-time Change Tracking System
- Instant reflection and recording of model modifications
- **Hourly backup checkpoints** for revision management
- Change history with diff visualization

### 3. BIM Workflow Automation
- Contract-based task execution
- Cost estimation and BOQ (Bill of Quantities)
- **CBS** (Cost Breakdown Structure)
- **WBS** (Work Breakdown Structure)
- Quantity takeoff and material allocation
- Progress tracking (milestones, completion rates)

### 4. LLM-integrated Decision Support
- Automated document generation
- Project data-driven business intelligence
- Natural language query interface for BIM data

---

## Technical Architecture

```
AgenticRevit/
├── src/
│   ├── Core/                          # Core plugin infrastructure
│   │   ├── AgenticRevitPlugin.cs      # Main ExternalApplication
│   │   ├── Commands/                  # IExternalCommand implementations
│   │   ├── Events/                    # Revit event handlers
│   │   └── Lifecycle/                 # Plugin lifecycle management
│   │
│   ├── Graph/                         # GraphDB & Ontology layer
│   │   ├── OntologyManager.cs         # Ontology schema management
│   │   ├── GraphDBConnector.cs        # Neo4j/GraphDB connection
│   │   ├── BIMNodeMapper.cs           # Revit Element → Graph Node
│   │   └── RelationshipBuilder.cs     # Graph relationship creation
│   │
│   ├── ChangeTracking/                # Model change detection
│   │   ├── ChangeMonitor.cs           # DocumentChanged event handler
│   │   ├── RevisionManager.cs         # Hourly backup system
│   │   ├── DiffCalculator.cs          # Change diff computation
│   │   └── ChangeHistory.cs           # Persistent change log
│   │
│   ├── BIMWorkflow/                   # BIM business processes
│   │   ├── CostEstimation/            # Cost management
│   │   │   ├── BOQGenerator.cs
│   │   │   ├── CBSManager.cs
│   │   │   └── CostCalculator.cs
│   │   ├── Scheduling/                # Schedule management
│   │   │   ├── WBSManager.cs
│   │   │   ├── ProgressTracker.cs
│   │   │   └── MilestoneManager.cs
│   │   └── Quantity/                  # Quantity takeoff
│   │       ├── QuantityExtractor.cs
│   │       └── MaterialAllocator.cs
│   │
│   ├── LLMIntegration/                # AI/LLM features
│   │   ├── LangGraphAgent.cs          # LangGraph integration
│   │   ├── QueryProcessor.cs          # NL query processing
│   │   ├── DocumentGenerator.cs       # Auto document creation
│   │   └── DecisionSupport.cs         # AI-assisted decisions
│   │
│   ├── Models/                        # Data models
│   │   ├── BIMElement.cs
│   │   ├── OntologyNode.cs
│   │   ├── ChangeRecord.cs
│   │   ├── CostItem.cs
│   │   └── ScheduleTask.cs
│   │
│   ├── Services/                      # Business services
│   │   ├── RevitDataExtractor.cs
│   │   ├── ExportService.cs
│   │   └── ReportGenerator.cs
│   │
│   └── UI/                            # User interface
│       ├── MainDockPanel.xaml
│       ├── GraphViewer.xaml
│       ├── ChangeLogPanel.xaml
│       └── QueryInterface.xaml
│
├── tests/                             # Unit & integration tests
├── docs/                              # Documentation
│   ├── ontology/                      # Ontology schemas
│   ├── api/                           # API documentation
│   └── user-guide/                    # User manual
│
├── .claude/                           # Claude Code configuration
│   ├── agents/                        # Project-specific agents
│   ├── skills/                        # Development skills
│   └── hooks/                         # Automation hooks
│
└── AgenticRevit.sln                   # Visual Studio solution
```

---

## Revit 2025 API Integration Points

### Core Events to Handle
```csharp
// Document events
Application.DocumentOpened
Application.DocumentClosing
Application.DocumentSaved

// Model change events
Application.DocumentChanged  // Primary change detection
Document.DocumentUpdated

// Element events
ElementModified
ElementCreated
ElementDeleted
```

### Key API Namespaces
```csharp
using Autodesk.Revit.DB;           // Core database classes
using Autodesk.Revit.UI;           // UI framework
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;   // Command attributes
```

---

## Ontology Strategy

### BIM Ontology Layers

1. **Physical Layer** (Geometry & Location)
   - Elements, Geometry, Coordinates
   - Spatial relationships (contains, adjacent, above/below)

2. **Semantic Layer** (Classification & Properties)
   - Category, Family, Type
   - Parameters (Instance/Type)
   - Materials, Assemblies

3. **Process Layer** (Workflow & Time)
   - WBS tasks, CBS items
   - Schedule, Progress
   - Cost allocation

4. **Document Layer** (Information & Reference)
   - Drawings, Specifications
   - Contracts, BOQ
   - Change records

### Graph Schema Example
```cypher
// Node types
(:Element {id, category, family, type, parameters...})
(:Space {name, level, area, volume...})
(:Task {wbs_code, name, start, end, progress...})
(:Cost {cbs_code, amount, unit...})

// Relationships
(element)-[:LOCATED_IN]->(space)
(element)-[:ASSIGNED_TO]->(task)
(element)-[:HAS_COST]->(cost)
(task)-[:DEPENDS_ON]->(task)
```

---

## Change Tracking System

### Hourly Backup Strategy
```
Backups/
├── 2025-01-09/
│   ├── backup_09-00.json    # 9:00 AM checkpoint
│   ├── backup_10-00.json    # 10:00 AM checkpoint
│   └── ...
├── changes/
│   ├── change_log.json      # Continuous change log
│   └── diffs/               # Element-level diffs
```

### Change Record Structure
```csharp
public class ChangeRecord
{
    public DateTime Timestamp { get; set; }
    public string ElementId { get; set; }
    public ChangeType Type { get; set; }  // Created, Modified, Deleted
    public Dictionary<string, object> BeforeState { get; set; }
    public Dictionary<string, object> AfterState { get; set; }
    public string UserId { get; set; }
}
```

---

## Development Guidelines

### Code Style
- Follow Microsoft C# coding conventions
- Use XML documentation for public APIs
- Implement IDisposable for resource management
- Use async/await for long-running operations

### Error Handling
- Comprehensive try-catch with logging
- Graceful degradation strategies
- User-friendly error messages

### Testing Requirements
- Unit tests for business logic
- Integration tests for Revit API calls
- Mock Revit API for isolated testing

---

## Build & Deployment

### Prerequisites
- Visual Studio 2022+
- Revit 2025 SDK
- .NET Framework 4.8
- Neo4j (optional, for GraphDB)

### Build Commands
```bash
# Build solution
dotnet build AgenticRevit.sln

# Run tests
dotnet test

# Create release package
dotnet publish -c Release
```

### Plugin Registration
- Copy DLL to `%APPDATA%\Autodesk\Revit\Addins\2025\`
- Register `.addin` manifest file

---

## Current Development Status

### Phase 1: Foundation (Current)
- [ ] Project structure setup
- [ ] Core plugin infrastructure
- [ ] Basic Revit event handling
- [ ] Change tracking prototype

### Phase 2: GraphDB Integration
- [ ] Ontology schema design
- [ ] Neo4j connector
- [ ] Element-to-node mapping
- [ ] Relationship builder

### Phase 3: BIM Workflow
- [ ] CBS/WBS implementation
- [ ] Cost estimation module
- [ ] Quantity takeoff
- [ ] Progress tracking

### Phase 4: LLM Integration
- [ ] LangGraph setup
- [ ] Query processing
- [ ] Document generation
- [ ] Decision support

---

## Key Contacts & Resources

- **Revit API Documentation**: https://www.revitapidocs.com/
- **Neo4j Documentation**: https://neo4j.com/docs/
- **LangGraph**: https://langchain-ai.github.io/langgraph/

---

## Memory Safe for Clear Command

This document contains all essential project information. All configurations and code are stored locally. Development can be resumed at any time by referencing this file.
