---
name: ontology-design
description: BIM 온톨로지 설계 및 스키마 관리 스킬
triggers: ["ontology", "온톨로지", "스키마", "schema", "그래프설계"]
tools: [Read, Write, Grep, Glob]
---

# Ontology Design Skill

## Purpose
BIM 데이터의 온톨로지 스키마를 설계하고 관리합니다.

## Ontology Layers

### 1. Physical Layer (물리 계층)
- Elements (요소)
- Geometry (형상)
- Location (위치)
- Material (재료)

### 2. Semantic Layer (의미 계층)
- Category (범주)
- Family (패밀리)
- Type (유형)
- Parameters (매개변수)

### 3. Process Layer (프로세스 계층)
- WBS (작업분류체계)
- CBS (비용분류체계)
- Schedule (일정)
- Progress (진척)

### 4. Document Layer (문서 계층)
- Contracts (계약)
- Specifications (시방서)
- Drawings (도면)
- Reports (보고서)

## Schema Templates

### Element Node
```cypher
CREATE CONSTRAINT element_id IF NOT EXISTS
FOR (e:Element) REQUIRE e.id IS UNIQUE;

CREATE INDEX element_category IF NOT EXISTS
FOR (e:Element) ON (e.category);

CREATE INDEX element_revitId IF NOT EXISTS
FOR (e:Element) ON (e.revitElementId);
```

### Relationships
```cypher
// Spatial relationships
(e:Element)-[:LOCATED_IN]->(s:Spatial)
(s:Spatial)-[:CONTAINS]->(e:Element)
(e1:Element)-[:ADJACENT_TO]->(e2:Element)

// Structural relationships
(e:Element)-[:HOSTED_BY]->(h:Element)
(e:Element)-[:CONNECTED_TO]->(c:Element)

// Project relationships
(e:Element)-[:ASSIGNED_TO]->(t:Task)
(e:Element)-[:HAS_COST]->(c:Cost)
```

## Design Guidelines

1. **일관성**: 노드 속성명은 camelCase 사용
2. **확장성**: 새로운 노드 타입 추가 용이하도록 설계
3. **성능**: 자주 쿼리되는 속성에 인덱스 생성
4. **무결성**: 필수 관계 정의 및 검증

## Validation Rules

- 모든 Element는 최소 하나의 Spatial에 LOCATED_IN 관계 필요
- Task 노드는 WBSCode가 필수
- Cost 노드는 CBSCode가 필수
