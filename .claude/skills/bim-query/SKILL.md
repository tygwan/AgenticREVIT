---
name: bim-query
description: BIM 데이터 조회 및 분석 쿼리 작성 스킬
triggers: ["쿼리", "query", "조회", "검색", "분석"]
tools: [Read, Grep, Glob, Bash]
---

# BIM Query Skill

## Purpose
BIM 그래프 데이터베이스에 대한 쿼리를 작성하고 최적화합니다.

## Query Categories

### 1. Spatial Queries (공간 조회)

```cypher
// 특정 레벨의 모든 요소
MATCH (e:Element)-[:LOCATED_IN]->(l:Spatial {spatialType: 'Level', name: $levelName})
RETURN e

// 특정 공간의 요소 수
MATCH (e:Element)-[:LOCATED_IN]->(r:Spatial {spatialType: 'Room', number: $roomNumber})
RETURN COUNT(e) as elementCount

// 인접 공간 찾기
MATCH (r1:Spatial {id: $roomId})-[:ADJACENT_TO]->(r2:Spatial)
RETURN r2
```

### 2. Element Queries (요소 조회)

```cypher
// 카테고리별 요소 수
MATCH (e:Element)
RETURN e.category, COUNT(e) as count
ORDER BY count DESC

// 특정 패밀리의 모든 인스턴스
MATCH (e:Element {family: $familyName})
RETURN e

// 호스팅 관계 조회
MATCH (e:Element)-[:HOSTED_BY]->(h:Element)
WHERE h.id = $hostId
RETURN e
```

### 3. Cost Queries (비용 조회)

```cypher
// CBS 코드별 비용 집계
MATCH (c:Cost)
WHERE c.cbsCode STARTS WITH $cbsPrefix
RETURN c.cbsCode, SUM(c.totalCost) as total
ORDER BY c.cbsCode

// 카테고리별 비용
MATCH (e:Element)-[:HAS_COST]->(c:Cost)
RETURN e.category, SUM(c.totalCost) as totalCost
ORDER BY totalCost DESC

// 예산 대비 실적
MATCH (c:Cost)
WHERE c.cbsCode = $cbsCode
RETURN c.budget, c.actual, (c.actual / c.budget * 100) as percentage
```

### 4. Progress Queries (진척 조회)

```cypher
// 전체 프로젝트 진척률
MATCH (t:Task)
RETURN AVG(t.progress) as overallProgress

// 지연 작업 조회
MATCH (t:Task)
WHERE t.plannedEnd < date() AND t.progress < 100
RETURN t.wbsCode, t.taskName, t.progress, t.plannedEnd

// 이번 주 완료 예정 작업
MATCH (t:Task)
WHERE t.plannedEnd >= date() AND t.plannedEnd <= date() + duration('P7D')
RETURN t
```

### 5. Relationship Queries (관계 조회)

```cypher
// 요소의 모든 관계 조회
MATCH (e:Element {id: $elementId})-[r]-(n)
RETURN type(r) as relationType, n

// 작업에 할당된 요소들
MATCH (e:Element)-[:ASSIGNED_TO]->(t:Task {wbsCode: $wbsCode})
RETURN e

// 연결된 요소 체인
MATCH path = (e1:Element {id: $startId})-[:CONNECTED_TO*1..5]->(e2:Element)
RETURN path
```

## Query Optimization Tips

1. **인덱스 활용**: WHERE 절에서 인덱스된 속성 사용
2. **LIMIT 사용**: 대량 결과 시 LIMIT으로 제한
3. **프로파일링**: PROFILE 키워드로 쿼리 성능 분석
4. **파라미터화**: 하드코딩 대신 $parameter 사용

## Common Patterns

### Aggregation
```cypher
MATCH (e:Element)
WITH e.category as category, COUNT(e) as count
WHERE count > 10
RETURN category, count
```

### Path Finding
```cypher
MATCH path = shortestPath((e1:Element)-[*]-(e2:Element))
WHERE e1.id = $startId AND e2.id = $endId
RETURN path
```

### Conditional Return
```cypher
MATCH (t:Task)
RETURN t.taskName,
       CASE
         WHEN t.progress >= 100 THEN 'Completed'
         WHEN t.progress >= 50 THEN 'In Progress'
         ELSE 'Not Started'
       END as status
```
