# 🧠 Agentes Especialistas — GestãoNew

## Como Funciona

Cada arquivo markdown neste diretório define uma **persona de IA especializada** com:
- **Papel** — qual o domínio de conhecimento do agente
- **Responsabilidades** — o que ele faz e quais decisões toma
- **Ativação** — quando invocar este agente (ex: ao iniciar uma tarefa de backend)
- **Checklists** — lista de verificação específica do domínio

## Arquitetura Multi-Agent

```
                    ┌─────────────────┐
                    │   Orchestrator  │ ← Coordena, delega, reporta
                    │ (orchestrator)  │
                    └───────┬─────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   ┌────▼─────┐      ┌──────▼──────┐      ┌─────▼────┐
   │Architect │      │  Backend    │      │ Database │
   │          │      │  Engineer   │      │ Engineer │
   └──────────┘      └─────────────┘      └──────────┘
        │                   │                   │
        └───────────────────┼───────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   ┌────▼─────┐      ┌──────▼──────┐      ┌─────▼────┐
   │ Frontend │      │ QA Engineer │      │  DevOps  │
   │ Engineer │      │             │      │ Engineer │
   └──────────┘      └─────────────┘      └──────────┘
```

## Mapeamento de Personas

| Arquivo | Persona | Especialidade |
|---------|---------|---------------|
| `orchestrator.md` | Orquestrador Multi-Agent | Coordenação, delegação de tarefas, sequenciamento, handoff entre agentes |
| `architect.md` | Arquiteto(a) de Software | Estrutura do projeto, contratos, padrões, decisões tecnológicas |
| `frontend-engineer.md` | Engenheiro(a) Frontend React 18 | Componentes React TSX, hooks customizados, stores Zustand, integração com APIs |
| `backend-engineer.md` | Engenheiro(a) Backend .NET | Implementação de endpoints, services, middlewares, DI |
| `database-engineer.md` | Engenheiro(a) de Dados | Modelagem, queries Dapper, Oracle, procedures, performance |
| `qa-engineer.md` | QA / Testes | Testes unitários, integração, validação de regras de negócio |
| `devops-engineer.md` | DevOps | Deploy, CI/CD, Docker, configuração de ambiente |

## Fluxo de Trabalho Multi-Agent

1. **Orquestração**: Orchestrator lê o PRD, extrai RFs e cria tasks no `TASKS.md`
2. **Design**: Architect define estrutura de diretórios e tipos
3. **Implementação**: Backend Engineer + Frontend Engineer + Database Engineer executam em paralelo
4. **Qualidade**: QA Engineer valida as implementações com testes
5. **Refinamento**: Frontend Engineer ajusta UI/UX + Architect faz code review
6. **Entrega**: DevOps Engineer cuida do build, validação e deploy

## Como Ativar um Agente

Antes de iniciar qualquer tarefa, o **Orchestrator** (`orchestrator.md`) deve:

1. Ler o PRD e extrair os RFs
2. Criar tasks atômicas no `TASKS.md` com agente orquestrador + agente executor
3. Delegar cada task ao agente especialista apropriado
4. Monitorar progresso e atualizar `PROGRESS.md`
5. Gerenciar handoff entre fases (só libera próxima fase quando anterior está 100%)

> 💡 **Regra de ouro**: O Orchestrator **NUNCA** executa tarefas técnicas — apenas coordena e reporta.
> Para tarefas que envolvem múltiplos domínios, carregue os agentes na ordem de dependência (DB → Backend → Frontend → QA).
