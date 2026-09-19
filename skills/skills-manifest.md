# 🧠 Skills Manifest — Índice de Governança Técnica

## Visão Geral

Este manifesto é o catálogo mestre de todas as skills disponíveis no projeto **GestaoNew**. Cada skill é um guia técnico detalhado que define padrões, convenções e boas práticas para uma tecnologia ou domínio específico do projeto.

Arquitetos, engenheiros e agentes de IA DEVEM consultar a skill relevante antes de escrever ou modificar código.

---

## Catálogo de Skills

### 🔷 Backend .NET 9

| # | Skill | Arquivo | Domínio | Quando Consultar |
|---|-------|---------|---------|------------------|
| 1 | **Clean Architecture** | `clean-architecture-dotnet.md` | Estrutura de camadas, regras de dependência, padrões de design | Ao criar novos projetos, services, repositórios ou decidir onde colocar código |
| 2 | **Minimal API Patterns** | `minimal-api-patterns.md` | Endpoints REST, DTOs, versionamento, Swagger | Ao criar endpoints, configurar rotas ou documentar APIs |
| 3 | **JWT Authentication** | `jwt-authentication-dotnet.md` | Autenticação, autorização, BCrypt, refresh tokens | Ao implementar login, proteger endpoints ou gerenciar tokens |
| 4 | **Serilog Logging** | `serilog-logging-dotnet.md` | Logging estruturado, níveis, sinks, middleware | Ao adicionar logs em services, configurar logging ou depurar |

### 🔶 Banco de Dados — Oracle + Dapper

| # | Skill | Arquivo | Domínio | Quando Consultar |
|---|-------|---------|---------|------------------|
| 5 | **Dapper ORM** | `dapper-orm.md` | Queries Dapper, DbSession, repository pattern | Ao escrever qualquer query SQL com Dapper |
| 6 | **Oracle Best Practices** | `oracle-best-practices.md` | Modelagem Oracle, tipos, paginação, performance | Ao criar modelos de dados ou otimizar queries Oracle |
| 7 | **Oracle Procedures + Dapper** | `oracle-procedures-dapper.md` | REF CURSOR, multi-cursor, functions table-valued, UPSERT | Ao chamar procedures Oracle complexas (DRE, PZM, Financeiro) |

### 🟢 Frontend — React + TypeScript

| # | Skill | Arquivo | Domínio | Quando Consultar |
|---|-------|---------|---------|------------------|
| 8 | **React Frontend Patterns** | `react-frontend-patterns.md` | Componentes, hooks, services, tipos, estado | Ao criar componentes React, páginas, hooks ou services |

### 🔴 Qualidade e Infraestrutura

| # | Skill | Arquivo | Domínio | Quando Consultar |
|---|-------|---------|---------|------------------|
| 9 | **Testing Patterns** | `testing-xunit-dotnet.md` | xUnit, Moq, FluentAssertions, cobertura | Ao escrever testes unitários ou de integração |
| 10 | **Security Patterns** | `security-patterns-dotnet.md` | SQL Injection, CORS, HTTPS, rate limiting, headers | Ao revisar código para segurança ou configurar proteções |
| 11 | **DevOps & Docker** | `devops-docker-dotnet.md` | Docker, docker-compose, CI/CD, health checks | Ao configurar deploy, criar Dockerfiles ou pipelines |
| 12 | **Workflow Enforcer** | `workflow-enforcer.md` | Governança de execução, protocolo multi-agente | ⚠️ Skill OBRIGATÓRIA — sempre antes de qualquer tarefa |
# | Skill | Arquivo | Domínio | Quando Consultar |
|---|-------|---------|---------|------------------|
| ... | ... | ... | ... | ... |
| 13 | **PRD Generation** | `prd-generation-multiagent.md` | Engenharia de Requisitos, Multi-agentes | Ao criar escopo para novas features, módulos ou planejar arquitetura |
---

## Mapeamento: Agente → Skills

Cada agente especialista deve consultar as skills relevantes ao seu domínio:

| Agente | Skills Obrigatórias |
|--------|-------------------|
| **Architect** | `workflow-enforcer`, `clean-architecture-dotnet`, `oracle-best-practices`, `security-patterns-dotnet` |
| **Backend Engineer** | `workflow-enforcer`, `minimal-api-patterns`, `jwt-authentication-dotnet`, `serilog-logging-dotnet`, `dapper-orm`, `security-patterns-dotnet` |
| **Database Engineer** | `workflow-enforcer`, `oracle-best-practices`, `dapper-orm`, `oracle-procedures-dapper` |
| **Frontend Engineer** | `react-frontend-patterns`, `security-patterns-dotnet` (seções XSS/CSRF) |
| **QA Engineer** | `testing-xunit-dotnet`, `security-patterns-dotnet` (checklist de code review) |
| **DevOps Engineer** | `devops-docker-dotnet`, `serilog-logging-dotnet`, `security-patterns-dotnet` |
| **Architect** | `workflow-enforcer`, `prd-generation-multiagent`, `clean-architecture-dotnet`, `oracle-best-practices`, `security-patterns-dotnet` |
---

## Mapeamento: Fase do PRD → Skills

Cada fase do roadmap de implementação (PRD_MODERNIZACAO_GESTAO_NEW.md) requer skills específicas:

| Fase | Skills Críticas |
|------|----------------|
| **Fase 0** — Correções Urgentes | `workflow-enforcer`, `clean-architecture-dotnet`, `serilog-logging-dotnet` |
| **Fase 1** — Infraestrutura de Dados | `oracle-best-practices`, `dapper-orm`, `oracle-procedures-dapper` |
| **Fase 2** — Autenticação JWT | `jwt-authentication-dotnet`, `security-patterns-dotnet`, `testing-xunit-dotnet` |
| **Fase 3** — Módulo Acesso/Usuários | `minimal-api-patterns`, `dapper-orm`, `testing-xunit-dotnet`, `react-frontend-patterns` |
| **Fase 4** — Módulo Compras | `oracle-procedures-dapper` (REF CURSOR), `minimal-api-patterns`, `testing-xunit-dotnet` |
| **Fase 5** — Módulo Financeiro | `oracle-procedures-dapper` (DRE, Posição), `minimal-api-patterns` |
| **Fase 6** — Módulo Prazo Médio | `oracle-procedures-dapper` (drill-down 3 níveis, funções table-valued) |
| **Fase 7** — Módulo Vendas | `minimal-api-patterns`, `oracle-procedures-dapper` |
| **Fase 8** — Módulos Secundários | `minimal-api-patterns`, `oracle-procedures-dapper` |
| **Fase 9** — Worker Service | `clean-architecture-dotnet`, `serilog-logging-dotnet`, `oracle-procedures-dapper` |
| **Fase 10** — Qualidade e Testes | `testing-xunit-dotnet` (completo), `security-patterns-dotnet` |
| **Fase 11** — Documentação | `minimal-api-patterns` (Swagger), `devops-docker-dotnet` (README) |
| **Fase 12** — DevOps | `devops-docker-dotnet` (completo), `security-patterns-dotnet` |

---

## Protocolo de Consulta (Como Usar as Skills)

### Antes de Codificar (Pré-tarefa)

```
1. workflow-enforcer.md      ← SEMPRE (protocolo obrigatório)
2. clean-architecture-dotnet.md ← Se for criar nova classe/arquivo
3. [skill específica do domínio] ← Conforme tabela acima
4. docs/ [documentos relevantes] ← architecture.md, coding-standards, etc.
```

### Durante a Codificação

```
1. Mantenha a skill relevante aberta como referência
2. Siga os templates e anti-padrões documentados
3. Se encontrar um padrão não documentado → crie/atualize uma skill
```

### Após Codificar (Pós-tarefa)

```
1. testing-xunit-dotnet.md    ← Escrever/atualizar testes
2. security-patterns-dotnet.md ← Verificar checklist de segurança
3. dotnet build               ← Validar compilação
```

---

## Estrutura de uma Skill (Template)

Toda skill neste diretório segue a mesma estrutura:

```markdown
# [Emoji] Skill: [Nome] — [Tecnologia/Versão]

## Sobre
[Descrição concisa do que a skill cobre e quando usá-la]

## Configuração
[Pacotes, appsettings, registros no Program.cs]

## Padrões de Implementação
[Templates de código, exemplos práticos]

## Regras Obrigatórias
[Lista numerada de regras inegociáveis]

## Anti-Padrões (Proibidos)
[Exemplos do que NUNCA fazer]

## Checklist
[Itens verificáveis para code review]
```

---

## Como Adicionar uma Nova Skill

1. Crie o arquivo `.md` em `skills/` seguindo o template acima
2. Atualize este manifesto (`skills-manifest.md`):
   - Adicione a nova skill na tabela do catálogo
   - Atualize o mapeamento Agente → Skills
   - Atualize o mapeamento Fase → Skills (se aplicável)
3. Atualize `CLAUDE.md` se houver mudança nos contratos arquiteturais
4. Notifique os agentes via `PROGRESS.md`

---

## Status Atual das Skills

| Arquivo | Status | Data Criação | Última Atualização |
|---------|--------|:---:|:---:|
| `workflow-enforcer.md` | ✅ Ativo | Pré-existente | — |
| `clean-architecture-dotnet.md` | ✅ Ativo | Pré-existente | — |
| `dapper-orm.md` | ✅ Ativo | Pré-existente | — |
| `oracle-best-practices.md` | ✅ Ativo | Pré-existente | — |
| `minimal-api-patterns.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `jwt-authentication-dotnet.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `react-frontend-patterns.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `testing-xunit-dotnet.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `oracle-procedures-dapper.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `serilog-logging-dotnet.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `security-patterns-dotnet.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `devops-docker-dotnet.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |
| `skills-manifest.md` | ✅ Novo | 30/07/2026 | 30/07/2026 |

---

> **Mantido por:** Equipe de Arquitetura — atualizar sempre que nova tecnologia ou padrão for adotado.