# AgenticREVIT Technical Specification

**Version**: 1.0
**Last Updated**: 2025-01-09

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Revit 2025 Host                          │
├─────────────────────────────────────────────────────────────┤
│  AgenticRevitPlugin (IExternalApplication)                  │
│  ├── Event Handlers (DocumentChanged, DocumentOpened, etc) │
│  └── Module Coordinator                                     │
├─────────────────────────────────────────────────────────────┤
│  Core Modules                                               │
│  ├── ChangeMonitor        → Real-time change detection     │
│  ├── RevisionManager      → Hourly backup system           │
│  └── OntologyManager      → Graph structure management     │
├─────────────────────────────────────────────────────────────┤
│  Graph Layer                                                │
│  ├── GraphDBConnector     → Neo4j connection               │
│  └── OntologyNode/Rel     → Node & Relationship models     │
├─────────────────────────────────────────────────────────────┤
│                    Neo4j GraphDB                            │
└─────────────────────────────────────────────────────────────┘
```

---

## Technology Stack

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| Platform | Autodesk Revit | 2025 | BIM Host Application |
| Runtime | .NET Framework | 4.8 | Plugin Runtime |
| UI | WPF + WinForms | - | User Interface |
| Graph DB | Neo4j | 5.x | Ontology Storage |
| Logging | Serilog | 3.1.1 | Structured Logging |
| JSON | Newtonsoft.Json | 13.0.3 | Serialization |

---

## Component Details

### 1. AgenticRevitPlugin

**Location**: `src/Core/AgenticRevitPlugin.cs`

**Interface**: `IExternalApplication`

**Lifecycle**:
```
OnStartup() → Initialize modules → Subscribe events
    ↓
OnDocumentOpened() → Build initial graph → Start monitoring
    ↓
OnDocumentChanged() → Process changes → Update graph
    ↓
OnShutdown() → Cleanup → Dispose resources
```

**Key Methods**:
- `OnStartup(UIControlledApplication)`: 플러그인 초기화
- `OnShutdown(UIControlledApplication)`: 리소스 정리
- `OnDocumentChanged(...)`: 변경사항 처리

### 2. ChangeMonitor

**Location**: `src/ChangeTracking/ChangeMonitor.cs`

**Purpose**: 모델 변경사항 실시간 감지 및 기록

**Data Flow**:
```
DocumentChanged Event
    ↓
ProcessChanges(added, modified, deleted)
    ↓
CreateChangeRecord() → Queue
    ↓
ElementsChanged Event → OntologyManager
```

**Key Data Structures**:
```csharp
public class ChangeRecord
{
    public string ElementId { get; set; }
    public string UniqueId { get; set; }
    public ChangeType ChangeType { get; set; }  // Created, Modified, Deleted
    public DateTime Timestamp { get; set; }
    public string Category { get; set; }
    public Dictionary<string, object?> CurrentState { get; set; }
}
```

### 3. RevisionManager

**Location**: `src/ChangeTracking/RevisionManager.cs`

**Purpose**: 1시간 단위 자동 백업

**Timer Logic**:
```
StartAutoBackup() → Timer(1 hour)
    ↓
OnTimerElapsed() → CreateCheckpoint()
    ↓
SerializeState() → SaveToFile()
    ↓
CheckpointCreated Event
```

**Backup Structure**:
```
[ProjectName]_backups/
├── checkpoint_20250109_140000.json
├── checkpoint_20250109_150000.json
└── checkpoint_20250109_160000.json
```

### 4. OntologyManager

**Location**: `src/Graph/OntologyManager.cs`

**Purpose**: BIM 데이터를 그래프 구조로 관리

**Node Types**:
| Type | Description | Key Properties |
|------|-------------|----------------|
| ElementNode | BIM 요소 | RevitElementId, Category, Family, Type |
| SpatialNode | 공간 요소 | SpatialType, Level, Area, Volume |
| TaskNode | WBS 작업 | Name, StartDate, EndDate, Progress |
| CostNode | CBS 비용 | Code, UnitCost, Quantity |
| DocumentNode | 문서 참조 | DocumentType, FilePath |

**Relationship Types**:
| Relationship | From → To | Description |
|--------------|-----------|-------------|
| LOCATED_IN | Element → Spatial | 요소의 공간 위치 |
| HOSTED_BY | Element → Element | 호스트 관계 |
| BELONGS_TO | Element → Task | WBS 연결 |
| HAS_COST | Element → Cost | CBS 연결 |

### 5. GraphDBConnector

**Location**: `src/Graph/GraphDBConnector.cs`

**Purpose**: Neo4j 연결 및 Cypher 쿼리 실행

**Connection**:
```csharp
public async Task<bool> ConnectAsync(string uri, string user, string password)
{
    _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    await _driver.VerifyConnectivityAsync();
}
```

**Key Operations**:
- `CreateNodeAsync<T>(T node)`: 노드 생성
- `UpdateNodeAsync<T>(string id, T node)`: 노드 업데이트
- `CreateRelationshipAsync(...)`: 관계 생성
- `ExecuteQueryAsync(string cypher)`: 쿼리 실행

---

## Data Models

### OntologyNode Base
```csharp
public abstract class OntologyNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public Dictionary<string, object> Properties { get; set; }
}
```

### ElementNode
```csharp
public class ElementNode : OntologyNode
{
    public string RevitElementId { get; set; }
    public string UniqueId { get; set; }
    public string Category { get; set; }
    public string Family { get; set; }
    public string Type { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Z { get; set; }
}
```

---

## API Interfaces

### IChangeMonitor
```csharp
public interface IChangeMonitor : IDisposable
{
    event EventHandler<ElementChangedEventArgs> ElementsChanged;
    void StartMonitoring(Document document);
    void StopMonitoring(Document document);
    IReadOnlyList<ChangeRecord> GetChanges(Document document);
}
```

### IOntologyManager
```csharp
public interface IOntologyManager : IDisposable
{
    event EventHandler<GraphUpdatedEventArgs> GraphUpdated;
    void BuildInitialGraph(Document document);
    void UpdateGraph(Document doc, ICollection<ElementId> added,
                     ICollection<ElementId> modified, ICollection<ElementId> deleted);
    IReadOnlyList<T> GetNodes<T>() where T : OntologyNode;
}
```

---

## Deployment

### Build Output
```
bin/Release/
├── AgenticRevit.dll          # Main plugin
├── AgenticRevit.pdb          # Debug symbols
├── Neo4j.Driver.dll          # Neo4j client
├── Serilog.dll               # Logging
├── Serilog.Sinks.File.dll    # File sink
├── Newtonsoft.Json.dll       # JSON
└── [System dependencies]
```

### Installation Path
```
%APPDATA%\Autodesk\Revit\Addins\2025\
├── AgenticRevit.addin        # Manifest
├── AgenticRevit.dll          # Plugin
└── [Dependencies]
```

### .addin Manifest
```xml
<RevitAddIns>
  <AddIn Type="Application">
    <Assembly>AgenticRevit.dll</Assembly>
    <FullClassName>AgenticRevit.Core.AgenticRevitPlugin</FullClassName>
    <AddInId>A1B2C3D4-E5F6-7890-ABCD-EF1234567890</AddInId>
    <Name>AgenticREVIT</Name>
    <VendorId>AgenticBIM</VendorId>
  </AddIn>
</RevitAddIns>
```

---

## Performance Considerations

### Memory Optimization
- ConcurrentDictionary for thread-safe caching
- Lazy loading for large graph structures
- Periodic cache cleanup

### Query Optimization
- Batch operations for multiple elements
- Index creation on frequently queried properties
- Connection pooling for Neo4j

### Event Handling
- Debouncing for rapid change events
- Background processing for heavy operations
- Transaction batching for graph updates

---

## Security Measures

### Neo4j Connection
- TLS/SSL encryption required
- Credentials stored in environment variables
- Connection timeout and retry logic

### Local Storage
- Backup files in user-specific directory
- No sensitive data in logs
- Automatic cleanup of old backups
