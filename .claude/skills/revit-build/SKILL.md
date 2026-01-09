---
name: revit-build
description: AgenticRevit 플러그인 빌드 및 배포 자동화 스킬
triggers: ["빌드", "build", "배포", "deploy", "컴파일", "compile"]
tools: [Bash, Read, Write, Glob]
---

# Revit Build Skill

## Purpose
AgenticRevit 플러그인의 빌드, 테스트, 배포를 자동화합니다.

## Commands

### Build Debug
```bash
cd src && dotnet build -c Debug
```

### Build Release
```bash
cd src && dotnet build -c Release
```

### Clean & Rebuild
```bash
cd src && dotnet clean && dotnet build -c Release
```

### Deploy to Revit
```bash
# Copy DLL to Revit Addins folder
$source = "src/bin/Release/AgenticRevit.dll"
$dest = "$env:APPDATA/Autodesk/Revit/Addins/2025/"
Copy-Item $source $dest -Force
Copy-Item "AgenticRevit.addin" $dest -Force
```

## Build Checklist
1. [ ] NuGet 패키지 복원
2. [ ] 컴파일 에러 확인
3. [ ] 경고 처리
4. [ ] DLL 복사
5. [ ] .addin 매니페스트 복사

## Troubleshooting

### Revit API 참조 에러
- Revit 2025 SDK 설치 확인
- .csproj의 HintPath 경로 확인

### 빌드 실패
- .NET Framework 4.8 설치 확인
- x64 플랫폼 설정 확인
