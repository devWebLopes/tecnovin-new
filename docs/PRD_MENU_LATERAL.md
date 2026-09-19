# PRD — Menu Lateral (Refatoração e Migração) · Multi-Agent

| Campo | Valor |
|-------|-------|
| **Título** | Refatoração e Migração do Menu Lateral Esquerdo (shell da aplicação) |
| **Projeto** | GestãoNew — Shell / Layout |
| **Versão do PRD** | 1.0.0 |
| **Data** | 2026-08-01 |
| **Status** | Planejado |
| **Fonte de verdade** | `docs/menus.md` (engenharia reversa completa do legado TreisTecnovin) |
| **Chave de autorização** | `CHAVE_CONTROLE` (por página) — permissão sempre por perfil (usuário tem 1 perfil) |
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
│ │ Database         │   Validação de schema + CONNECT BY      │
│ │ Engineer Agent   │   + Mais Acessados c/ filtro de perfil  │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 2: CAMADA DE API                                        │
│ ┌──────────────────┐                                         │
│ │ Backend          │   DTOs + MenuService + Endpoints        │
│ │ Engineer Agent   │   + Handler de autorização + Cache      │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 3: CAMADA FRONTEND                                      │
│ ┌──────────────────┐  ┌──────────────────┐                   │
│ │ Frontend         │  │ Architect        │                   │
│ │ Engineer Agent   │  │ Agent            │                   │
│ │ - SideMenu/abos  │  │ - Tipos TS       │                   │
│ │ - MaisAcessados  │  │ - menuStore      │                   │
│ │ - Integração API │  │ - Rotas/ROUTE_MAP│                   │
│ └──────────────────┘  └──────────────────┘                   │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 4: QUALIDADE & TESTES                                   │
│ ┌──────────────────┐                                         │
│ │ QA Engineer      │   Testes backend + frontend +           │
│ │ Agent            │   checklist de fidelidade (menus.md §10.4)│
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

---

## 1. Visão do Produto

### 1.1 Resumo Executivo

O menu lateral é o **shell de navegação** do sistema. No legado (`Treis.Web\Principal.aspx`, ASP.NET WebForms + DevExpress), o painel de menu é remontado a cada `Page_Load` e entrega:

1. **Menu hierárquico** de páginas **filtrado pelo perfil do usuário** (permissão por `ACESSO_PERFIL_PAGINA`, nunca por usuário).
2. **"Mais Acessados"** — top 10 de páginas mais abertas pelo usuário (telemetria em `ACESSO_VISUALIZACAO_PAGINA`).
3. **Barra do usuário** — login exibido, link condicional para o B.I. (dependente de `USUARIO_EMPRESA`), troca de senha e sair.

Este PRD migra esse comportamento para a stack nova (`.NET 9 Minimal API + Dapper + Oracle` e `React 18 + Ant Design 5 + Zustand + React Router 6`), **preservando 100% das regras de negócio RN-01 a RN-14 do `docs/menus.md`** e **sem alterar nenhuma das 5 tabelas Oracle** (Regra de Ouro nº 1).

O estado atual do projeto novo já possui um embrião do menu (`SideMenu.tsx` consumindo `GET /api/v1/paginas/menu-hierarquico`), porém com **três lacunas críticas** identificadas na engenharia reversa:

| # | Lacuna atual vs legado | Severidade |
|---|------------------------|-----------|
| G-01 | `GetPaginasMenuAsync` **não reproduz o UNION legado**: pais sem vínculo próprio no perfil (pais implícitos, RN-03) **não aparecem** no menu | 🔴 Bloqueante |
| G-02 | **Não existe** painel "Mais Acessados", nem telemetria de acesso (`POST /menu/acessos`) consumida por endpoint | 🔴 Bloqueante |
| G-03 | **Contrato frontend desalinhado**: `SideMenu.tsx` lê `page.nome`/`page.icone`, mas a API retorna `tituloMenu`/`chaveControle` — o menu renderiza rótulos vazios | 🟠 Alta |

A migração também corrige os **12 defeitos latentes (D-01 a D-12)** do legado, conforme decisões na seção 6.

**Fonte de verdade**: `docs/menus.md` — engenharia reversa verificada linha a linha contra `Principal.aspx(.cs)`, `DaoAcesso.cs`, `Pagina.cs`, `Usuario.cs`, `tab-view.js` e `rotina.js` (nenhum código do legado foi alterado).

### 1.2 Objetivos SMART

- **Específico**: Reconstruir o menu lateral na stack moderna com 100% de fidelidade funcional ao legado (RN-01 a RN-14), fechando as lacunas G-01 a G-03 e corrigindo os defeitos D-01 a D-12 conforme decisões.
- **Mensurável**:
  - 3 endpoints REST versionados (`GET /api/v1/menu`, `POST /api/v1/menu/acessos`, `GET /api/v1/paginas/{chave}/autorizacao`).
  - 1 novo service (`IMenuService`/`MenuService`), 1 novo handler de autorização, 1 cache por perfil.
  - 1 nova store Zustand + 1 service Axios + 1 novo painel "Mais Acessados" no frontend.
  - Checklist de fidelidade do `menus.md §10.4` 100% atendido.
  - `dotnet build`/`dotnet test` e `npm run build`/`npm run lint` sem erros.
- **Atingível**: JWT, DI, Dapper, Oracle e o layout React já existem (Fases 0–17). A tabela `ACESSO_VISUALIZACAO_PAGINA` e o upsert (`RegistraAcessoPaginaAsync`) já estão implementados no repository.
- **Relevante**: Substitui o shell legado, elimina `Session` (stateless), reproduz a hierarquia real (RN-03), e fecha o furo de segurança de acesso direto por URL (RN-12) no backend.
- **Temporal**: Entrega no ciclo atual, após a Fase 17 (Finalização e Correções — concluída).

### 1.3 Domínios de Negócio

| Domínio | Conceitos principais | Artefatos envolvidos |
|---|---|---|
| **Acesso / Segurança** | Usuário (1 perfil), permissão página×perfil, `CHAVE_CONTROLE` | `ACESSO_CADASTRO_USUARIO`, `ACESSO_PERFIL_PAGINA`, `ACESSO_CADASTRO_PAGINA` |
| **Navegação** | Catálogo hierárquico de páginas (self-reference `ID_PAGINA_PAI`) | `ACESSO_CADASTRO_PAGINA` |
| **Telemetria de uso** | Top 10 "Mais Acessados" por usuário | `ACESSO_VISUALIZACAO_PAGINA` |
| **Integração B.I.** | Vínculo usuário×empresa habilita link do B.I. | `USUARIO_EMPRESA` |

**Conceitos centrais a preservar**:
- `CHAVE_CONTROLE` é a "moeda" de autorização — usada no menu **e** no guard de cada página (RN-12).
- Permissão é **por perfil** (usuário tem exatamente 1 perfil) — nunca por usuário (RN-01).
- Hierarquia via `ID_PAGINA_PAI`, profundidade ilimitada (RN-04).

### 1.4 Escopo

#### Dentro do escopo

- **Backend**:
  - Reescrever `AcessoRepository.GetPaginasMenuAsync` com `CONNECT BY PRIOR` reproduzindo a semântica do UNION legado (pais implícitos RN-03, filtro `ATIVO='S'` inclusive nos pais — D-06).
  - Novo `GetPaginasMaisAcessadasAsync(idUsuario, idPerfil)` com filtro por perfil (D-05) e ordenação explícita por quantidade (D-04).
  - `MenuService`/`IMenuService` + endpoints `GET /api/v1/menu`, `POST /api/v1/menu/acessos`, `GET /api/v1/paginas/{chaveControle}/autorizacao`.
  - Handler de autorização por página (`PaginaAutorizacaoHandler` + policy `PaginaAcesso`) — tradução do `GetPaginaByChave` das ~60 páginas internas (RN-12).
  - Cache do menu por perfil (`IMemoryCache`) com invalidação ao salvar perfil/página (D-08).
  - DTOs de menu (seção 4.3).
- **Frontend**:
  - Refatorar `SideMenu.tsx` alinhando o contrato (G-03), com ícones por heurística, tooltip e navegação por rota SPA.
  - Novo painel **"Mais Acessados"** (top 10) + registro de telemetria ao navegar (RN-07/RN-08).
  - Barra do usuário com link condicional do B.I. (RN-09).
  - Store Zustand de menu com cache TTL (D-08) e mapeamento `CHAVE_CONTROLE` → rota SPA.
  - Guard de rota no frontend (best-effort) + tela "Sem Permissão".
  - Persistência do estado colapsado do menu (melhoria).
- **Decisões sobre os 12 defeitos (D-01 a D-12)** — seção 6.

#### Fora do escopo

- Alteração estrutural nas 5 tabelas Oracle (Regra de Ouro nº 1).
- Migração de senhas para BCrypt (pendência já documentada em Fase 2/AuthService — fora deste PRD).
- Sistema de **abas com iframe** do legado (RN-13) — a navegação SPA usa o router; abas ficam adiadas (decisão na seção 6).
- Posição do menu: o legado era **à direita**; o projeto novo mantém a posição **esquerda** já adotada no `AppLayout` (decisão cosmética, não é regra de negócio).
- Telas internas dos módulos (Compras, Financeiro, etc.) — apenas a navegação/linkagem a partir do menu.

---

## 2. Arquitetura & Diretrizes Técnicas

### 2.1 Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| Backend API | .NET 9 + ASP.NET Core Minimal APIs |
| ORM / Acesso a Dados | Dapper 2.1.79 + `Oracle.ManagedDataAccess.Core` |
| Banco de Dados | Oracle (schema existente, sem alterações estruturais) |
| Autenticação | JWT Bearer (claims `ID_USUARIO`, `ID_PERFIL`) — Fase 2 |
| Autorização | Policy `PaginaAcesso` + `AuthorizationHandler` (novo) |
| Cache | `IMemoryCache` (nativo) — menu por perfil |
| Logging | Serilog (`ILogger<T>`) — Fase 0 |
| Frontend Framework | React 18 + TypeScript (strict) |
| Build Frontend | Vite 6.x |
| UI Framework | Ant Design 5.x (`Menu`, `Layout`, `List`, `Tooltip`, `Dropdown`) |
| Gerenciamento de Estado | Zustand 5.x (`persist`) |
| Roteamento | React Router DOM 6.x (lazy + rotas aninhadas) |
| Requisições HTTP | Axios 1.x (interceptors JWT + refresh já configurados) |

### 2.2 Estrutura de Diretórios

#### Backend (Empresa.Data + Empresa.Api)

```
Empresa.Data/
├── Models/
│   └── PaginaAcesso.cs              # NOVO — Pagina + QUANTIDADE (mais acessados)
└── Repositories/
    ├── IAcessoRepository.cs         # ATUALIZAR — +2 métodos / assinatura alterada
    └── AcessoRepository.cs          # ATUALIZAR — CONNECT BY, filtro perfil, upsert

Empresa.Api/
├── Authorization/
│   ├── PaginaAutorizacaoRequirement.cs   # NOVO — requirement
│   └── PaginaAutorizacaoHandler.cs       # NOVO — handler (GetPaginaByChave × perfil)
├── DTOs/
│   ├── Request/
│   │   └── RegistroAcessoMenuRequest.cs  # NOVO — { chaveControle }
│   └── Response/
│       ├── MenuResponse.cs               # NOVO — { menu, maisAcessados, usuario }
│       ├── MenuItemResponse.cs           # NOVO — nó da árvore (recursivo)
│       ├── MaisAcessadoItemResponse.cs   # NOVO — { ..., quantidade }
│       └── MenuUsuarioResponse.cs        # NOVO — { login, nome, exibeLinkBi }
├── Services/
│   ├── IMenuService.cs                   # NOVO — interface
│   └── MenuService.cs                    # NOVO — orquestra tree + cache + telemetria
└── Endpoints/
    └── MenuEndpoints.cs                  # NOVO — 3 endpoints
```

#### Frontend (Empresa.Web/src/)

```
src/
├── types/api.ts                    # ATUALIZAR — MenuResponse, MenuItem, MaisAcessadoItem, MenuUsuario
├── services/
│   └── menuService.ts              # NOVO — getMenu(), registrarAcesso(chave)
├── store/
│   └── menuStore.ts                # NOVO — Zustand: menu, maisAcessados, exibeLinkBi, cache TTL
├── components/layout/
│   ├── SideMenu.tsx                # REFATORAR — contrato, ícones, telemetria, ROUTE_MAP
│   ├── MaisAcessadosPanel.tsx      # NOVO — lista top 10
│   └── AppLayout.tsx               # ATUALIZAR — link B.I. (RN-09)
└── pages/auth/
    └── SemPermissaoPage.tsx        # NOVO — RN-12 (frontend best-effort)
```

### 2.3 Contratos Arquiteturais (8 regras — `CLAUDE.md`)

| # | Contrato | Aplicação neste PRD |
|---|----------|---------------------|
| 1 | **Clean Architecture** | `MenuService` (Api) depende de `IAcessoRepository` (Data). Nunca o inverso. |
| 2 | **Dapper + Oracle** | Queries reescritas com bind variables `:param`. Nunca concatenar SQL (D-01). |
| 3 | **DI nativa** | `IMenuService`/`MenuService` + handler registrados em `Program.cs` via `AddScoped`. |
| 4 | **Nomenclatura** | `Empresa.Api.Services.IMenuService`, `Empresa.Data.Repositories.IAcessoRepository`. Classes em inglês, docs em português. |
| 5 | **Async/Await** | Todo I/O é `async Task<T>`. Proibido `.Result`/`.Wait()`. |
| 6 | **Models vs DTOs** | `Pagina`/`PaginaAcesso` são models. Endpoints retornam DTOs (`MenuResponse`, `MenuItemResponse`). |
| 7 | **Tratamento de Erros** | `Results.Ok()`/`NotFound()`/`BadRequest()`/`Unauthorized()`/`Forbid()`. Erros logados via Serilog (D-03). Middleware global. |
| 8 | **Senhas** | Não se aplica diretamente (menu não manipula senhas). |

---

## 3. Requisitos Funcionais Detalhados

> **Legenda de mapeamento**: cada RF referencia as regras de negócio do `docs/menus.md` (RN-01 a RN-14) e os defeitos (D-01 a D-12).

### RF01 — Menu Hierárquico do Perfil (`GET /api/v1/menu`)

**Prioridade**: P0 (Essencial — BLOQUEANTE)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (SQL) + `backend-engineer` (endpoint/service)
**RNs**: RN-01, RN-02, RN-03, RN-04, RN-05, RN-06 | **Defeitos**: D-03, D-06, D-07, D-08

#### Backend

- **RF01.1** — Endpoint `GET /api/v1/menu` — retorna o **shell completo** do menu: `{ menu, maisAcessados, usuario }` (RN-09 incluída em `usuario.exibeLinkBi`).
- **RF01.2** — Extrair `ID_PERFIL` e `ID_USUARIO` dos claims JWT (`context.User.FindFirst("ID_PERFIL")`). Ausente/inválido → `401 Unauthorized` (RN-10).
- **RF01.3** — Reescrever `AcessoRepository.GetPaginasMenuAsync(idPerfil)` com `CONNECT BY PRIOR` **reproduzindo o UNION legado** (SQL 5.1 do `menus.md`):
  - Páginas ativas permitidas ao perfil (RN-01, RN-02);
  - **Mais todos os ancestrais** dessas páginas, mesmo sem vínculo próprio no perfil (RN-03) — profundidade ilimitada (RN-04, D-07);
  - `ATIVO = 'S'` aplicado **também aos ancestrais** (correção D-06 — validar impacto com dados reais antes de aplicar);
  - Ordenação por `ORDEM` crescente (RN-05).

  Query recomendada (validar fidelidade na Fase 1 contra a saída do UNION legado):
  ```sql
  SELECT DISTINCT p.ID_PAGINA, p.URL, p.TITULO_ABA, p.CHAVE_CONTROLE,
                  p.TITULO_MENU, p.ID_PAGINA_PAI, p.ORDEM, p.TOOLTIP, p.ATIVO
    FROM ACESSO_CADASTRO_PAGINA p
   START WITH p.ID_PAGINA IN (
              SELECT pp.ID_PAGINA FROM ACESSO_PERFIL_PAGINA pp
               WHERE pp.ID_PERFIL = :IdPerfil)
   CONNECT BY PRIOR p.ID_PAGINA_PAI = p.ID_PAGINA
   WHERE p.ATIVO = 'S'
   ORDER BY p.ORDEM
  ```
  > ⚠️ **IMPORTANTE**: a query atual (sem `START WITH`/`CONNECT BY` e sem UNION) **não traz pais implícitos** — a query acima é mandatória para atingir RN-03 (G-01).
- **RF01.4** — `MenuService.GetMenuAsync(idUsuario, idPerfil)`:
  1. Árvore montada a partir da lista plana (algoritmo do `menus.md §10.3`, fiel ao legado): raízes = `ID_PAGINA_PAI IS NULL` ordenadas por `ORDEM`; filhos recursivos ordenados por `ORDEM`;
  2. **Cache da árvore** por perfil: `IMemoryCache`, chave `menu:tree:{idPerfil}`, TTL 10 min (D-08);
  3. "Mais Acessados" e "usuário/B.I." calculados **fora** do cache (sempre frescos).
- **RF01.5** — Falha ao consultar o banco **não pode** retornar menu vazio silencioso: logar via `ILogger` e propagar erro 500 (correção D-03). Só `401` quando o claim de perfil está ausente (não mascara erro de banco como redirecionamento ao login — D-09).

#### Frontend

- **RF01.6** — `menuService.getMenu()` (Axios, `GET /menu`) chamado pelo `menuStore` no carregamento do layout.
- **RF01.7** — Estado do `menuStore`: `menu`, `maisAcessados`, `usuario`, `carregando`, `erro`, `ultimaCarga`.
- **RF01.8** — Fallback: manter o `FALLBACK_MENU` estático atual enquanto a API não responde; não bloquear a navegação.

---

### RF02 — Painel "Mais Acessados" (Top 10 do Usuário)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `database-engineer` (SQL) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)
**RNs**: RN-07, RN-08 | **Defeitos**: D-04, D-05

#### Backend

- **RF02.1** — Novo método `AcessoRepository.GetPaginasMaisAcessadasAsync(idUsuario, idPerfil)` — SQL 5.2 do legado **corrigido**:
  - `INNER JOIN ACESSO_PERFIL_PAGINA` para **filtrar por perfil** (D-05 — página revogada não pode mais aparecer);
  - `ORDER BY avp.QUANTIDADE DESC` + `FETCH FIRST 10 ROWS ONLY` (D-04 — ordenação explícita);
  - `ATIVO = 'S'` (RN-02);
  - Novo model `PaginaAcesso` (herda `Pagina` + `Quantidade`).
- **RF02.2** — Incluído no `GET /api/v1/menu` (campo `maisAcessados`). Cada item expõe `idPagina`, `chaveControle`, `tituloMenu`, `tituloAba`, `url`, `quantidade`.

#### Frontend

- **RF02.3** — Novo painel **"Mais Acessados"** (`MaisAcessadosPanel.tsx`): lista plana (Ant Design `List` ou `Menu` modo vertical), ordenada por quantidade, com ícone e tooltip.
- **RF02.4** — Clique em um item → navega para a rota SPA (via `ROUTE_MAP`) **e** registra telemetria (RF03).
- **RF02.5** — Empty state: "Nenhuma página acessada ainda." quando lista vazia.
- **RF02.6** — Posicionamento: seção abaixo do menu principal (mesmo `Sider`), separada visualmente; permanece visível quando o menu está expandido e some quando colapsado.

---

### RF03 — Registro de Acesso (Telemetria) — `POST /api/v1/menu/acessos`

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `backend-engineer` (endpoint) + `frontend-engineer` (cliente)
**RNs**: RN-07, RN-08, RN-12 | **Defeitos**: D-03

#### Backend

- **RF03.1** — Endpoint `POST /api/v1/menu/acessos` com body `{ "chaveControle": "..." }`.
- **RF03.2** — Fluxo do service:
  1. Extrair `ID_USUARIO` e `ID_PERFIL` do JWT;
  2. Resolver `chaveControle` → `idPagina` **e** validar permissão via `GetPaginaByChaveAsync(chaveControle, idPerfil)` (RN-12);
  3. Sem permissão → `403 Forbidden` (**não registra** acesso não autorizado);
  4. Com permissão → `RegistraAcessoPaginaAsync(idUsuario, idPagina)` — **upsert** (MERGE) idêntico ao SQL 5.3 do legado (RN-08): 1ª abertura → INSERT com `quantidade = 1`; seguintes → `quantidade + 1`.
- **RF03.3** — Retornar `204 No Content` em sucesso; `400 BadRequest` se `chaveControle` vazio.
- **RF03.4** — Efeito líquido igual ao legado; usar `MERGE` (upsert) mantendo a semântica RN-08.

#### Frontend

- **RF03.5** — `menuService.registrarAcesso(chaveControle)` chamado **fire-and-forget** ao abrir uma página pelo menu (não bloquear navegação; falha de telemetria não interrompe o fluxo — D-03 corrigido no frontend com `catch` silencioso + log).
- **RF03.6** — **Deduplicação por `CHAVE_CONTROLE`** (D-11): a telemetria é registrada na **primeira** abertura da rota (não a cada re-navegação para a mesma chave), replicando a semântica "contador a cada abertura de aba nova" (RN-07).

---

### RF04 — Guard de Autorização por Página (Defesa em Profundidade)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `architect` + `backend-engineer`
**RNs**: RN-12 | **Defeitos**: D-03

#### Backend

- **RF04.1** — Criar `PaginaAutorizacaoRequirement` + `PaginaAutorizacaoHandler : AuthorizationHandler<PaginaAutorizacaoRequirement>`:
  - Lê `ID_PERFIL` do claim JWT;
  - Lê a chave de controle da rota (`context.Resource as HttpContext` → `Request.RouteValues["chaveControle"]`), ou de query string `?chaveControle=`;
  - Chama `IAcessoRepository.GetPaginaByChaveAsync(chave, idPerfil)`; encontrou → `context.Succeed`, senão `context.Fail()`.
- **RF04.2** — Registrar policy `PaginaAcesso` em `Program.cs` (`AddAuthorization` + `AddScoped<IAuthorizationHandler, PaginaAutorizacaoHandler>()`).
- **RF04.3** — Aplicar `.RequireAuthorization("PaginaAcesso")` no novo endpoint de autorização e documentar o padrão para as futuras rotas de módulo.
- **RF04.4** — Endpoint `GET /api/v1/paginas/{chaveControle}/autorizacao` protegido pela policy — retorna `200` se autorizado, `403` caso contrário (usado pelo guard de rota do frontend).
- **RF04.5** — **Nunca confiar apenas no frontend** para autorização (RN-12): o backend é a autoridade.

#### Frontend

- **RF04.6** — Guard de rota best-effort: antes de renderizar um módulo, verificar se a `chaveControle` do item está presente no `menu` carregado no `menuStore` (dados já filtrados pelo backend); ausente → redirecionar para `/sem-permissao`.
- **RF04.7** — Nova página `SemPermissaoPage` (mensagem "Sem permissão de acesso a esta página", link de volta ao Dashboard).

---

### RF05 — Barra do Usuário e Link Condicional do B.I.

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `backend-engineer` (dados) + `frontend-engineer` (UI)
**RNs**: RN-09, RN-11, RN-14

#### Backend

- **RF05.1** — `MenuUsuarioResponse` no `GET /api/v1/menu`: `{ login, nome, exibeLinkBi }`.
- **RF05.2** — `exibeLinkBi` calculado a partir de `USUARIO_EMPRESA`: `true` se o usuário possuir ≥1 vínculo (RN-09). Query: `LEFT JOIN USUARIO_EMPRESA BI ON BI.ID_USUARIO = U.ID_USUARIO` (SQL 5.5) — vínculo nulo → link oculto.

#### Frontend

- **RF05.3** — `AppLayout.tsx`: exibir o `login` na barra do usuário e, quando `exibeLinkBi = true`, o link/atalho **"Acesso ao B.I."** (ícone `FundOutlined`/`BarChartOutlined`), mantendo o dropdown existente (Alterar Senha / Sair) (RN-11, RN-14).
- **RF05.4** — O logout continua via `authStore.logout()` + redirect `/login` (RN-14). Troca de senha segue o fluxo atual (`/alterar-senha`) — sem iframe/popup DevExpress.

---

### RF06 — Refatoração do `SideMenu` (Contrato, Navegação e Ícones)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer` + `architect`
**RNs**: RN-06, RN-13 | **Defeitos**: D-10, D-11 | **Lacuna**: G-03

#### Frontend

- **RF06.1** — **Corrigir o contrato** (G-03): definir `MenuItem` em `types/api.ts` espelhando `MenuItemResponse` (`idPagina`, `chaveControle`, `tituloMenu`, `tituloAba`, `url`, `tooltip`, `ordem`, `filhos`). `SideMenu` passa a usar `tituloMenu` como `label` (o tipo atual `PaginaMenuItem` com `nome`/`icone` **não corresponde** ao retorno real — o menu hoje renderiza rótulos vazios).
- **RF06.2** — Navegação: clique em **folha** → `navigate(rota)` (RN-06: grupo expande/colapsa e nunca navega, mesmo com URL); clique em **grupo** → toggle do Ant Design.
- **RF06.3** — `ROUTE_MAP`: `Record<chaveControle, string>` mapeando `CHAVE_CONTROLE` → rota SPA (as `url` do banco são `.aspx` legadas, D-10). Fallback: transformar a `url` legada em rota SPA por convenção; registrar todo novo item de menu neste mapa.
- **RF06.4** — Ícones por heurística sobre `chaveControle`/`url`/`tituloMenu` (o banco **não tem coluna de ícone**) — refatorar `resolveIcon` para receber os 3 campos.
- **RF06.5** — Tooltip via `tooltip` do item (RN-05/`TOOLTIP`).
- **RF06.6** — `selectedKeys`/`openKeys` derivados da URL atual (manter a lógica existente do `SideMenu`).
- **RF06.7** — Persistir o estado `collapsed` do `Sider` no `localStorage` (melhoria de UX; não era possível no legado).

---

### RF07 — Cache do Menu por Perfil (Backend)

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `backend-engineer`
**Defeitos**: D-08

- **RF07.1** — `IMemoryCache` registrado (`builder.Services.AddMemoryCache()`) e injetado em `MenuService`.
- **RF07.2** — Chave `menu:tree:{idPerfil}`, TTL 10 minutos. Apenas a **árvore** é cacheada (mais acessados e usuário/B.I. sempre frescos).
- **RF07.3** — **Invalidação**: remover `menu:tree:{idPerfil}` ao salvar permissões do perfil (`PerfilService.SalvarPermissoesAsync`); remover `menu:tree:*` ao criar/atualizar/excluir página (`PaginaService`). Expor helper `IMenuCacheInvalidator` ou método `MenuService.Invalidate(idPerfil?)`.
- **RF07.4** — Falha de cache nunca quebra o endpoint: em exceção de leitura/gravação de cache, recomputar e logar aviso (Serilog).

---

### RF08 — Validações e Tratamento de Erros

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `backend-engineer` (backend) + `frontend-engineer` (frontend)
**RNs**: RN-10 | **Defeitos**: D-03, D-09

- **RF08.1** — Backend: `401` reservado para falta/invalidez do JWT ou claim; **erros de banco propagam como `500`** e são logados — nunca silenciados ou convertidos em redirect de login (D-03, D-09).
- **RF08.2** — Backend: `chaveControle` obrigatória no `POST /menu/acessos`; vazio → `400 BadRequest`.
- **RF08.3** — Frontend: erros de rede/menu exibidos via `message.error` (interceptor Axios já configurado); estado `erro` no `menuStore` para feedback de carregamento do menu sem quebrar o fallback estático.
- **RF08.4** — Frontend: falha no `registrarAcesso` é **silenciosa** (não interrompe navegação) — `catch` + `console.warn`.

---

## 4. Camada de Dados (Banco e Queries)

### 4.1 Objetos Oracle Consumidos (SEM alteração estrutural)

| Objeto | Tipo | Uso no menu |
|--------|------|-------------|
| `ACESSO_CADASTRO_PAGINA` | Tabela | Catálogo hierárquico (pais via `ID_PAGINA_PAI`, `ORDEM`, `ATIVO`, `CHAVE_CONTROLE`) |
| `ACESSO_PERFIL_PAGINA` | Tabela | Permissão página×perfil (RN-01) — filtro do menu e do "Mais Acessados" |
| `ACESSO_CADASTRO_USUARIO` | Tabela | `ID_USUARIO`, `ID_PERFIL`, `NOME`, `LOGIN` (contexto do menu) |
| `ACESSO_VISUALIZACAO_PAGINA` | Tabela | Telemetria / top 10 (RN-07/RN-08) |
| `USUARIO_EMPRESA` | Tabela | Link B.I. (RN-09) |

> ⚠️ **Pendência de validação (Fase 1, database-engineer)**: o `docs/menus.md §4.4` documenta a coluna `DATA_HORA`; o código atual (`RegistraAcessoPaginaAsync`) usa `DATA_ULTIMO_ACESSO`. **Validar o nome real da coluna no Oracle** antes da implementação e padronizar.

### 4.2 Models

```csharp
// Empresa.Data/Models/PaginaAcesso.cs (NOVO)
namespace Empresa.Data.Models;

public class PaginaAcesso : Pagina
{
    public int Quantidade { get; set; }
}
```

### 4.3 DTOs (Empresa.Api/DTOs)

```csharp
// Response/MenuResponse.cs
public class MenuResponse
{
    public List<MenuItemResponse> Menu { get; set; } = new();
    public List<MaisAcessadoItemResponse> MaisAcessados { get; set; } = new();
    public MenuUsuarioResponse Usuario { get; set; } = new();
}

// Response/MenuItemResponse.cs (recursivo)
public class MenuItemResponse
{
    public int IdPagina { get; set; }
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ToolTip { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public List<MenuItemResponse> Filhos { get; set; } = new();
}

// Response/MaisAcessadoItemResponse.cs
public class MaisAcessadoItemResponse
{
    public int IdPagina { get; set; }
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}

// Response/MenuUsuarioResponse.cs
public class MenuUsuarioResponse
{
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool ExibeLinkBi { get; set; }
}

// Request/RegistroAcessoMenuRequest.cs
public class RegistroAcessoMenuRequest
{
    [Required(ErrorMessage = "chaveControle é obrigatória.")]
    public string ChaveControle { get; set; } = string.Empty;
}
```

### 4.4 Contrato JSON do `GET /api/v1/menu`

```json
{
  "menu": [
    {
      "idPagina": 10,
      "chaveControle": "cadastroPerfil",
      "tituloMenu": "Cadastro de Perfil",
      "tituloAba": "Perfis",
      "url": "interna/CadastroPerfil.aspx",
      "tooltip": "Manutenção de perfis de acesso",
      "ordem": 1,
      "filhos": []
    }
  ],
  "maisAcessados": [
    { "idPagina": 42, "chaveControle": "...", "tituloMenu": "...", "tituloAba": "...", "url": "...", "quantidade": 87 }
  ],
  "usuario": { "login": "fulano", "nome": "Fulano", "exibeLinkBi": true }
}
```

### 4.5 Algoritmo de montagem da árvore (fiel ao legado — `menus.md §10.3`)

```
entrada: lista plana de Pagina (resultado do CONNECT BY)
saída:  MenuItemResponse[]

raizes = paginas.filtrar(p => p.IDPaginaPai is null).ordenar(p => p.Ordem)
para cada raiz:
    no = criarMenuItem(raiz)          # tituloMenu, chaveControle, url, tituloAba, tooltip, ordem
    preencherFilhos(flatList, no, raiz.IDPagina)

preencherFilhos(flatList, noPai, idPai):
    filhos = flatList.filtrar(p => p.IDPaginaPai == idPai).ordenar(p => p.Ordem)
    para cada filho:
        noFilho = criarMenuItem(filho)
        preencherFilhos(flatList, noFilho, filho.IDPagina)   # recursão ilimitada (RN-04)
        noPai.Filhos.adicionar(noFilho)
```

---

## 5. Endpoints da API

### 5.1 Endpoints do Módulo Menu

| Método | Endpoint | Descrição | RF |
|--------|---------|-----------|-----|
| `GET` | `/api/v1/menu` | Shell do menu: árvore hierárquica (cacheada) + mais acessados + usuário/B.I. | RF01, RF02, RF05 |
| `POST` | `/api/v1/menu/acessos` | Upsert de telemetria `{ chaveControle }` — registra acesso do usuário (RN-08) | RF03 |
| `GET` | `/api/v1/paginas/{chaveControle}/autorizacao` | Checagem de permissão para guard de rota do frontend (policy `PaginaAcesso`) | RF04 |

### 5.2 Autenticação e Permissão

- Todos os endpoints exigem `Authorization: Bearer <jwt>` (`.RequireAuthorization()`).
- `GET /api/v1/menu` e `POST /api/v1/menu/acessos` usam `ID_PERFIL`/`ID_USUARIO` dos claims (stateless — sem `Session`).
- `GET /api/v1/paginas/{chave}/autorizacao` usa a policy `PaginaAcesso` (RN-12).

---

## 6. Correções de Defeitos do Legado (Decisões D-01 a D-12)

| # | Defeito | Decisão | Ação / RF |
|---|---------|---------|-----------|
| D-01 | SQL injection (`String.Format` concatenação) | ✅ **Corrigir** | Bind variables obrigatórias (Dapper) — já adotado no código atual; mantido nos novos métodos |
| D-02 | Senha em texto plano | ⏸️ **Fora do escopo** (pendência Fase 2/AuthService) | Migração BCrypt gradual — já documentada; menu não toca em senha |
| D-03 | Exceções engolidas → menu vazio silencioso | ✅ **Corrigir** | `ILogger` + propagar 500 (RF01.5, RF08.1) |
| D-04 | "Mais Acessados" sem `Ordem`/ordenação correta | ✅ **Corrigir** | `ORDER BY avp.QUANTIDADE DESC` explícito (RF02.1) |
| D-05 | "Mais Acessados" não filtra por perfil | ✅ **Corrigir** | `JOIN ACESSO_PERFIL_PAGINA` (RF02.1) |
| D-06 | Pai do UNION não checa `ATIVO='S'` | ✅ **Corrigir** (validar impacto na Fase 1) | `WHERE p.ATIVO='S'` também nos ancestrais (RF01.3) |
| D-07 | Subida de hierarquia limitada a 1 nível | ✅ **Corrigir** | `CONNECT BY PRIOR` — profundidade ilimitada (RF01.3) |
| D-08 | Menu remontado a cada request | ✅ **Corrigir** | `IMemoryCache` por perfil + invalidação (RF07) |
| D-09 | Catch genérico redireciona ao login | ✅ **Corrigir** | Distinguir `401` (auth) de erro de banco (500) (RF08.1) |
| D-10 | `target` do TreeViewNode abusado como transportador | ✅ **Corrigido na stack** | DTO estruturado (`MenuItemResponse`) + `ROUTE_MAP` (RF06.3) |
| D-11 | Deduplicação de abas por título | ✅ **Corrigir** | Deduplicação por `CHAVE_CONTROLE` (RF03.6) |
| D-12 | Driver `System.Data.OracleClient` depreciado | ✅ **Já corrigido** | ODP.NET Core (`Oracle.ManagedDataAccess.Core`) |

### Decisões de comportamento (adaptaço do legado para o SPA)

| Comportamento legado | Decisão nova stack |
|----------------------|--------------------|
| Abas com iframe (máx. 10) + colapso do menu ao abrir página (RN-13) | **Adiado**: navegação por router SPA (1 rota ativa); sem iframes. Deduplicação por `CHAVE_CONTROLE` fica pronta para abas futuras (RF03.6). O menu permanece visível (UX do SPA). |
| Menu à **direita** (splitter 400px) | Mantido **à esquerda** no novo layout (decisão já consolidada no `AppLayout`). Não é regra de negócio. |
| Troca de senha via popup no pane do menu | Mantido o fluxo atual: rota `/alterar-senha` (sem iframe). |

---

## 7. Métricas de Aceite (Definition of Done)

- [ ] `dotnet build` sem erros e sem warnings; `dotnet test` ≥ 80% de cobertura no novo código.
- [ ] `npm run build` e `npm run lint` sem erros (frontend).
- [ ] **Fidelidade (checklist `menus.md §10.4`)**: para um mesmo perfil, a saída da nova query (`CONNECT BY`) é **idêntica** à do SQL 5.1 legado (UNION) — validado com dados reais.
- [ ] Pais implícitos aparecem quando ao menos 1 filho é permitido, inclusive hierarquias ≥3 níveis (RN-03, D-07).
- [ ] Ordenação por `ORDEM` em cada nível idêntica (RN-05).
- [ ] Grupo não navega; folha navega (RN-06).
- [ ] "Mais Acessados": máximo 10, ordenado por `quantidade` DESC, filtrado por perfil (RN-07, D-04, D-05).
- [ ] Telemetria: 1ª abertura → `quantidade = 1`; repetida → `quantidade + 1` (RN-08); acesso não autorizado **não** é registrado (RN-12).
- [ ] Link B.I. condicional a `USUARIO_EMPRESA` (RN-09).
- [ ] Acesso direto a rota sem permissão bloqueado no backend (policy `PaginaAcesso`) — RN-12.
- [ ] Nenhuma alteração estrutural nas 5 tabelas Oracle (Regra de Ouro nº 1).
- [ ] Cache do menu invalidado ao salvar perfil/página (D-08).
- [ ] Menu renderiza rótulos/ícones corretos (G-03 fechado).
- [ ] Login sem perfil no JWT → `401`; erro de banco → `500` logado (D-03, D-09).

---

## 8. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|-------------|---------|-----------|
| `CONNECT BY` não reproduzir exatamente a saída do UNION legado (pais implícitos) | Média | Alto | Fase 1: validar contra dados reais; se divergir, usar CTE recursiva com a mesma semântica do UNION |
| Nome da coluna de data em `ACESSO_VISUALIZACAO_PAGINA` (`DATA_HORA` vs `DATA_ULTIMO_ACESSO`) | Média | Alto | Task DB na Fase 1 para validar o schema real antes da implementação |
| `ROUTE_MAP` manual (chave→rota) desatualizar quando novas páginas forem criadas | Média | Médio | Fallback por convenção da `url`; documentar obrigatoriedade no mapa; check no code review |
| Cache stale após alteração de permissões | Baixa | Médio | Invalidação explícita em `SalvarPermissoesAsync` e CRUD de página (RF07.3) |
| Contrato frontend já desalinhado hoje (G-03) gerar regressão visual | Média | Médio | RF06.1 corrige o tipo; validar visual em QA |
| Aplicar `ATIVO='S'` aos ancestrais (D-06) ocultar grupos que hoje aparecem | Baixa | Médio | Validar impacto na Fase 1; se crítico, aplicar `ATIVO` apenas nas folhas e documentar |
| Performance do `GET /menu` com muitas páginas | Baixa | Baixo | Cache por perfil + 1 request; queries indexadas pelas FKs existentes |

---

## 9. Plano de Execução (Orquestrado)

| Fase | Descrição | Agentes | Tasks |
|------|-----------|---------|-------|
| **Fase 1** | Camada de Dados | `orchestrator` → `database-engineer` | DB-01 a DB-06 |
| **Fase 2** | Camada de API | `orchestrator` → `backend-engineer` | BE-01 a BE-10 |
| **Fase 3** | Camada Frontend | `orchestrator` → `frontend-engineer` + `architect` | FE-01 a FE-10 |
| **Fase 4** | Qualidade & Testes | `orchestrator` → `qa-engineer` | QA-01 a QA-06 |
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

### 9.2 Tasks por Fase

#### Fase 1 — Camada de Dados (`database-engineer`)

| ID | Tarefa |
|----|--------|
| DB-01 | Validar schema real: colunas de `ACESSO_VISUALIZACAO_PAGINA` (`DATA_HORA` vs `DATA_ULTIMO_ACESSO`), índices de `ACESSO_PERFIL_PAGINA` e `ACESSO_CADASTRO_PAGINA`, PK/triggers |
| DB-02 | Reescrever `AcessoRepository.GetPaginasMenuAsync` com `START WITH` + `CONNECT BY PRIOR` (pais implícitos RN-03, profundidade ilimitada D-07, `ATIVO='S'` nos ancestrais D-06) |
| DB-03 | Validar fidelidade: comparar saída da nova query vs UNION legado (SQL 5.1) para um perfil real; documentar divergências |
| DB-04 | Criar model `PaginaAcesso.cs` e novo `AcessoRepository.GetPaginasMaisAcessadasAsync(idUsuario, idPerfil)` (filtro perfil D-05 + ordenação D-04 + `FETCH FIRST 10`) |
| DB-05 | Ajustar `RegistraAcessoPaginaAsync` para o nome real da coluna de data e manter upsert MERGE (RN-08) |
| DB-06 | Aplicar skills Oracle: bind variables, sem `SELECT *`, tratamento Oracle NULL → C# nullable |

#### Fase 2 — Camada de API (`backend-engineer`)

| ID | Tarefa |
|----|--------|
| BE-01 | Criar DTOs: `RegistroAcessoMenuRequest`, `MenuResponse`, `MenuItemResponse`, `MaisAcessadoItemResponse`, `MenuUsuarioResponse` |
| BE-02 | Criar `IMenuService`/`MenuService` — montar árvore (algoritmo §4.5), orquestrar mais acessados + usuário/B.I. |
| BE-03 | Implementar cache da árvore por perfil (`IMemoryCache`, chave `menu:tree:{idPerfil}`, TTL 10min) + `Invalidate(idPerfil?)` (D-08) |
| BE-04 | Criar `MenuEndpoints.cs`: `GET /api/v1/menu`, `POST /api/v1/menu/acessos` (com autorização por chave antes do upsert) |
| BE-05 | Criar `PaginaAutorizacaoRequirement` + `PaginaAutorizacaoHandler` (policy `PaginaAcesso`) e endpoint `GET /api/v1/paginas/{chaveControle}/autorizacao` (RN-12) |
| BE-06 | Registrar DI em `Program.cs`: `AddMemoryCache`, `AddScoped<IMenuService, MenuService>`, `AddScoped<IAuthorizationHandler, PaginaAutorizacaoHandler>`, policy |
| BE-07 | Invalidação de cache em `PerfilService.SalvarPermissoesAsync` e no CRUD de `PaginaService` (D-08) |
| BE-08 | `dotnet build` 0 erros; endpoints com `.RequireAuthorization()`, `.WithTags("Menu")`, `.WithSummary()`, `.WithDescription()`, `.Produces<>()` |
| BE-09 | `GET /api/v1/menu` trata 401 (claim ausente) vs 500 (erro de banco) sem engolir exceções (D-03/D-09) |
| BE-10 | Documentar padrão `PaginaAcesso` para adoção nas rotas de módulos futuros |

#### Fase 3 — Camada Frontend (`frontend-engineer` + `architect`)

| ID | Tarefa |
|----|--------|
| FE-01 | Atualizar `types/api.ts`: `MenuResponse`, `MenuItem`, `MaisAcessadoItem`, `MenuUsuario` (corrige G-03) |
| FE-02 | Criar `services/menuService.ts` — `getMenu()`, `registrarAcesso(chave)` (fire-and-forget) |
| FE-03 | Criar `store/menuStore.ts` — Zustand: `menu`, `maisAcessados`, `usuario`, `carregando`, `erro`, cache TTL, `carregarMenu`, `registrarAcesso` |
| FE-04 | Refatorar `SideMenu.tsx` — usar `tituloMenu`, `resolveIcon(chave,url,nome)`, tooltip, `ROUTE_MAP`, telemetria no clique de folha (RN-06, D-11) |
| FE-05 | Criar `MaisAcessadosPanel.tsx` — top 10 com ícones, empty state, clique navega + registra acesso (RF02) |
| FE-06 | Atualizar `AppLayout.tsx` — link condicional B.I. (`exibeLinkBi`) + persistir `collapsed` no localStorage (RF05, RF06.7) |
| FE-07 | Criar `SemPermissaoPage.tsx` + guard de rota best-effort via `menuStore` (RF04.6–7) |
| FE-08 | Implementar `ROUTE_MAP` (chaveControle → rota SPA) cobrindo as rotas existentes (`/`, `/usuarios`, `/perfis`, `/configuracoes/acesso-bi`, placeholders) |
| FE-09 | Tratar erros/loading: fallback estático mantido, `message.error` no interceptor, `console.warn` na telemetria |
| FE-10 | `npm run lint` e `npm run build` sem erros |

#### Fase 4 — Qualidade & Testes (`qa-engineer`)

| ID | Tarefa |
|----|--------|
| QA-01 | `AcessoRepositoryTests` — CONNECT BY (pais implícitos, ≥3 níveis), mais acessados (filtro perfil, limite 10, ordenação) |
| QA-02 | `MenuServiceTests` — montagem da árvore (RN-05), cache (hit/miss/invalidação), exceções logadas (D-03) |
| QA-03 | `PaginaAutorizacaoHandlerTests` — autorizado/negado/sem claim (RN-12) |
| QA-04 | Testes de telemetria — 1ª abertura insert, repetida incrementa, não autorizada não registra (RN-08) |
| QA-05 | Frontend (Vitest) — `menuStore` (carregar/invalidar/fallback), `ROUTE_MAP` resolvendo chave→rota |
| QA-06 | Checklist de fidelidade (`menus.md §10.4`) documentado + `dotnet build`/`dotnet test`/`npm run build` |

#### Fase 5 — Review & Entrega (`architect` + `devops-engineer`)

| ID | Tarefa |
|----|--------|
| RE-01 | Code review completo (backend + frontend) — contratos do `CLAUDE.md`, DI, async/await, DTOs vs Models, nomenclatura |
| RE-02 | Documentação: atualizar `CLAUDE.md`/`docs/README.md` se necessário, Swagger dos novos endpoints |
| RE-03 | Build de produção: `dotnet publish` + `npm run build` sem erros |
| RE-04 | Validação final da fidelidade RN-01 a RN-14 + decisões D-01 a D-12 |

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

- `docs/menus.md` — Especificação técnica do menu legado (462 linhas, engenharia reversa verificada), incluindo:
  - 5 tabelas Oracle e diagrama de relacionamento (§4)
  - 6 consultas SQL exatas (§5) — contratos de dados a reproduzir
  - 14 regras de negócio (RN-01 a RN-14) — contrato a preservar
  - 12 defeitos latentes (D-01 a D-12) — decisões na seção 6
  - Blueprint de reprodução na nova stack (§10) — endpoints, mapeamento, algoritmo, checklist de fidelidade

### Anexo B — Stack Legada (para referência)

| Item | Legado | Novo |
|------|--------|------|
| Framework | ASP.NET WebForms (.NET 4.8) + DevExpress 16.2 | ASP.NET Core Minimal API (.NET 9) + React 18 |
| Orquestração do menu | `Principal.aspx` + `Principal.aspx.cs` (code-behind) | `MenuService` + `MenuEndpoints` |
| ORM | `System.Data.OracleClient` (SQL inline) | Dapper + `Oracle.ManagedDataAccess.Core` (bind variables) |
| UI | `ASPxTreeView` + abas `tab-view.js` com iframe | Ant Design `Menu`/`List` + React Router |
| Estado | `Session["User"]` / `Session["UserLogado"]` / `Session.Timeout=60` | JWT stateless (claims `ID_USUARIO`, `ID_PERFIL`) |
| Guard por página | `GetPaginaByChave` em ~60 `Page_Load` | Policy `PaginaAcesso` (AuthorizationHandler) |
| Telemetria | `callAtualizar.PerformCallback` (callback) | `POST /api/v1/menu/acessos` (MERGE upsert) |
| Mais Acessados | `GetPaginasAcessadas` (sem filtro de perfil) | `GetPaginasMaisAcessadasAsync` (com filtro — D-05) |
| Posição | Pane `MENU` à direita (400px) | `Sider` à esquerda (240px, colapsável) |

---

*PRD gerado por orquestração multi-agente: Analista (mapeamento do `menus.md` + exploração do repositório) → Arquiteto (tradução para a stack atual, lacunas G-01 a G-03, decisões D-01 a D-12) → Redator (este documento). Nenhum código de produção foi alterado.*
