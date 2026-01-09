# Phase 2: GraphDB Integration

**Status**: 🔄 In Progress
**Dependencies**: Phase 1
**Last Updated**: 2025-01-09

---

## Scope

이 Phase에서 구현할 기능 범위:

- [ ] Neo4j 연결 완성 및 테스트
- [ ] Cypher 쿼리 구현
- [ ] 그래프 동기화 로직
- [ ] 연결 재시도/복구
- [ ] 기본 그래프 시각화

## Technical Details

### Architecture Changes

```
src/Graph/
├── GraphDBConnector.cs      # 연결 완성
├── CypherQueryBuilder.cs    # NEW: 쿼리 빌더
├── GraphSyncManager.cs      # NEW: 동기화 관리
└── GraphVisualization.cs    # NEW: 기본 시각화
```

### Key Components

| Component | File | Purpose |
|-----------|------|---------|
| GraphDBConnector | `src/Graph/GraphDBConnector.cs` | Neo4j 연결 완성 |
| CypherQueryBuilder | `src/Graph/CypherQueryBuilder.cs` | Cypher 쿼리 생성 |
| GraphSyncManager | `src/Graph/GraphSyncManager.cs` | 그래프 동기화 |
| ConnectionRetryPolicy | `src/Graph/ConnectionRetryPolicy.cs` | 재연결 정책 |

### Cypher Query Examples

```cypher
// Create element node
CREATE (e:Element {
  revitId: $revitId,
  category: $category,
  family: $family,
  type: $type
})

// Create relationship
MATCH (e:Element {revitId: $elementId})
MATCH (l:Level {name: $levelName})
CREATE (e)-[:LOCATED_IN]->(l)

// Query elements by category
MATCH (e:Element {category: $category})
RETURN e
```

### API/Interfaces

```csharp
public interface IGraphDBConnector
{
    Task<bool> ConnectAsync(string uri, string user, string password);
    Task<T> CreateNodeAsync<T>(T node) where T : OntologyNode;
    Task<bool> CreateRelationshipAsync(string sourceId, string targetId, string type);
    Task<IEnumerable<T>> QueryAsync<T>(string cypher, object parameters);
}

public interface IGraphSyncManager
{
    Task SyncToGraphDBAsync();
    Task<SyncStatus> GetSyncStatusAsync();
    event EventHandler<SyncCompletedEventArgs> SyncCompleted;
}
```

## Files to Create/Modify

| File | Action | Description |
|------|--------|-------------|
| `src/Graph/GraphDBConnector.cs` | Modify | 연결 로직 완성 |
| `src/Graph/CypherQueryBuilder.cs` | Create | Cypher 쿼리 빌더 |
| `src/Graph/GraphSyncManager.cs` | Create | 동기화 관리자 |
| `src/Graph/ConnectionRetryPolicy.cs` | Create | 재연결 정책 |

## Implementation Steps

1. **Neo4j Connection 완성**
   - 연결 테스트 구현
   - 에러 핸들링 강화
   - 연결 상태 모니터링

2. **Cypher Query 구현**
   - 노드 CRUD 쿼리
   - 관계 CRUD 쿼리
   - 검색 쿼리

3. **동기화 로직**
   - 배치 동기화
   - 증분 동기화
   - 충돌 해결

4. **재연결/복구**
   - 지수적 백오프
   - 연결 풀 관리
   - 오프라인 캐싱

## Dependencies

### External
- Neo4j.Driver 5.15.0
- Neo4j Server 5.x

### Internal
- OntologyManager (Phase 1)
- ChangeMonitor events (Phase 1)

## Acceptance Criteria

- [ ] Neo4j 연결 성공/실패 처리
- [ ] 모든 노드 타입 CRUD 동작
- [ ] 관계 생성 및 쿼리
- [ ] 연결 끊김 시 자동 재연결
- [ ] 10K 요소 동기화 < 30s

## Technical Notes

- 비동기 처리 필수 (UI 블로킹 방지)
- 트랜잭션 기반 배치 처리
- 환경변수로 연결 정보 관리

---

**Related**: [TASKS.md](./TASKS.md) | [CHECKLIST.md](./CHECKLIST.md)
**Parent**: [PROGRESS.md](../../PROGRESS.md)
