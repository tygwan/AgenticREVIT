# AgenticREVIT Context Summary

**Purpose**: AI 세션 컨텍스트 최적화를 위한 빠른 참조 문서
**Integration**: `context-optimizer` skill과 연동
**Last Updated**: 2025-01-09

---

## Quick Reference

**AgenticREVIT**는 Revit 2025용 BIM 지능화 플러그인입니다. GraphDB/Ontology로 BIM 데이터를 구조화하고, 실시간 변경 추적, 1시간 백업, 향후 LLM 연동을 지원합니다. 현재 Phase 1 (Foundation)이 완료되었으며, Phase 2 (GraphDB Integration) 진행 예정입니다.

### Key Paths
```
AgenticRevit/
├── src/Core/AgenticRevitPlugin.cs     # Main entry point
├── src/ChangeTracking/                 # Change tracking modules
├── src/Graph/                          # Ontology & GraphDB
├── src/Models/                         # Data models
└── docs/                               # Development docs
```

### Critical Dependencies
- Revit 2025 API (RevitAPI.dll, RevitAPIUI.dll)
- Neo4j.Driver 5.15.0
- .NET Framework 4.8

---

## Architecture Snapshot

**Core Components**:
- `AgenticRevitPlugin` → IExternalApplication, event handling
- `ChangeMonitor` → Real-time change detection
- `RevisionManager` → Hourly backups
- `OntologyManager` → Graph structure
- `GraphDBConnector` → Neo4j connection

**Data Flow**:
```
Revit Model → ChangeMonitor → OntologyManager → GraphDBConnector → Neo4j
```

**Event Chain**:
```
DocumentChanged → ProcessChanges → UpdateGraph → SyncToGraphDB
```

---

## Current Focus

### Phase 2: GraphDB Integration
- Neo4j 연결 구현
- Cypher 쿼리 작성
- 그래프 동기화 테스트

### Recent Changes (2025-01-09)
- Phase 1 완료 (Plugin, ChangeTracking, OntologyManager)
- 빌드 설정 및 자동 배포 구성
- 개발 문서 작성

---

## Token Optimization

### Essential Files (Context Loading)
```
✅ Load these for most sessions:
- docs/CONTEXT.md (this file)
- docs/PROGRESS.md (current status)
- src/Core/AgenticRevitPlugin.cs (entry point)
```

### Conditional Loading
```
📋 Load when working on:
- Change tracking → src/ChangeTracking/*.cs
- Graph/Ontology → src/Graph/*.cs
- Data models → src/Models/*.cs
```

### Excludable Paths
```
❌ Skip for token savings:
- bin/, obj/ (build outputs)
- .vs/ (IDE settings)
- *.pdb, *.dll (binaries)
```

---

## Session Continuity

### Resume Checklist
1. Read `docs/PROGRESS.md` for current status
2. Check recent commits: `git log --oneline -5`
3. Review active tasks in PROGRESS.md

### Key Decisions Made
- Using Neo4j for GraphDB (not alternatives)
- 1-hour backup interval (configurable)
- Ontology-based BIM structure
- .NET Framework 4.8 (Revit 2025 requirement)

### Pending Decisions
- LLM provider selection (OpenAI/Anthropic/Local)
- Dashboard framework (WPF/Web-based)
- Testing framework selection

---

## Quick Commands

### Build
```bash
cd AgenticRevit
dotnet build src/AgenticRevit.csproj -c Release
```

### Deploy Location
```
%APPDATA%\Autodesk\Revit\Addins\2025\
```

### Git
```bash
git status
git log --oneline -5
git push origin main
```

---

## Terminology

| Term | Definition |
|------|------------|
| CBS | Cost Breakdown Structure - 비용 분류체계 |
| WBS | Work Breakdown Structure - 작업 분류체계 |
| BOQ | Bill of Quantities - 물량내역서 |
| Ontology | 개념 및 관계의 구조화된 표현 |
| ElementNode | Revit 요소를 나타내는 그래프 노드 |
| SpatialNode | 공간(Level, Room)을 나타내는 노드 |
