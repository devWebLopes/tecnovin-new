# PRD — Painel "Acesso B.I." (CadastroUsuarioBi) · Multi-Agent

| Campo | Valor |
|-------|-------|
| **Título** | Painel de Acesso ao B.I. — Gestão de Usuários × Empresas |
| **Projeto** | GestãoNew — Módulo Configurações |
| **Versão do PRD** | 1.0.0 |
| **Data** | 2026-07-31 |
| **Status** | Planejado |
| **Fonte de verdade** | `docs/acesso_bi_regra.md` (engenharia reversa completa do legado) |
| **Chave de permissão** | `cadastroUsuarioBi` |
| **Multi-Agent?** | Sim — Orquestrado via `agents/orchestrator.md` |
| **Agentes Ativos** | `orchestrator`, `architect`, `backend-engineer`, `database-engineer`, `frontend-engineer`, `qa-engineer`, `devops-engineer` |

---

## 0. Arquitetura Multi-Agent

### 0.1 Visão Geral da Orquestração

Este PRD é executado por um time de agentes de IA especializados, orquestrados pelo **Orchestrator Agent** (`agents/orchestrator.md`). O Orchestrator **NUNCA** executa tarefas técnicas — apenas coordena, delega e reporta.

```
┌──────────────────────────────────────────────────────────────┐
│                    ORCHESTRATOR AGENT                        │
│  - Lê o PRD e extrai todos os RFs                           │
│  - Cria plano de execução com tasks no TASKS.md             │
│  - Delega cada task ao agente apropriado                    │
│  - Monitora progresso e atualiza PROGRESS.md                │
│  - Gerencia dependências entre tasks                        │
│  - Consolida outputs e faz handoff entre agentes            │
└──────────┬──────────────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 1: CAMADA DE DADOS                                      │
│ ┌──────────────────┐                                         │
│ │ Database         │                                         │
│ │ Engineer Agent   │                                         │
│ │ - Model          │                                         │
│ │ - Repository     │                                         │
│ │ - Queries Q1-Q11 │                                         │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 2: CAMADA DE API                                        │
│ ┌──────────────────┐                                         │
│ │ Backend          │                                         │
│ │ Engineer Agent   │                                         │
│ │ - DTOs           │                                         │
│ │ - Service        │                                         │
│ │ - Endpoints (5)  │                                         │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 3: CAMADA FRONTEND                                      │
│ ┌──────────────────┐  ┌──────────────────┐                   │
│ │ Frontend         │  │ Architect        │                   │
│ │ Engineer Agent   │  │ Agent            │                   │
│ │ - Componentes    │  │ - Estrutura      │                   │
│ │ - Hooks/Services │  │ - Tipos TS       │                   │
│ │ - Integração API │  │ - Rotas          │                   │
│ └──────────────────┘  └──────────────────┘                   │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 4: QUALIDADE & TESTES                                   │
│ ┌──────────────────┐                                         │
│ │ QA Engineer      │                                         │
│ │ Agent            │                                         │
│ │ - Testes backend │                                         │
│ │ - Testes frontend│                                         │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 5: REVIEW & ENTREGA                                     │
│ ┌──────────────────┐  ┌──────────────────┐                   │
│ │ Architect        │  │ DevOps           │                   │
│ │ Agent            │  │ Engineer Agent   │                   │
│ │ - Code Review    │  │ - Build          │                   │
│ │ - Documentação   │  │ - Validação      │                   │
│ └──────────────────┘  └──────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
```

### 0.2 Catálogo de Agentes

| Agente | Arquivo | Função Principal |
|--------|---------|-----------------|
| **Orchestrator** | `agents/orchestrator.md` | Coordenação, distribuição de tarefas e sequenciamento |
| **Architect** | `agents/architect.md` | Design de arquitetura, estrutura de diretórios, tipos TS, code review |
| **Backend Engineer** | `agents/backend-engineer.md` | Implementação de endpoints Minimal API, DTOs, services |
| **Database Engineer** | `agents/database-engineer.md` | Modelagem de dados, queries Dapper, repositórios Oracle |
| **Frontend Engineer** | `agents/frontend-engineer.md` | Componentes React 18 + Ant Design 5, hooks Zustand, services Axios |
| **QA Engineer** | `agents/qa-engineer.md` | Testes unitários/integração (xUnit backend + Vitest frontend) |
| **DevOps Engineer** | `agents/devops-engineer.md` | Build de produção, validação final |

### 0.3 Fluxo de Orquestração

```
1. Orchestrator lê PRD → extrai RFs
2. Orchestrator cria/atualiza TASKS.md com tasks atômicas
3. Orchestrator delega tasks por fase:
   a. Fase 1 → database-engineer
   b. Fase 2 → backend-engineer
   c. Fase 3 → frontend-engineer + architect
   d. Fase 4 → qa-engineer
   e. Fase 5 → architect + devops-engineer
4. A cada task concluída → Orchestrator atualiza PROGRESS.md
5. Ao final de cada fase → Orchestrator valida handoff e libera próxima fase
```

### 0.4 Regras de Delegação

- **Orchestrator NUNCA executa tarefas técnicas** — apenas coordena e reporta
- Cada RF referencia o **Orchestrator** como coordenador e indica o(s) **agente(s) executor(es)**
- Cada task no `TASKS.md` referencia o Orchestrator como agente orquestrador + agente especialista executor
- Tasks dentro da mesma fase podem ser executadas em paralelo quando não há dependências
- Handoff entre fases exige validação do Orchestrator (todas as tasks da fase anterior concluídas)
- `TASKS.md` é a fonte canônica de tarefas; `PROGRESS.md` é o dashboard de progresso

---

## 1. Visão do Produto

### 1.1 Resumo Executivo

O painel **"Acesso B.I."** (título: **"Usuários com acesso ao B.I. por empresas"**) é uma tela administrativa dentro do menu **Configurações** que gerencia **quais usuários do sistema ERP podem acessar o módulo externo de B.I. (Business Intelligence) e para quais empresas do grupo esse acesso é válido**.

O painel materializa a relação **usuário × empresa** na tabela Oracle `USUARIO_EMPRESA`. Essa relação é o "gate" de acesso ao B.I.:

1. No menu principal, o atalho "Acesso ao BI" só é exibido se o usuário logado possuir **pelo menos uma linha** em `USUARIO_EMPRESA`.
2. O painel permite **conceder acesso em massa** (usuário → todas as empresas de uma vez) e **acesso granular** (usuário → empresa específica).
3. A coluna `EMPRESA` de `USUARIO_EMPRESA` restringe o escopo de empresas que o usuário pode visualizar dentro do B.I.

**Fonte de verdade**: `docs/acesso_bi_regra.md` — documento de engenharia reversa completa do painel legado (ASP.NET WebForms + DevExpress v16.2 + Oracle). Todas as queries, regras de negócio, comportamentos visuais e defeitos conhecidos estão documentados nesse arquivo.

### 1.2 Objetivos SMART

- **Específico**: Reconstruir o painel "Acesso B.I." na stack moderna (.NET 9 Minimal API + React 18 + Ant Design 5 + Dapper + Oracle), com 100% de fidelidade funcional ao legado, corrigindo os 9 defeitos documentados (D1–D9).
- **Mensurável**:
  - 5 endpoints REST versionados (`/api/v1/acesso-bi/...`).
  - 1 novo repositório Dapper (`IAcessoBiRepository` / `AcessoBiRepository`).
  - 1 novo service (`IAcessoBiService` / `AcessoBiService`).
  - 1 novo módulo frontend (`acesso-bi/`) com 4 componentes React.
  - Cobertura de testes ≥ 80% para o novo código.
  - `dotnet build` e `npm run build` sem erros.
- **Atingível**: A infraestrutura de autenticação JWT, permissões por perfil, DI, Dapper e Oracle já está estabelecida (Fases 0–13). O frontend React + Ant Design já está em produção com o módulo `usuarios/` (Fase 15).
- **Relevante**: Substitui o legado ASP.NET WebForms, elimina dependência de `Session` (stateless), fecha vulnerabilidades de SQL Injection, e integra o painel ao menu Configurações do novo sistema.
- **Temporal**: Entrega prevista para o ciclo atual de desenvolvimento, após a conclusão da Fase 15 (Frontend Usuários e Perfis).

### 1.3 Domínios de Negócio

| Domínio | Conceitos principais | Artefatos envolvidos |
|---|---|---|
| **Segurança / Controle de Acesso** | Usuário, Perfil, Página, Permissão de página por perfil | `ACESSO_CADASTRO_USUARIO`, `ACESSO_CADASTRO_PERFIL`, `ACESSO_CADASTRO_PAGINA`, `ACESSO_PERFIL_PAGINA` |
| **Inteligência de Negócio (B.I.)** | Acesso ao B.I. externo, escopo por empresa | `USUARIO_EMPRESA`, atalho "Acesso ao BI" no menu principal |
| **Estrutura Corporativa** | Empresa (grupo multiempresa), Razão Social, Nome Fantasia | View Oracle `VW_EMPRESA_NEW` |

**Conceito central:** O sistema ERP é multiempresa. A tabela `USUARIO_EMPRESA` é uma tabela associativa N:N entre usuários e empresas, representando a autorização "usuário U pode ver dados da empresa E no B.I.".

### 1.4 Escopo

#### Dentro do escopo
- **Backend:** Model `UsuarioEmpresa`, repositório `IAcessoBiRepository` / `AcessoBiRepository`, service `IAcessoBiService` / `AcessoBiService`, 5 endpoints Minimal API.
- **Frontend:** Módulo `acesso-bi/` com página principal `AcessoBiPage`, componentes `UsuarioBiLista` (tabela mestre com expand), `EmpresaVinculoLista` (tabela detalhe), `UsuarioBiForm` (modal de concessão total), `EmpresaSelect` (dropdown de empresas disponíveis).
- **Gate de permissão:** Verificação da chave `cadastroUsuarioBi` via `AcessoRepository` (já existente).
- **Correção dos 9 defeitos documentados** (D1–D9 do `acesso_bi_regra.md`).
- **Integração com menu Configurações** (item "Acesso B.I." no `SideMenu`).

#### Fora do escopo
- Alteração estrutural no banco Oracle (tabelas permanecem como estão — Regra 1 do `CLAUDE.md`).
- Migração de dados do legado.
- Página de redirecionamento para o B.I. externo (`CarregaBi.aspx` — será tratada em feature separada).
- Ícone de atalho "Acesso ao BI" no dashboard principal (será tratado em feature separada).

---

## 2. Arquitetura & Diretrizes Técnicas

### 2.1 Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| Backend API | .NET 9 + ASP.NET Core Minimal APIs |
| ORM / Acesso a Dados | Dapper 2.1.79 + `Oracle.ManagedDataAccess.Core` |
| Banco de Dados | Oracle (schema existente, sem alterações estruturais) |
| Autenticação | JWT Bearer (já configurado — Fase 2) |
| Logging | Serilog (já configurado — Fase 0) |
| Frontend Framework | React 18 + TypeScript (strict) |
| Build Frontend | Vite 6.x |
| UI Framework | Ant Design 5.x (antd) |
| Gerenciamento de Estado | Zustand 5.x |
| Roteamento | React Router DOM 6.x |
| Requisições HTTP | Axios 1.x (com interceptors JWT já configurados) |
| Datas | dayjs 1.x |

### 2.2 Estrutura de Diretórios

#### Backend (Empresa.Data + Empresa.Api)

```
Empresa.Data/
├── Models/
│   └── UsuarioEmpresa.cs          # NOVO — entidade USUARIO_EMPRESA
└── Repositories/
    ├── IAcessoBiRepository.cs     # NOVO — interface
    └── AcessoBiRepository.cs      # NOVO — implementação Dapper

Empresa.Api/
├── DTOs/
│   ├── Request/
│   │   └── AcessoBiRequest.cs     # NOVO — conceder/remover acesso
│   └── Response/
│       ├── UsuarioBiResponse.cs   # NOVO — linha do grid mestre
│       └── EmpresaBiResponse.cs   # NOVO — linha do grid detalhe
├── Services/
│   ├── IAcessoBiService.cs        # NOVO — interface
│   └── AcessoBiService.cs         # NOVO — lógica de negócio
└── Endpoints/
    └── AcessoBiEndpoints.cs       # NOVO — 5 endpoints
```

#### Frontend (Empresa.Web/src/)

```
src/
└── modules/
    └── acesso-bi/                  # NOVO — módulo completo
        ├── components/
        │   ├── UsuarioBiLista.tsx       # Tabela mestre (Ant Design Table expandable)
        │   ├── EmpresaVinculoLista.tsx  # Tabela detalhe (empresas do usuário)
        │   ├── UsuarioBiForm.tsx        # Modal de concessão total (usuário → todas empresas)
        │   └── EmpresaSelect.tsx        # Select de empresa disponível (não vinculada)
        ├── services/
        │   └── acessoBiService.ts       # Axios — 5 chamadas à API
        ├── hooks/
        │   └── useAcessoBi.ts           # Hook Zustand — estado do painel
        ├── types.ts                     # Tipos TypeScript
        └── AcessoBiPage.tsx             # View principal (master-detail)
```

### 2.3 Contratos Arquiteturais (8 regras — `CLAUDE.md`)

| # | Contrato | Aplicação neste PRD |
|---|----------|---------------------|
| 1 | **Clean Architecture** | `Api` → `Data` (nunca o inverso). `AcessoBiService` depende de `IAcessoBiRepository`. |
| 2 | **Dapper + Oracle** | Todas as queries Q1–Q9 do legado migradas para Dapper com parâmetros (`:param`). Nunca concatenar strings SQL. |
| 3 | **DI nativa** | `IAcessoBiRepository` e `IAcessoBiService` registrados em `Program.cs` via `AddScoped`. |
| 4 | **Nomenclatura** | `Empresa.Data.Repositories.IAcessoBiRepository`, `Empresa.Api.Services.IAcessoBiService`. Classes em inglês, docs em português. |
| 5 | **Async/Await** | Todo I/O (banco) é `async Task<T>`. Proibido `.Result`/`.Wait()`. |
| 6 | **Models vs DTOs** | `UsuarioEmpresa` é model de banco. Endpoints recebem/devolvem DTOs (`AcessoBiRequest`, `UsuarioBiResponse`, `EmpresaBiResponse`). |
| 7 | **Tratamento de Erros** | `Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`, `Results.Conflict()` (duplicidade). Middleware global captura exceções. |
| 8 | **Senhas** | Não se aplica diretamente (painel não manipula senhas). |

---

## 3. Requisitos Funcionais Detalhados

### RF01 — Gate de Permissão e Acesso ao Painel

**Prioridade**: P0 (Essencial — BLOQUEANTE)
**Orquestrador**: `orchestrator` → delega para `backend-engineer`

- **RF01.1** — O endpoint do painel deve verificar, via `AcessoRepository.GetPaginaByChave("cadastroUsuarioBi", idPerfil)`, se o usuário logado tem permissão.
- **RF01.2** — Sem permissão → retornar `403 Forbidden` com mensagem "Sem permissão de acesso a esta página.".
- **RF01.3** — A verificação é feita no service `AcessoBiService`, antes de qualquer operação de dados.
- **RF01.4** — A chave de permissão `cadastroUsuarioBi` já existe na tabela `ACESSO_CADASTRO_PAGINA` (banco legado). Não é necessário criá-la.

**Mecanismo**: Endpoints protegidos com `[Authorize]` (JWT) + verificação de permissão no service.

---

### RF02 — Listagem de Usuários com Acesso ao B.I. (Grid Mestre)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (repositório) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)

#### Backend

- **RF02.1** — Endpoint `GET /api/v1/acesso-bi/usuarios` — Retorna lista de usuários que possuem ≥1 vínculo em `USUARIO_EMPRESA`.
- **RF02.2** — Query SQL semanticamente idêntica à **Q2** do legado:

```sql
SELECT U.ID_USUARIO,
       A.NOME,
       LISTAGG(SUBSTR(V.NM_FANTASIA, 1, INSTR(V.NM_FANTASIA, ' ') - 1), ' - ')
           WITHIN GROUP (ORDER BY U.EMPRESA) AS EMPRESAS
  FROM USUARIO_EMPRESA U
 INNER JOIN ACESSO_CADASTRO_USUARIO A ON U.ID_USUARIO = A.ID_USUARIO
 INNER JOIN VW_EMPRESA_NEW V          ON U.EMPRESA    = V.CD_EMPRESA
 GROUP BY U.ID_USUARIO, A.NOME
```

- **RF02.3** — Retornar DTO `UsuarioBiResponse`:
```json
{
  "idUsuario": 1,
  "nome": "João Silva",
  "empresas": "TECNO - VINICOLA - FILIAL"
}
```

- **RF02.4** — Ordenação padrão por `NOME` (alfabética).
- **RF02.5** — Suporte a parâmetro opcional `?search=` para filtrar por nome de usuário (usar `WHERE A.NOME LIKE '%' || :search || '%'`).

#### Frontend

- **RF02.6** — Tabela Ant Design `<Table>` com colunas: **Usuário** (`nome`), **Empresas** (`empresas`), **Ações**.
- **RF02.7** — Linhas expandíveis (`expandable`) — ao expandir, exibe o grid detalhe (RF03).
- **RF02.8** — Apenas UMA linha expandida por vez (`expandable={{ expandedRowKeys, onExpandedRowsChange }}` com controle de estado).
- **RF02.9** — Botão **"Conceder Acesso Total"** no header da tabela — abre modal `UsuarioBiForm` (RF04).
- **RF02.10** — Coluna **Empresas** exibe a agregação formatada (read-only, sem edição inline — diferente do legado que usava combo).
- **RF02.11** — Loading state (skeleton/spin) enquanto carrega.
- **RF02.12** — Empty state: "Nenhum usuário com acesso ao B.I. cadastrado." (com ícone `Empty` do Ant Design).

---

### RF03 — Listagem de Empresas Vinculadas (Grid Detalhe)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (repositório) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)

#### Backend

- **RF03.1** — Endpoint `GET /api/v1/acesso-bi/usuarios/{idUsuario}/empresas` — Retorna empresas vinculadas ao usuário.
- **RF03.2** — Query SQL semanticamente idêntica à **Q7** do legado:

```sql
SELECT B.ID_USUARIO_EMPRESA,
       B.ID_USUARIO,
       B.EMPRESA   AS EMPRESA1,
       E.NM_FANTASIA AS EMPRESA
  FROM USUARIO_EMPRESA B
 INNER JOIN VW_EMPRESA_NEW E ON B.EMPRESA = E.CD_EMPRESA
 WHERE ID_USUARIO = :ID_USUARIO
```

- **RF03.3** — Retornar DTO `EmpresaBiResponse`:
```json
{
  "idUsuarioEmpresa": 100,
  "idUsuario": 1,
  "codigoEmpresa": 5,
  "nomeFantasia": "TECNOVIN DO BRASIL LTDA"
}
```

- **RF03.4** — **IMPORTANTE**: Eliminar dependência de `Session` do legado — o `ID_USUARIO` vem como parâmetro de rota (`{idUsuario}`), não de variável de sessão.

#### Frontend

- **RF03.5** — Tabela Ant Design `<Table>` aninhada no `expandedRowRender` da tabela mestre.
- **RF03.6** — Colunas: **Empresa** (`nomeFantasia`), **Ações**.
- **RF03.7** — Botão **"Vincular Empresa"** — abre dropdown `EmpresaSelect` para escolher uma empresa não vinculada (RF05).
- **RF03.8** — Botão **"Remover"** (ícone lixeira) em cada linha — exclusão com confirmação modal (`Modal.confirm`).
- **RF03.9** — Mensagem de confirmação de exclusão: "Deseja realmente excluir o acesso deste usuário ao BI da empresa selecionada?".
- **RF03.10** — Após excluir, recarregar grid detalhe e grid mestre (atualizar agregação `EMPRESAS`).

---

### RF04 — Concessão de Acesso Total (Usuário → Todas Empresas)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (repositório) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)

#### Backend

- **RF04.1** — Endpoint `POST /api/v1/acesso-bi/usuarios` — Concede acesso ao B.I. para um usuário em **TODAS** as empresas.
- **RF04.2** — Query SQL semanticamente idêntica à **Q3** do legado:

```sql
INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA)
SELECT NULL, :ID_USUARIO, CD_EMPRESA
  FROM VW_EMPRESA_NEW
```

- **RF04.3** — Body da requisição:
```json
{
  "idUsuario": 1
}
```

- **RF04.4** — **CORREÇÃO DO DEFEITO D6**: Antes de inserir, verificar se o usuário **já possui** acesso (≥1 linha em `USUARIO_EMPRESA`). Se sim, retornar `409 Conflict` com mensagem "Usuário já possui acesso ao B.I. Conceda acesso por empresa na edição.".
- **RF04.5** — Não informar `ID_USUARIO_EMPRESA` — a PK é gerada por trigger/sequence Oracle (comportamento do legado).
- **RF04.6** — Parâmetros usam Dapper (`new { ID_USUARIO = request.IdUsuario }`) — **nunca concatenar strings SQL** (corrige defeito D7).
- **RF04.7** — Retornar `201 Created` com a lista de `UsuarioBiResponse` atualizada.

#### Frontend

- **RF04.8** — Modal `UsuarioBiForm` com:
  - Título: "Conceder Acesso ao B.I."
  - Dropdown `<Select>` com **apenas usuários que AINDA NÃO têm acesso** (corrige defeito D6 — filtro).
  - Botão "Conceder" (submit) e "Cancelar".
- **RF04.9** — Dropdown de usuários carregado via `GET /api/v1/usuarios?somenteSemAcessoBi=true` (ou filtrado no frontend).
- **RF04.10** — Loading state no botão "Conceder" durante a requisição.
- **RF04.11** — Sucesso: fechar modal, exibir `message.success("Acesso concedido com sucesso!")`, recarregar grid mestre.
- **RF04.12** — Conflito (409): exibir `message.warning("Usuário já possui acesso ao B.I.")`.

---

### RF05 — Concessão de Acesso por Empresa (Usuário → Empresa Específica)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (repositório) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)

#### Backend

- **RF05.1** — Endpoint `GET /api/v1/acesso-bi/usuarios/{idUsuario}/empresas/disponiveis` — Retorna empresas **ainda NÃO vinculadas** ao usuário.
- **RF05.2** — Query SQL semanticamente idêntica à **Q6** do legado (com parâmetros, corrigindo D7):

```sql
SELECT CD_EMPRESA, NM_FANTASIA
  FROM VW_EMPRESA_NEW
 WHERE CD_EMPRESA NOT IN (SELECT NVL(EMPRESA, 0)
                            FROM USUARIO_EMPRESA
                           WHERE ID_USUARIO = :ID_USUARIO)
```

- **RF05.3** — Endpoint `POST /api/v1/acesso-bi/usuarios/{idUsuario}/empresas` — Vincula UMA empresa ao usuário.
- **RF05.4** — Query SQL semanticamente idêntica à **Q8** do legado:

```sql
INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA)
VALUES (NULL, :ID_USUARIO, :EMPRESA)
```

- **RF05.5** — Body da requisição:
```json
{
  "codigoEmpresa": 5
}
```

- **RF05.6** — Antes de inserir, verificar se o vínculo já existe. Se sim, retornar `409 Conflict`.
- **RF05.7** — Retornar `201 Created` com o `EmpresaBiResponse` do vínculo criado.

#### Frontend

- **RF05.8** — Botão **"Vincular Empresa"** no grid detalhe (RF03.7) — exibe dropdown inline ou modal simples.
- **RF05.9** — Dropdown `<Select>` carregado via `GET /api/v1/acesso-bi/usuarios/{idUsuario}/empresas/disponiveis`.
- **RF05.10** — Exibir `nomeFantasia` como label, `codigoEmpresa` como value.
- **RF05.11** — Ao selecionar uma empresa, confirmar via `Modal.confirm`: "Vincular empresa [nome] ao usuário [nome]?".
- **RF05.12** — Sucesso: `message.success`, recarregar grid detalhe + grid mestre.

---

### RF06 — Exclusão de Vínculo (Remover Acesso)

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (repositório) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)

#### Backend

- **RF06.1** — Endpoint `DELETE /api/v1/acesso-bi/empresas/{idUsuarioEmpresa}` — Remove um vínculo específico.
- **RF06.2** — Query SQL semanticamente idêntica à **Q9** do legado, **CORRIGINDO O DEFEITO D2**:

```sql
DELETE FROM USUARIO_EMPRESA WHERE ID_USUARIO_EMPRESA = :ID_USUARIO_EMPRESA
```

- **RF06.3** — ⚠️ **CORREÇÃO D2**: O legado usava `Session["USUARIOCLICADO"]` (que continha `ID_USUARIO` da linha mestre) como `ID_USUARIO_EMPRESA` — bug crítico. Na nova stack, o parâmetro `idUsuarioEmpresa` vem da rota e é a PK real da linha detalhe.
- **RF06.4** — Retornar `204 No Content` em caso de sucesso.
- **RF06.5** — Se `idUsuarioEmpresa` não existir, retornar `404 NotFound`.

#### Frontend

- **RF06.6** — Botão "Remover" (ícone `DeleteOutlined` ou `Popconfirm`) em cada linha do grid detalhe.
- **RF06.7** — Confirmação: `Popconfirm` com título "Deseja realmente excluir o acesso deste usuário ao BI da empresa selecionada?".
- **RF06.8** — Sucesso: `message.success("Vínculo removido com sucesso!")`, recarregar grids.

---

### RF07 — Integração com Menu Configurações

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer`

- **RF07.1** — Adicionar item "Acesso B.I." no menu **Configurações** do `SideMenu.tsx`.
- **RF07.2** — Rota: `/configuracoes/acesso-bi` → componente `AcessoBiPage`.
- **RF07.3** — Ícone: `KeyOutlined` ou `SafetyCertificateOutlined` (Ant Design).
- **RF07.4** — Verificação de permissão: o item só aparece se o perfil do usuário tiver a página de chave `cadastroUsuarioBi` (a API de páginas do menu já retorna apenas as páginas permitidas — comportamento herdado da Fase 3).

---

### RF08 — Validações e Tratamento de Erros

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `backend-engineer` (backend) + `frontend-engineer` (frontend)

- **RF08.1** — Backend: Validar que `idUsuario` existe em `ACESSO_CADASTRO_USUARIO` antes de inserir em `USUARIO_EMPRESA`.
- **RF08.2** — Backend: Validar que `codigoEmpresa` existe em `VW_EMPRESA_NEW` antes de inserir.
- **RF08.3** — Backend: Prevenir duplicidade `(ID_USUARIO, EMPRESA)` — verificar antes de INSERT.
- **RF08.4** — Frontend: Exibir mensagens de erro do backend via `message.error()` (interceptor Axios já configurado).
- **RF08.5** — Frontend: Tratar erros de rede (timeout, 500) com `message.error("Erro de conexão. Tente novamente.")`.

---

## 4. Camada de Dados (Banco e Queries)

### 4.1 Objetos Oracle Consumidos

| Objeto | Tipo | Uso no painel | Schema |
|--------|------|---------------|--------|
| `USUARIO_EMPRESA` | **Tabela** (alvo do CRUD) | Armazena os vínculos usuário↔empresa para o B.I. | `ID_USUARIO_EMPRESA` (NUMBER PK), `ID_USUARIO` (NUMBER FK), `EMPRESA` (NUMBER FK) |
| `ACESSO_CADASTRO_USUARIO` | Tabela | Combo de usuários; JOIN para exibir o nome | `ID_USUARIO` (PK), `NOME` |
| `VW_EMPRESA_NEW` | **View** | Catálogo de empresas; fonte do INSERT em massa | `CD_EMPRESA`, `DS_RAZAO_SOCIAL`, `NM_FANTASIA` |
| `ACESSO_CADASTRO_PAGINA` | Tabela | Gate de permissão (chave `cadastroUsuarioBi`) | `CHAVE_CONTROLE`, `ID_PAGINA` |
| `ACESSO_PERFIL_PAGINA` | Tabela | Gate de permissão (vínculo perfil×página) | `ID_PERFIL`, `ID_PAGINA` |

### 4.2 Nova Model (Empresa.Data/Models)

```csharp
// UsuarioEmpresa.cs
namespace Empresa.Data.Models;

public class UsuarioEmpresa
{
    public long IdUsuarioEmpresa { get; set; }
    public long IdUsuario { get; set; }
    public long Empresa { get; set; }
    // Propriedades de navegação (para JOINs)
    public string Nome { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
}
```

### 4.3 Queries Completas (Dapper)

| Query | Propósito | Fonte Legado |
|-------|-----------|-------------|
| `GetUsuariosComAcessoAsync()` | Listar usuários com acesso (GROUP BY + LISTAGG) | Q2 |
| `GetEmpresasDoUsuarioAsync(idUsuario)` | Empresas vinculadas ao usuário | Q7 |
| `GetEmpresasDisponiveisAsync(idUsuario)` | Empresas NÃO vinculadas | Q6 |
| `ConcederAcessoTotalAsync(idUsuario)` | INSERT em massa (1 linha por empresa) | Q3 |
| `ConcederAcessoEmpresaAsync(idUsuario, codigoEmpresa)` | INSERT unitário | Q8 |
| `RemoverAcessoAsync(idUsuarioEmpresa)` | DELETE por PK | Q9 (corrigido D2) |

### 4.4 Relacionamentos (DER Textual)

```
ACESSO_CADASTRO_USUARIO                 VW_EMPRESA_NEW (view)
| ID_USUARIO (PK)                       | CD_EMPRESA
| NOME                                  | NM_FANTASIA
+------ 1:N ------+              +------ 1:N ------+
                  |              |
                  v              v
               USUARIO_EMPRESA  (N:N usuário × empresa → acesso ao B.I.)
               | ID_USUARIO_EMPRESA (PK — trigger/sequence Oracle)
               | ID_USUARIO  (FK → ACESSO_CADASTRO_USUARIO)
               | EMPRESA     (FK → VW_EMPRESA_NEW.CD_EMPRESA)
```

---

## 5. Endpoints da API

### 5.1 Endpoints do Módulo Acesso B.I.

| Método | Endpoint | Descrição | RF |
|--------|---------|-----------|-----|
| `GET` | `/api/v1/acesso-bi/usuarios?search=` | Listar usuários com acesso ao B.I. (grid mestre) | RF02 |
| `GET` | `/api/v1/acesso-bi/usuarios/{idUsuario}/empresas` | Listar empresas vinculadas ao usuário (grid detalhe) | RF03 |
| `GET` | `/api/v1/acesso-bi/usuarios/{idUsuario}/empresas/disponiveis` | Listar empresas ainda NÃO vinculadas (dropdown) | RF05 |
| `POST` | `/api/v1/acesso-bi/usuarios` | Conceder acesso total (usuário → todas empresas) | RF04 |
| `POST` | `/api/v1/acesso-bi/usuarios/{idUsuario}/empresas` | Vincular uma empresa ao usuário | RF05 |
| `DELETE` | `/api/v1/acesso-bi/empresas/{idUsuarioEmpresa}` | Remover vínculo específico | RF06 |

### 5.2 Autenticação e Permissão

- Todos os endpoints exigem `Authorization: Bearer <jwt>` (`.RequireAuthorization()`).
- O service `AcessoBiService` verifica a permissão `cadastroUsuarioBi` no início de cada operação.
- Sem permissão → `403 Forbidden`.

---

## 6. Correções de Defeitos do Legado (Anexo D)

Estas correções são parte do escopo e devem ser implementadas como requisitos funcionais:

| # | Defeito | Correção | RF relacionado |
|---|---------|----------|----------------|
| D1 | `FechaBloco` com id errado (`dadosDados` vs `dadosFiltro`) | Cabeçalho colapsável funcional (Ant Design `Collapse` ou estado toggle) | RF02 (frontend) |
| D2 | DELETE detalhe usa `ID_USUARIO` mestre como `ID_USUARIO_EMPRESA` | Usar `ID_USUARIO_EMPRESA` real da rota (`{idUsuarioEmpresa}`) | RF06.2–RF06.3 |
| D3 | Botões Excluir/Editar não renderizados (DevExpress defaults) | Excluir funcionando no detalhe (RF06); sem exclusão no mestre (remoção total é perigosa) | — |
| D4 | `ID_USUARIO_EMPRESA` inserido como NULL | Manter comportar o legado (trigger/sequence Oracle preenche) | RF04.5 |
| D5 | CSS `##F2F2F2` inválido | Usar `#F2F2F2` no tema Ant Design | RF02 (frontend) |
| D6 | Combo NOMES lista todos usuários (risco de duplicata) | Filtrar usuários sem acesso no modal de concessão total; verificar duplicidade antes do INSERT | RF04.4, RF04.8 |
| D7 | Queries montadas por concatenação (SQL Injection) | Todas as queries usam parâmetros Dapper (`:param`) | RF02–RF06 |
| D8 | Colunas vestigiais declaradas e nunca exibidas | Não reproduzir — eliminar `ID_USUARIO_EMPRESA` e `EMPRESA` não utilizados no SELECT mestre | — |
| D9 | `sqlEmpresa` alimenta apenas coluna oculta | Eliminar — sem query extra desnecessária | — |

---

## 7. Métricas de Aceite (Definition of Done)

- [ ] Backend: `dotnet build` sem erros e sem warnings.
- [ ] Backend: `dotnet test` com ≥ 80% de cobertura no novo código.
- [ ] Frontend: `npm run build` sem erros.
- [ ] Frontend: `npm run lint` sem erros.
- [ ] Os 5 endpoints REST respondem corretamente (testar via Swagger).
- [ ] Gate de permissão `cadastroUsuarioBi` funcional — usuário sem permissão recebe 403.
- [ ] Concessão total cria 1 linha por empresa (verificar contagem).
- [ ] Concessão unitária não permite duplicidade (409 Conflict).
- [ ] Exclusão remove apenas a linha correta (D2 corrigido).
- [ ] Grid mestre exibe agregação `EMPRESAS` formatada com `" - "`.
- [ ] Apenas 1 detalhe expandido por vez.
- [ ] Menu "Configurações → Acesso B.I." funcional com rota.
- [ ] Todos os 9 defeitos (D1–D9) corrigidos ou tratados conforme decisão.

---

## 8. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|-------------|---------|-----------|
| `LISTAGG` não funcionar como esperado com Dapper/Oracle M-A | Baixa | Médio | Testar com dados reais na Fase 1; Dapper suporta `LISTAGG` nativamente (é SQL puro) |
| Trigger/sequence Oracle não gerar PK automaticamente | Média | Alto | Verificar no banco legado; se não existir, usar `USUARIO_EMPRESA_SEQ.NEXTVAL` no INSERT |
| Colisão com módulo `usuarios/` existente (mesmas tabelas) | Baixa | Baixo | O módulo `usuarios/` gerencia `ACESSO_CADASTRO_USUARIO`; o novo módulo gerencia `USUARIO_EMPRESA` — tabelas distintas |
| Performance do `LISTAGG` com muitas empresas | Baixa | Médio | Índices existentes em `USUARIO_EMPRESA`; `VW_EMPRESA_NEW` é view otimizada |

---

## 9. Plano de Execução (Orquestrado)

| Fase | Descrição | Agentes | Tasks |
|------|-----------|---------|-------|
| **Fase 1** | Camada de Dados | `orchestrator` → `database-engineer` | DB-01 a DB-07 |
| **Fase 2** | Camada de API | `orchestrator` → `backend-engineer` | BE-01 a BE-10 |
| **Fase 3** | Camada Frontend | `orchestrator` → `frontend-engineer` + `architect` | FE-01 a FE-12 |
| **Fase 4** | Qualidade & Testes | `orchestrator` → `qa-engineer` | QA-01 a QA-05 |
| **Fase 5** | Review & Entrega | `orchestrator` → `architect` + `devops-engineer` | RE-01 a RE-04 |

### 9.1 Sequenciamento de Delegação por Fase

```
FASE 1 ──── database-engineer ────► FASE 2
                                        │
                                  backend-engineer
                                        │
                                        ▼
                                    FASE 3
                                        │
                        ┌───────────────┼───────────────┐
                        │                               │
                  architect                     frontend-engineer
                        │                               │
                        └───────────────┬───────────────┘
                                        ▼
                                    FASE 4
                                        │
                                  qa-engineer
                                        │
                                        ▼
                                    FASE 5
                                        │
                        ┌───────────────┼───────────────┐
                        │                               │
                  architect                      devops-engineer
```

---

## 10. Aprovações

| Papel | Nome | Data | Assinatura |
|-------|------|------|-----------|
| PO | | | |
| Tech Lead | | | |
| QA | | | |

---

## 11. Anexos

### Anexo A — Documento de Referência

- `docs/acesso_bi_regra.md` — Especificação técnica completa do painel legado (769 linhas), incluindo:
  - 11 queries SQL exatas (Q1–Q11)
  - 2 regras de negócio (RN-01, RN-02)
  - 9 defeitos documentados (D1–D9)
  - Checklist de fidelidade para reimplementação
  - Contrato de estado de sessão
  - Ciclo de vida completo (hosting em abas/iframes)

### Anexo B — Stack Legada (para referência)

| Item | Legado | Novo |
|------|--------|------|
| Framework | ASP.NET WebForms (.NET 4.8) | ASP.NET Core Minimal API (.NET 9) |
| ORM | `System.Data.OracleClient` + `SqlDataSource` | Dapper + `Oracle.ManagedDataAccess.Core` |
| UI | DevExpress Web v16.2.8 (ASPxGridView) | React 18 + Ant Design 5 (Table) |
| Estado | `Session` (5 variáveis) | Stateless (JWT + parâmetros de rota) |
| Autenticação | Forms Auth + sessão | JWT Bearer |
| Tema | Office2010Blue | Ant Design default (com customizações) |
| Roteamento | `~/interna/acesso/CadastroUsuarioBi.aspx` | `/configuracoes/acesso-bi` |