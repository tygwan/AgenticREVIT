# AgenticREVIT

**Agentic BIM Intelligence Plugin for Autodesk Revit 2025**

BIM 프로젝트 데이터를 GraphDB/Ontology로 구조화하고, LLM을 연동하여 지능형 BIM 업무 자동화를 지원하는 Revit 플러그인입니다.

---

## Features

### Current (v0.1.0)

- **Real-time Change Tracking** - 모델 변경사항 실시간 감지 및 기록
- **Hourly Backup System** - 1시간 단위 자동 백업 체크포인트
- **Ontology Manager** - BIM 데이터의 그래프 구조화
- **Neo4j Integration** - GraphDB 연동

### Planned

- **CBS/WBS Management** - 비용/작업 분류체계 관리
- **Quantity Takeoff (BOQ)** - BIM 기반 물량 산출
- **Progress Tracking** - 공정률/기성 관리
- **LLM Integration** - 자연어 쿼리 및 문서 자동 생성

---

## Requirements

- Autodesk Revit 2025
- .NET Framework 4.8
- Neo4j (선택)

---

## Installation

### Build from Source

```bash
git clone https://github.com/tygwan/AgenticREVIT.git
cd AgenticRevit
dotnet build src/AgenticRevit.csproj -c Release
```

빌드 시 자동으로 `%APPDATA%\Autodesk\Revit\Addins\2025\`에 배포됩니다.

---

## Development Roadmap

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Foundation (Plugin, Change Tracking, Backup) | :white_check_mark: Complete |
| 2 | GraphDB Integration | :construction: In Progress |
| 3 | BIM Workflow (CBS/WBS/BOQ) | Planned |
| 4 | LLM Integration | Planned |
| 5 | Dashboard UI | Planned |

---

## Tech Stack

- **Platform**: Autodesk Revit 2025
- **Language**: C# (.NET Framework 4.8)
- **Graph Database**: Neo4j
- **AI/LLM**: LangGraph (planned)

---

## License

MIT License

---

## Contact

- **GitHub**: [@tygwan](https://github.com/tygwan)
