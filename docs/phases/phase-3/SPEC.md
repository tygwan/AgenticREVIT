# Phase 3: BIM Workflow

**Status**: ⏳ Planned
**Dependencies**: Phase 2
**Last Updated**: 2025-01-09

---

## Scope

- [ ] CBS (Cost Breakdown Structure) 모델
- [ ] WBS (Work Breakdown Structure) 모델
- [ ] Element-Task 연결
- [ ] 물량 산출 알고리즘
- [ ] BOQ 생성

## Technical Details

### Architecture Changes

```
src/BIMWorkflow/
├── CostEstimation/
│   ├── CBSManager.cs
│   ├── BOQGenerator.cs
│   └── CostCalculator.cs
├── Scheduling/
│   ├── WBSManager.cs
│   ├── ProgressTracker.cs
│   └── MilestoneManager.cs
└── Quantity/
    ├── QuantityExtractor.cs
    └── MaterialAllocator.cs
```

### Key Components

| Component | File | Purpose |
|-----------|------|---------|
| CBSManager | `src/BIMWorkflow/CostEstimation/CBSManager.cs` | 비용 분류체계 |
| WBSManager | `src/BIMWorkflow/Scheduling/WBSManager.cs` | 작업 분류체계 |
| QuantityExtractor | `src/BIMWorkflow/Quantity/QuantityExtractor.cs` | 물량 산출 |
| BOQGenerator | `src/BIMWorkflow/CostEstimation/BOQGenerator.cs` | BOQ 생성 |

### Data Models

```csharp
public class CostNode : OntologyNode
{
    public string CBSCode { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
}

public class TaskNode : OntologyNode
{
    public string WBSCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double Progress { get; set; }
}
```

## Files to Create

| File | Action | Description |
|------|--------|-------------|
| `src/BIMWorkflow/CostEstimation/CBSManager.cs` | Create | CBS 관리 |
| `src/BIMWorkflow/CostEstimation/BOQGenerator.cs` | Create | BOQ 생성 |
| `src/BIMWorkflow/Scheduling/WBSManager.cs` | Create | WBS 관리 |
| `src/BIMWorkflow/Quantity/QuantityExtractor.cs` | Create | 물량 추출 |

## Dependencies

### External
- None additional

### Internal
- GraphDBConnector (Phase 2)
- OntologyManager (Phase 1)

## Acceptance Criteria

- [ ] CBS 코드 체계 관리
- [ ] WBS 작업 연결
- [ ] Element별 물량 추출
- [ ] BOQ 엑셀 내보내기
- [ ] 그래프 관계로 연결

---

**Related**: [TASKS.md](./TASKS.md) | [CHECKLIST.md](./CHECKLIST.md)
**Parent**: [PROGRESS.md](../../PROGRESS.md)
