# AGENTS.md — GestãoNew

> **Ponto de entrada para agentes de IA** (Claude Code, Cursor, Copilot, etc.)

## Fonte de Verdade

**LEIA `CLAUDE.md` na raiz** — é a constituição do projeto. Contém:
- Os **8 contratos arquiteturais** (Clean Architecture, Dapper + Oracle, DI nativa, Async/Await, Models vs DTOs, IResult, Senhas hash, Nomenclatura)
- O **protocolo de execução** de 8 passos antes de codificar
- Os **checklists universais** pré e pós-tarefa
- As **restrições globais** (guardrails)

Este arquivo (`AGENTS.md`) é um atalho/ponteiro. O `CLAUDE.md` tem precedência em caso de conflito.

## Documentação Estruturada (Progressive Disclosure)

| Caminho | Conteúdo | Quando Consultar |
|---------|----------|------------------|
| `CLAUDE.md` | Constituição, contratos, protocolo | **Sempre** (antes de codificar) |
| `PROGRESS.md` | Snapshot de sessão + falhas | **Sempre** (saber onde parou) |
| `TASKS.md` | Backlog operacional granular | **Sempre** (identificar próxima tarefa) |
| `agents/` | 7 personas especializadas | Quando a tarefa envolver a especialidade |
| `skills/` | 12 guias técnicos (Dapper, Oracle, JWT, React...) | Quando a tarefa usar a tecnologia |
| `commands/` | Prompts reutilizáveis (feature, bug, review, commit) | Quando executar o tipo de tarefa |
| `docs/` | 13 documentos por área (architecture, security, testing...) | Aprofundamento sob demanda |

## Stack Real (IMPORTANTE)

### Backend
- **.NET 9** + **ASP.NET Core Minimal APIs** + **Dapper 2.1.79** + **Oracle**
- Autenticação: **JWT Bearer + BCrypt** · Logging: **Serilog** · Testes: **xUnit + Moq**
- Solução: `Empresa.Api`, `Empresa.Data`, `Empresa.Util`, `Empresa.Worker`, `Empresa.Tests`

### Frontend
- **React 18 + TypeScript + Vite + Ant Design 5 + Zustand + React Router DOM 6 + Axios + dayjs**
- ⚠️ **NUNCA** use Vue/PrimeVue/Pinia — a documentação antiga usava essa stack, mas o projeto foi migrado para React.

## Comandos de Validação

```bash
# Backend
dotnet build && dotnet test

# Frontend
cd Empresa.Web && npm run lint && npm run build

# Dev local (atalho)
play.bat  # abre API + Frontend
```

## Guardrails (NUNCA)

- **Nunca** use `Sync` para operações de I/O.
- **Nunca** crie arquivos soltos na raiz — tudo dentro de um projeto da solution.
- **Nunca** commite `.env`, `appsettings.*.local`, `secrets.json`, chaves ou binários (`bin/`, `obj/`).
- **Nunca** altere migrations/procedures Oracle destrutivas sem aprovação do `database-engineer`.
- **Nunca** use a stack frontend desatualizada (Vue/PrimeVue/Pinia) — o projeto é React.