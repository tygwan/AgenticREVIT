# AgenticREVIT PRD (Product Requirements Document)

**Version**: 1.0
**Last Updated**: 2025-01-09
**Author**: Development Team

---

## Overview

### Project Name
AgenticREVIT - Agentic BIM Intelligence Plugin for Autodesk Revit 2025

### Purpose
BIM 프로젝트 데이터를 GraphDB/Ontology로 구조화하고, LLM을 연동하여 지능형 BIM 업무 자동화를 지원하는 Revit 플러그인

### Target Users
- BIM 매니저 및 코디네이터
- 건설 프로젝트 관리자
- 물량/원가 담당자
- 공정 관리 담당자

---

## Requirements

### Functional Requirements

#### P0 (Critical - v0.1.0)
| ID | Feature | Description | Status |
|----|---------|-------------|--------|
| FR-001 | Real-time Change Tracking | 모델 변경사항 실시간 감지 및 기록 | ✅ Complete |
| FR-002 | Hourly Backup System | 1시간 단위 자동 백업 체크포인트 | ✅ Complete |
| FR-003 | Ontology Manager | BIM 데이터의 그래프 구조화 | ✅ Complete |
| FR-004 | Neo4j Integration | GraphDB 연동 기본 구조 | ✅ Complete |

#### P1 (High Priority - v0.2.0)
| ID | Feature | Description | Status |
|----|---------|-------------|--------|
| FR-005 | CBS Management | 비용 분류체계 관리 | Planned |
| FR-006 | WBS Management | 작업 분류체계 관리 | Planned |
| FR-007 | Element-Task Linking | BIM 요소와 작업 연결 | Planned |

#### P2 (Medium Priority - v0.3.0)
| ID | Feature | Description | Status |
|----|---------|-------------|--------|
| FR-008 | Quantity Takeoff | BIM 기반 물량 산출 | Planned |
| FR-009 | BOQ Generation | 물량내역서 자동 생성 | Planned |
| FR-010 | Cost Integration | 단가 연동 및 원가 계산 | Planned |

#### P3 (Future - v0.4.0+)
| ID | Feature | Description | Status |
|----|---------|-------------|--------|
| FR-011 | Progress Tracking | 공정률/기성 관리 | Planned |
| FR-012 | LLM Integration | 자연어 쿼리 및 문서 생성 | Planned |
| FR-013 | Dashboard UI | 시각화 대시보드 | Planned |

### Non-Functional Requirements

#### Performance
| ID | Requirement | Target |
|----|-------------|--------|
| NFR-001 | Change detection latency | < 500ms |
| NFR-002 | Graph query response | < 2s for 10K elements |
| NFR-003 | Backup creation time | < 30s |
| NFR-004 | Memory overhead | < 500MB |

#### Security
| ID | Requirement | Description |
|----|-------------|-------------|
| NFR-005 | Neo4j Authentication | 암호화된 연결 필수 |
| NFR-006 | Local Data Storage | 민감 정보 로컬 저장 |
| NFR-007 | API Key Management | 환경변수 기반 관리 |

#### Compatibility
| ID | Requirement | Target |
|----|-------------|--------|
| NFR-008 | Revit Version | 2025 |
| NFR-009 | .NET Framework | 4.8 |
| NFR-010 | Neo4j Version | 5.x |
| NFR-011 | Windows OS | 10/11 x64 |

---

## User Stories

### Change Tracking
```
AS A BIM 매니저
I WANT TO 모델 변경사항을 실시간으로 추적하고 싶다
SO THAT 누가, 언제, 무엇을 변경했는지 파악할 수 있다
```

### Revision Management
```
AS A 프로젝트 관리자
I WANT TO 1시간 단위로 자동 백업되길 원한다
SO THAT 문제 발생 시 특정 시점으로 복원할 수 있다
```

### Graph Query
```
AS A BIM 코디네이터
I WANT TO 그래프 쿼리로 요소 관계를 검색하고 싶다
SO THAT 복잡한 BIM 데이터에서 원하는 정보를 빠르게 찾을 수 있다
```

---

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Change Detection Accuracy | > 99% | 누락된 변경사항 비율 |
| User Adoption Rate | > 70% | 설치 후 활성 사용자 비율 |
| Query Performance | < 2s | 평균 응답 시간 |
| System Stability | > 99.5% | 크래시 없는 세션 비율 |

---

## Dependencies

### External
- Autodesk Revit 2025 API
- Neo4j.Driver 5.15.0
- Serilog 3.1.1
- Newtonsoft.Json 13.0.3

### Internal
- ChangeMonitor ← OntologyManager
- OntologyManager ← GraphDBConnector
- RevisionManager ← ChangeMonitor

---

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Revit API 변경 | High | Low | 버전별 어댑터 패턴 적용 |
| 대용량 모델 성능 | Medium | Medium | 증분 처리 및 캐싱 |
| Neo4j 연결 실패 | Medium | Low | 로컬 캐시 및 재연결 로직 |
