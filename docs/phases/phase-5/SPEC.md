# Phase 5: Dashboard UI

**Status**: ⏳ Planned
**Dependencies**: Phase 1-4
**Last Updated**: 2025-01-09

---

## Scope

- [ ] WPF 대시보드 창
- [ ] 실시간 메트릭 표시
- [ ] 그래프 시각화
- [ ] 쿼리 인터페이스
- [ ] 설정 관리

## Technical Details

### Architecture Changes

```
src/UI/
├── MainDockPanel.xaml
├── GraphViewer.xaml
├── ChangeLogPanel.xaml
├── QueryInterface.xaml
├── SettingsPanel.xaml
└── ViewModels/
    ├── DashboardViewModel.cs
    ├── GraphViewModel.cs
    └── SettingsViewModel.cs
```

### Key Components

| Component | File | Purpose |
|-----------|------|---------|
| MainDockPanel | `src/UI/MainDockPanel.xaml` | 메인 대시보드 |
| GraphViewer | `src/UI/GraphViewer.xaml` | 그래프 시각화 |
| QueryInterface | `src/UI/QueryInterface.xaml` | 쿼리 UI |
| SettingsPanel | `src/UI/SettingsPanel.xaml` | 설정 관리 |

### UI/UX Design

```
┌─────────────────────────────────────────┐
│  AgenticREVIT Dashboard                 │
├─────────────────────────────────────────┤
│ ┌─────────────┐ ┌─────────────────────┐ │
│ │ Metrics     │ │ Graph View          │ │
│ │ - Changes   │ │                     │ │
│ │ - Elements  │ │   [Node Graph]      │ │
│ │ - Sync      │ │                     │ │
│ └─────────────┘ └─────────────────────┘ │
│ ┌─────────────────────────────────────┐ │
│ │ Query: [                    ] [Ask] │ │
│ │ Result: ...                         │ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

## Files to Create

| File | Action | Description |
|------|--------|-------------|
| `src/UI/MainDockPanel.xaml` | Create | 메인 UI |
| `src/UI/GraphViewer.xaml` | Create | 그래프 뷰어 |
| `src/UI/QueryInterface.xaml` | Create | 쿼리 인터페이스 |
| `src/UI/SettingsPanel.xaml` | Create | 설정 패널 |
| `src/UI/ViewModels/*.cs` | Create | ViewModel들 |

## Dependencies

### External
- WPF (included in .NET Framework 4.8)
- Graph visualization library (TBD)

### Internal
- All previous phases

## Acceptance Criteria

- [ ] Revit 내 도킹 패널 표시
- [ ] 실시간 메트릭 업데이트
- [ ] 그래프 노드/엣지 시각화
- [ ] 자연어 쿼리 입력/결과
- [ ] 설정 저장/로드

---

**Related**: [TASKS.md](./TASKS.md) | [CHECKLIST.md](./CHECKLIST.md)
**Parent**: [PROGRESS.md](../../PROGRESS.md)
