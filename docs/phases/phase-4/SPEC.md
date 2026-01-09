# Phase 4: LLM Integration

**Status**: ⏳ Planned
**Dependencies**: Phase 2, Phase 3
**Last Updated**: 2025-01-09

---

## Scope

- [ ] LangGraph/LangChain 연동
- [ ] 자연어 쿼리 인터페이스
- [ ] 문서 자동 생성
- [ ] 의사결정 지원 프롬프트
- [ ] 컨텍스트 관리

## Technical Details

### Architecture Changes

```
src/LLMIntegration/
├── LangGraphAgent.cs
├── QueryProcessor.cs
├── DocumentGenerator.cs
├── DecisionSupport.cs
└── ContextManager.cs
```

### Key Components

| Component | File | Purpose |
|-----------|------|---------|
| LangGraphAgent | `src/LLMIntegration/LangGraphAgent.cs` | LangGraph 에이전트 |
| QueryProcessor | `src/LLMIntegration/QueryProcessor.cs` | 자연어 쿼리 처리 |
| DocumentGenerator | `src/LLMIntegration/DocumentGenerator.cs` | 문서 자동 생성 |
| DecisionSupport | `src/LLMIntegration/DecisionSupport.cs` | 의사결정 지원 |

### API Integration

```csharp
public interface IQueryProcessor
{
    Task<QueryResult> ProcessQueryAsync(string naturalLanguageQuery);
    Task<string> GenerateCypherAsync(string query);
}

public interface IDocumentGenerator
{
    Task<string> GenerateReportAsync(ReportType type, object data);
    Task<string> SummarizeChangesAsync(DateTime from, DateTime to);
}
```

## Files to Create

| File | Action | Description |
|------|--------|-------------|
| `src/LLMIntegration/LangGraphAgent.cs` | Create | LangGraph 연동 |
| `src/LLMIntegration/QueryProcessor.cs` | Create | 쿼리 처리 |
| `src/LLMIntegration/DocumentGenerator.cs` | Create | 문서 생성 |

## Dependencies

### External
- LangChain/LangGraph SDK
- OpenAI/Anthropic API

### Internal
- GraphDBConnector (Phase 2)
- BIM Workflow data (Phase 3)

## Acceptance Criteria

- [ ] 자연어로 BIM 데이터 쿼리
- [ ] 보고서 자동 생성
- [ ] 변경 사항 요약
- [ ] 의사결정 제안

---

**Related**: [TASKS.md](./TASKS.md) | [CHECKLIST.md](./CHECKLIST.md)
**Parent**: [PROGRESS.md](../../PROGRESS.md)
