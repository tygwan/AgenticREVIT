---
name: init
description: Initialize and analyze a new project. Detects tech stack, structure, and creates initial CLAUDE.md. Use when starting work on any new codebase.
---

# Project Initialization Skill

## Usage
```
/init [path]
```

### Parameters
- `path`: Optional. Project root path (default: current directory)

### Examples
```bash
/init
/init ./my-project
/init @./backend
```

## Workflow

### 1. Structure Discovery
First, discover the project structure:

```bash
# Find root indicators
Glob: package.json, requirements.txt, *.csproj, go.mod, Cargo.toml, pom.xml

# Find source directories
Glob: src/**, lib/**, app/**

# Find config files
Glob: *.config.*, .env*, tsconfig.json, setup.py

# Find documentation
Glob: README*, docs/**, *.md
```

### 2. Tech Stack Detection
Identify technologies used:

| File | Stack |
|------|-------|
| package.json | Node.js |
| tsconfig.json | TypeScript |
| requirements.txt | Python |
| *.csproj | .NET |
| go.mod | Go |
| Cargo.toml | Rust |

### 3. Framework Detection
Identify frameworks:

```bash
# Web frameworks
Grep: "express|fastify|koa|nest" package.json
Grep: "flask|django|fastapi" requirements.txt
Grep: "gin|echo|fiber" go.mod

# Frontend frameworks
Grep: "react|vue|angular|svelte" package.json
Grep: "next|nuxt|gatsby" package.json
```

### 4. Generate Project Summary
Create or update CLAUDE.md:

```markdown
# Project Name

> **Type**: {Web App | API | CLI | Library}
> **Stack**: {Language + Framework}
> **Status**: {Active | Maintenance | Planning}

## Quick Start
```bash
{install command}
{run command}
```

## Structure
```
project/
├── src/         # Source code
├── tests/       # Tests
└── docs/        # Documentation
```

## Key Files
| Purpose | File |
|---------|------|
| Entry | src/index.ts |
| Config | config.json |
| Tests | tests/ |

## Commands
| Task | Command |
|------|---------|
| Install | npm install |
| Dev | npm run dev |
| Build | npm run build |
| Test | npm test |
```

### 5. Create docs/_INDEX.md (if not exists)
```markdown
# Documentation Index

## Overview
- [README](../README.md)
- [Architecture](./architecture.md)

## Guides
- [Development](./development.md)
- [Deployment](./deployment.md)

## Reference
- [API](./api.md)
- [Configuration](./configuration.md)
```

## Output

After running `/init`, you'll get:

1. **Console Output**: Project analysis summary
2. **CLAUDE.md**: Created/updated with project context
3. **docs/_INDEX.md**: Documentation structure (if needed)

## Tips

- Run `/init` when starting on any new project
- Re-run after major structural changes
- Use output to orient team members
