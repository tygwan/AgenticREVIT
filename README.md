# AgenticREVIT

**Agentic BIM Intelligence Plugin for Autodesk Revit 2025**

BIM 프로젝트 데이터를 GraphDB/Ontology로 구조화하고, LLM을 연동하여 지능형 BIM 업무 자동화를 지원하는 Revit 플러그인입니다.

---

## Features

### Core Features (v0.1.0)

| Feature | Description | Status |
|---------|-------------|--------|
| **Real-time Change Tracking** | 모델 변경사항 실시간 감지 및 기록 | :white_check_mark: 구현완료 |
| **Hourly Backup System** | 1시간 단위 자동 백업 체크포인트 | :white_check_mark: 구현완료 |
| **Ontology Manager** | BIM 데이터의 그래프 구조화 | :white_check_mark: 구현완료 |
| **Neo4j Connector** | GraphDB 연동 인터페이스 | :white_check_mark: 구현완료 |
| **Plugin Infrastructure** | Revit 2025 플러그인 아키텍처 | :white_check_mark: 구현완료 |
| **Auto Deployment** | 빌드 후 자동 배포 | :white_check_mark: 구현완료 |

### Planned Features

| Feature | Description | Status |
|---------|-------------|--------|
| **CBS/WBS Management** | 비용/작업 분류체계 관리 | :construction: 계획됨 |
| **Quantity Takeoff (BOQ)** | BIM 기반 물량 산출 | :construction: 계획됨 |
| **Progress Tracking** | 공정률/기성 관리 | :construction: 계획됨 |
| **LLM Integration** | 자연어 쿼리 및 문서 생성 | :construction: 계획됨 |
| **Dashboard UI** | WPF 기반 대시보드 | :construction: 계획됨 |

---

## Architecture

```
AgenticRevit/
├── src/
│   ├── Core/                      # Plugin infrastructure
│   │   ├── AgenticRevitPlugin.cs  # IExternalApplication entry point
│   │   ├── Commands/              # IExternalCommand implementations
│   │   └── Events/                # Event arguments & handlers
│   │
│   ├── ChangeTracking/            # Model change detection
│   │   ├── ChangeMonitor.cs       # Real-time change monitoring
│   │   └── RevisionManager.cs     # Hourly backup system
│   │
│   ├── Graph/                     # GraphDB & Ontology
│   │   ├── OntologyManager.cs     # Graph structure management
│   │   └── GraphDBConnector.cs    # Neo4j connection
│   │
│   ├── Models/                    # Data models
│   │   ├── BIMElement.cs          # BIM element representation
│   │   ├── OntologyNode.cs        # Graph node types
│   │   └── ChangeRecord.cs        # Change tracking records
│   │
│   ├── BIMWorkflow/               # Business processes (planned)
│   ├── LLMIntegration/            # AI features (planned)
│   └── UI/                        # User interface (planned)
│
├── .claude/                       # Claude Code configuration
│   ├── agents/                    # Project-specific agents
│   ├── skills/                    # Development skills
│   └── hooks/                     # Automation hooks
│
└── docs/                          # Documentation
```

---

## Ontology Schema

### Node Types

```cypher
// Element - BIM 요소
(:Element {
  id, revitElementId, uniqueId,
  category, family, type,
  x, y, z
})

// Spatial - 공간 (Room, Level, Space)
(:Spatial {
  id, spatialType, name, number,
  area, volume, level
})

// Task - WBS 작업
(:Task {
  id, wbsCode, taskName,
  plannedStart, plannedEnd,
  progress, status
})

// Cost - CBS 비용
(:Cost {
  id, cbsCode, category,
  unitCost, quantity, totalCost
})
```

### Relationships

| Relationship | Description |
|--------------|-------------|
| `LOCATED_IN` | Element → Spatial (위치) |
| `CONTAINS` | Spatial → Element (포함) |
| `HOSTED_BY` | Element → Element (호스팅) |
| `ASSIGNED_TO` | Element → Task (작업 할당) |
| `HAS_COST` | Element → Cost (비용 연결) |
| `DEPENDS_ON` | Task → Task (선후행) |

---

## Requirements

- **Autodesk Revit 2025**
- **.NET Framework 4.8**
- **Visual Studio 2022+** (개발 시)
- **Neo4j** (선택, GraphDB 사용 시)

---

## Installation

### Option 1: Build from Source

```bash
# Clone repository
git clone https://github.com/tygwan/AgenticREVIT.git
cd AgenticRevit

# Build (자동 배포됨)
dotnet build src/AgenticRevit.csproj -c Release
```

빌드 완료 후 자동으로 다음 경로에 배포됩니다:
```
%APPDATA%\Autodesk\Revit\Addins\2025\
```

### Option 2: Manual Installation

1. [Releases](https://github.com/tygwan/AgenticREVIT/releases)에서 최신 버전 다운로드
2. 압축 해제 후 모든 파일을 복사:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```
3. Revit 2025 재시작

---

## Usage

### Revit에서 플러그인 실행

1. Revit 2025 실행
2. **AgenticREVIT** 탭 선택
3. 기능 버튼 클릭:
   - **Dashboard**: 메인 대시보드
   - **Graph Viewer**: 온톨로지 그래프 시각화
   - **Backup Manager**: 백업 관리
   - **AI Query**: LLM 기반 자연어 쿼리

### Change Tracking

모델이 열리면 자동으로 변경 추적이 시작됩니다:
- 요소 생성/수정/삭제 실시간 감지
- 1시간마다 자동 백업 생성
- 백업 간 차이 비교 가능

---

## Development Roadmap

### Phase 1: Foundation (Current) :white_check_mark:
- [x] Plugin architecture
- [x] Change tracking system
- [x] Hourly backup mechanism
- [x] Ontology manager
- [x] Neo4j connector
- [x] Auto deployment

### Phase 2: GraphDB Integration
- [ ] Full Neo4j integration
- [ ] Cypher query optimization
- [ ] Graph visualization
- [ ] Relationship builder

### Phase 3: BIM Workflow
- [ ] CBS (Cost Breakdown Structure)
- [ ] WBS (Work Breakdown Structure)
- [ ] BOQ (Bill of Quantities)
- [ ] Progress tracking
- [ ] Cost estimation

### Phase 4: LLM Integration
- [ ] LangGraph agent setup
- [ ] Natural language query processing
- [ ] Document auto-generation
- [ ] Decision support system

### Phase 5: UI/UX
- [ ] WPF Dashboard
- [ ] Graph visualization panel
- [ ] Change history viewer
- [ ] Query interface

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Platform | Autodesk Revit 2025 |
| Language | C# (.NET Framework 4.8) |
| UI Framework | WPF |
| Graph Database | Neo4j |
| Serialization | Newtonsoft.Json |
| Logging | Serilog |
| AI/LLM | LangGraph (planned) |

---

## Project Structure

```
AgenticRevit/
├── src/                    # Source code
├── tests/                  # Unit tests
├── docs/                   # Documentation
│   ├── ontology/          # Ontology schemas
│   ├── api/               # API documentation
│   └── user-guide/        # User manual
├── .claude/               # Claude Code config
├── AgenticRevit.sln       # VS Solution
├── AgenticRevit.addin     # Revit manifest
└── README.md
```

---

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'feat: add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

### Commit Convention

```
feat: 새로운 기능
fix: 버그 수정
docs: 문서 수정
refactor: 리팩토링
test: 테스트 추가/수정
chore: 빌드, 설정 변경
```

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Contact

- **GitHub**: [@tygwan](https://github.com/tygwan)
- **Repository**: [AgenticREVIT](https://github.com/tygwan/AgenticREVIT)

---

## Acknowledgments

- Autodesk Revit API Documentation
- Neo4j Graph Database
- LangChain/LangGraph Community
