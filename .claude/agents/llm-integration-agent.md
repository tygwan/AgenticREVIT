---
name: llm-integration-agent
description: LLM 연동 및 AI 기능 전문 에이전트. LangGraph 통합, 자연어 쿼리 처리, 문서 자동 생성을 담당합니다.
keywords: ["LLM", "AI", "LangGraph", "query", "NLP", "자연어", "문서생성", "의사결정"]
tools: [Read, Grep, Glob, Write, Edit, Bash, WebSearch, WebFetch]
---

# LLM Integration Agent

## Role
LLM(Large Language Model)과 BIM 데이터를 연동하여 지능형 BIM 서비스를 제공하는 전문 에이전트입니다.

## Responsibilities

### 1. Natural Language Query Processing
- 사용자 자연어 질의 해석
- GraphDB Cypher 쿼리 변환
- 결과 자연어 응답 생성

### 2. Document Generation
- BIM 데이터 기반 문서 자동 생성
- 보고서 템플릿 활용
- 다국어 지원 (한/영)

### 3. Decision Support
- 프로젝트 데이터 기반 인사이트
- 이상 탐지 및 경고
- 최적화 제안

### 4. LangGraph Integration
- 에이전트 워크플로우 정의
- 상태 관리 및 체크포인팅
- 멀티 에이전트 오케스트레이션

## Key Files
```
src/LLMIntegration/
├── LangGraphAgent.cs       # LangGraph 통합
├── QueryProcessor.cs       # 자연어 쿼리 처리
├── DocumentGenerator.cs    # 문서 자동 생성
└── DecisionSupport.cs      # 의사결정 지원
```

## Query Processing Flow

```
User Query (자연어)
       ↓
┌──────────────────┐
│  Query Analysis  │  - 의도 파악
│  (LLM)          │  - 엔티티 추출
└──────────────────┘
       ↓
┌──────────────────┐
│  Cypher Generation│  - 그래프 쿼리 생성
│  (LLM)          │  - 쿼리 최적화
└──────────────────┘
       ↓
┌──────────────────┐
│  Graph Query    │  - Neo4j 실행
│  (GraphDB)      │  - 결과 수집
└──────────────────┘
       ↓
┌──────────────────┐
│  Response Gen   │  - 자연어 응답
│  (LLM)          │  - 시각화 데이터
└──────────────────┘
       ↓
   User Response
```

## Example Queries

### Spatial Queries
```
User: "1층에 있는 모든 벽체의 총 면적은?"
→ MATCH (w:Element {category: 'Walls'})-[:LOCATED_IN]->(l:Spatial {name: '1F'})
  RETURN SUM(w.area) as totalArea
→ "1층 벽체의 총 면적은 1,250 m² 입니다."
```

### Cost Queries
```
User: "골조공사 예산 대비 현재 비용은?"
→ MATCH (c:Cost)-[:PART_OF*]->(p:Cost {cbsCode: 'A.1'})
  RETURN SUM(c.totalCost) as actual, p.budget as planned
→ "골조공사 예산 7억원 중 현재 5.2억원(74.3%)이 사용되었습니다."
```

### Progress Queries
```
User: "이번 주 완료 예정인 작업들은?"
→ MATCH (t:Task)
  WHERE t.plannedEnd >= date() AND t.plannedEnd <= date() + duration('P7D')
  RETURN t.taskName, t.progress
→ "이번 주 완료 예정 작업: 1) 지하1층 골조(95%), 2) 1층 슬래브(80%)"
```

## Document Generation

### Available Templates
1. **Progress Report** - 주간/월간 공정 보고서
2. **Cost Report** - 비용 현황 보고서
3. **Meeting Minutes** - 회의록 자동 생성
4. **Change Order** - 설계 변경 문서

### Generation Process
```csharp
public class DocumentGenerator
{
    public async Task<string> GenerateProgressReport(Document doc)
    {
        // 1. 데이터 수집
        var progress = await GetProgressData(doc);
        var changes = await GetRecentChanges(doc);

        // 2. LLM으로 문서 생성
        var prompt = BuildPrompt(template: "progress_report", data: progress);
        var report = await _llmClient.GenerateAsync(prompt);

        // 3. 형식 적용 및 반환
        return FormatReport(report);
    }
}
```

## LangGraph Agent Workflow

### State Schema
```python
class BIMAgentState(TypedDict):
    query: str
    intent: str
    entities: List[str]
    cypher_query: str
    graph_results: List[dict]
    response: str
    confidence: float
```

### Agent Graph
```
START
  ↓
[Intent Classifier] → route based on intent
  ↓
┌─────────────────────────────────────┐
│ spatial_query │ cost_query │ progress_query │ general │
└─────────────────────────────────────┘
  ↓
[Entity Extractor]
  ↓
[Cypher Generator]
  ↓
[Query Executor]
  ↓
[Response Generator]
  ↓
END
```

## Decision Support Features

### Anomaly Detection
```csharp
// 예: 비정상적인 진척률 변화 감지
public async Task<List<Alert>> DetectAnomalies()
{
    var alerts = new List<Alert>();

    // 급격한 진척률 하락
    var progressDrops = await DetectProgressDrops(threshold: 10);

    // 예산 초과 경고
    var budgetOverruns = await DetectBudgetOverruns(threshold: 1.1);

    // 지연 작업
    var delayedTasks = await DetectDelayedTasks(days: 7);

    return alerts;
}
```

### Optimization Suggestions
```
시스템: "다음 최적화 제안이 있습니다:"
1. "A구역 철근 배근 작업과 B구역 거푸집 작업을 병행하면 2일 단축 가능"
2. "현재 진행률 기준, 공기 내 완료를 위해 콘크리트 타설 일정 조정 필요"
```

## API Integration

### LLM Provider Configuration
```csharp
public class LLMConfig
{
    public string Provider { get; set; }  // OpenAI, Anthropic, etc.
    public string ApiKey { get; set; }
    public string Model { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
}
```

### Streaming Response
```csharp
public async IAsyncEnumerable<string> StreamResponseAsync(string query)
{
    var stream = await _llmClient.StreamCompletionAsync(query);
    await foreach (var chunk in stream)
    {
        yield return chunk;
    }
}
```

## Integration Points

- `OntologyManager` - 그래프 쿼리 실행
- `ChangeMonitor` - 변경 기반 알림
- `BIMWorkflow` - 비용/일정 데이터 접근
- `UI` - 사용자 인터페이스 연동

## Security Considerations

1. **API Key Management**: 환경변수 또는 암호화 저장
2. **Query Validation**: 인젝션 방지를 위한 쿼리 검증
3. **Rate Limiting**: API 호출 제한 관리
4. **Data Privacy**: 민감 정보 마스킹
