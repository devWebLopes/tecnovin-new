# GitHub Copilot Instructions — GestãoNew

## Source of Truth
Always read `CLAUDE.md` at the repository root before making changes. It contains the 8 architectural contracts, execution protocol, and global restrictions. This file is a summary; `CLAUDE.md` takes precedence.

## Required Workflow
1. Read `PROGRESS.md` → know where work stopped and what failed
2. Read `TASKS.md` → identify the next task
3. Consult `agents/` → persona scope and responsibilities
4. Consult `skills/` → technical guides (Dapper, Oracle, JWT, React, etc.)
5. Execute the task following the contracts below
6. Validate with `dotnet build` + `dotnet test`
7. Update `TASKS.md` and `PROGRESS.md`

## Architectural Contracts (summary)
- **Clean Architecture**: Api → Data → Util. No layer references another beyond the one immediately below.
- **Dapper + Oracle**: Never use Entity Framework or raw ADO.NET outside Dapper.
- **Native DI**: Only `Microsoft.Extensions.DependencyInjection`.
- **Async/Await**: All I/O operations must be `async Task`. `.Result`/`.Wait()` are forbidden.
- **Models vs DTOs**: `Empresa.Data.Models` are database entities; endpoints receive DTOs.
- **Error Handling**: Standardized `IResult` (`Ok`, `NotFound`, `BadRequest`) + global middleware.
- **Passwords**: Hash (BCrypt or PBKDF2). Never plain text.

## Frontend (Empresa.Web)
- Actual stack: **React 18 + TypeScript + Vite + Ant Design 5 + Zustand + React Router DOM 6 + Axios + dayjs**
- Commands: `npm run dev` (dev), `npm run build` (build), `npm run lint` (lint), `npm run test` (vitest)
- Structure: `src/modules/<modulo>/{components,services,hooks,types.ts,<Modulo>Page.tsx}`

## Never Do
- Never use synchronous I/O operations.
- Never create loose files at repo root — everything inside a solution project.
- Never commit `.env`, `appsettings.*.local`, `secrets.json`, keys, or binaries (`bin/`, `obj/`).
- Never alter destructive Oracle migrations/procedures without `database-engineer` approval.
- Never use the outdated frontend stack (Vue/PrimeVue/Pinia) — the project is React.

## Validation Commands
```bash
# Backend
dotnet build && dotnet test

# Frontend
cd Empresa.Web && npm run lint && npm run build