# PRD — Módulo Agrícola (Cad. Safra/Meta + Compras Frutas) · Multi-Agent

| Campo | Valor |
|-------|-------|
| **Título** | Módulo Agrícola — Cadastro de Safra/Meta e Relatório de Compra de Frutas |
| **Projeto** | GestãoNew — Módulo Agrícola |
| **Versão do PRD** | 1.0.1 |
| **Data** | 2026-08-13 |
| **Status** | Planejado |
| **Fonte de verdade** | `docs/agricola_regra.md` (engenharia reversa completa do legado) |
| **Chaves de permissão** | `cadastroSafraMeta` (painel 1) · `comprasFrutasPorEmpresas` (painel 2) |
| **Multi-Agent?** | Sim — Orquestrado via `agents/orchestrator.md` |
| **Agentes Ativos** | `orchestrator`, `architect`, `backend-engineer`, `database-engineer`, `frontend-engineer`, `qa-engineer`, `devops-engineer` |

---

> **Errata 1.0.1 (2026-08-14):** o painel migrado está vinculado no menu atual à chave `comprasFrutasPorEmpresas`, confirmada no `ROUTE_MAP`, no gate originalmente implementado e na página legada `ComprasFrutasPorEmpresas.aspx.cs`. As referências deste PRD a `comprasFrutas` como chave do painel moderno ficam substituídas por `comprasFrutasPorEmpresas`. O legado possui ambas as chaves para telas distintas.

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
│ │ Database         │  - Model MetaCompra                     │
│ │ Engineer Agent   │  - MetaCompraRepository (CRUD)          │
│ │                  │  - AgricolaRepository (3 REF CURSORs)   │
│ │                  │  - Validação de schema com DBA          │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 2: CAMADA DE API                                        │
│ ┌──────────────────┐                                         │
│ │ Backend          │  - DTOs (metas + grid dinâmica)         │
│ │ Engineer Agent   │  - MetaCompraService / AgricolaService  │
│ │                  │  - 8 endpoints REST                     │
│ │                  │  - Gates de permissão (2 chaves)        │
│ └──────────────────┘                                         │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 3: CAMADA FRONTEND                                      │
│ ┌──────────────────┐  ┌──────────────────┐                   │
│ │ Frontend         │  │ Architect        │                   │
│ │ Engineer Agent   │  │ Agent            │                   │
│ │ - SafraMetaPage  │  │ - Estrutura      │                   │
│ │ - ComprasFrutas  │  │ - Tipos TS       │                   │
│ │ - DynamicGrid    │  │ - Rotas          │                   │
│ └──────────────────┘  └──────────────────┘                   │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 4: QUALIDADE & TESTES                                   │
│ ┌──────────────────┐                                         │
│ │ QA Engineer      │  - Testes backend (xUnit)               │
│ │ Agent            │  - Testes frontend (Vitest)             │
│ │                  │  - Checklist de fidelidade              │
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
| **Database Engineer** | `agents/database-engineer.md` | Modelagem de dados, queries Dapper, procedures Oracle, validação de schema |
| **Frontend Engineer** | `agents/frontend-engineer.md` | Componentes React 18 + Ant Design 5, hooks Zustand, services Axios |
| **QA Engineer** | `agents/qa-engineer.md` | Testes unitários/integração (xUnit backend + Vitest frontend) |
| **DevOps Engineer** | `agents/devops-engineer.md` | Build de produção, validação final |

### 0.3 Regras de Delegação

- **Orchestrator NUNCA executa tarefas técnicas** — apenas coordena e reporta
- Cada RF referencia o **Orchestrator** como coordenador e indica o(s) **agente(s) executor(es)**
- Cada task no `TASKS.md` referencia o Orchestrator como agente orquestrador + agente especialista executor
- Tasks dentro da mesma fase podem ser executadas em paralelo quando não há dependências
- Handoff entre fases exige validação do Orchestrator (todas as tasks da fase anterior concluídas)
- `TASKS.md` é a fonte canônica de tarefas; `PROGRESS.md` é o dashboard de progresso

---

## 1. Visão do Produto

### 1.1 Resumo Executivo

O **Módulo Agrícola** reúne os dois painéis do menu *Agrícola* do ERP legado, documentados em `docs/agricola_regra.md`:

1. **Cad. Safra/Meta** (`cadastroSafraMeta` — título: *"Cadastro de Safra / Meta"*): tela de **CRUD** da tabela Oracle `META_COMPRAS`, que registra as **metas de volume de compra de frutas por safra, linha de produto e empresa** do grupo. É a contrapartida de "meta" para os volumes "realizados" exibidos no segundo painel.

2. **Compras Frutas** (`comprasFrutas` — título: *"Relatório de Compra de Frutas"*): painel **analítico read-only** de recebimento de frutas, alimentado pela package Oracle `pkg_bi_compras` (3 procedures com REF CURSOR), com **grid 100% dinâmica** (colunas definidas pelo cursor), **semáforo de linhas de totalização** e **drill-down em 3 níveis** (painel → detalhamento por variedade/grau → notas fiscais).

**Fonte de verdade**: `docs/agricola_regra.md` — engenharia reversa completa do legado (ASP.NET WebForms + DevExpress v16.2 + `System.Data.OracleClient`). Todas as queries, regras de negócio, comportamentos visuais e defeitos conhecidos estão documentados nesse arquivo.

### 1.2 Estado Atual (Gap Analysis)

Existe um **stub** do módulo implementado na Fase 8 que **não é fiel ao legado** e será **reescrevido** por este PRD:

| Artefato atual | Problema | Correção neste PRD |
|---|---|---|
| `AgricolaRepository.GetComprasFrutasAsync(int empresa, int? safra)` | Assinatura incorreta — a procedure real recebe **apenas `p_data`** (DateTime); o parâmetro `safra` é ignorado | DB-04 — reescrita com a assinatura fiel (`p_data` IN + `r_resultado` OUT cursor) |
| String mágica `"packageCompras.sp_recebimento_frutas"` | `packageCompras` era a **chave de appSetting** do legado, que resolve para `pkg_bi_compras.`; viola a regra "sem strings mágicas" | DB-05 — constantes em `OracleProcedures.cs` |
| Model `CompraFruta` (Safra/Produto/Quantidade/Valor/Data) | Não corresponde às colunas do REF CURSOR (grid dinâmica: `EMPRESA`, `LINHA`, `UF`, `DESCRICAO`, `VARIEDADE`, `QTDE_D0..D4`...) | DB-02/DB-04 — contrato dinâmico (linhas como dicionário) |
| Endpoint `GET /api/v1/agricola/compras-frutas?empresa=&safra=` | Sem autorização, sem gate de página, contrato errado | BE-05/BE-06 — substituído pelos endpoints fiéis |
| Não existe CRUD de `META_COMPRAS` | Painel 1 ausente | DB-03, BE-02/BE-04, FE-05/FE-06 |
| Frontend `/agricola/*` | Placeholder `ComingSoon` | FE-05 a FE-12 |
| `ROUTE_MAP` | Sem entradas `cadastroSafraMeta` / `comprasFrutas` (menu resolve por fallback kebab-case) | FE-12 |

### 1.3 Objetivos SMART

- **Específico**: Reconstruir os 2 painéis do módulo Agrícola na stack moderna (.NET 9 Minimal API + React 18 + Ant Design 5 + Dapper + Oracle), com fidelidade funcional ao legado e correção dos defeitos documentados (seção 6).
- **Mensurável**:
  - 8 endpoints REST versionados (`/api/v1/agricola/...`).
  - 2 repositórios Dapper (`IMetaCompraRepository`, `IAgricolaRepository` reescrito).
  - 2 services (`IMetaCompraService`, `IAgricolaService` reescrito).
  - 1 módulo frontend `agricola/` com 2 páginas + grid dinâmica reutilizável.
  - Cobertura de testes ≥ 80% no novo código.
  - `dotnet build`, `dotnet test`, `npm run lint` e `npm run build` sem erros.
- **Atingível**: Toda a infraestrutura necessária já existe (JWT, gate por perfil via `AcessoRepository.GetPaginaByChaveAsync`, `DbSession`, padrão de procedures com REF CURSOR da skill `oracle-procedures-dapper.md`, `MenuGuard`, `ROUTE_MAP`). As 3 procedures Oracle **já existem em produção** — consumo read-only, sem DDL.
- **Relevante**: Elimina WebForms/DevExpress/`Session`, fecha a brecha de segurança do endpoint atual sem autorização, adiciona validação server-side inexistente no legado e integra os painéis ao menu data-driven (Fase 18).
- **Temporal**: Próxima fase do roadmap (Fase 19), após a Fase 18 (Menu Lateral).

### 1.4 Domínios de Negócio

| Domínio | Conceitos principais | Artefatos envolvidos |
|---|---|---|
| **Planejamento Agrícola** | Safra (período de colheita/compra), meta de quantidade, linha de produto/fruta | `META_COMPRAS` |
| **Recebimento de Frutas (B.I.)** | Volume recebido por dia (D0 a D-4), acumulado, dia anterior, participação %, variedade, grau, nota fiscal de entrada | `pkg_bi_compras` (3 procedures) |
| **Estrutura Corporativa** | Empresa do grupo (domínio fixo 2/200/300/700), UF do produtor | `VW_EMPRESA_NEW` (referência conceitual) |
| **Segurança / Controle de Acesso** | Página, perfil, permissão por chave de controle | `ACESSO_CADASTRO_PAGINA`, `ACESSO_PERFIL_PAGINA` |

**Conceito central:** `META_COMPRAS` guarda a **meta**; `pkg_bi_compras` entrega o **realizado**. A amarração meta × realizado, quando existe, ocorre **dentro da package Oracle** — a aplicação não faz join entre os dois mundos.

### 1.5 Escopo

#### Dentro do escopo
- **Backend:** Model `MetaCompra`; repositórios `MetaCompraRepository` (CRUD) e `AgricolaRepository` (reescrita fiel — 3 procedures); services; 8 endpoints; gates de permissão para `cadastroSafraMeta` e `comprasFrutas`; validações server-side.
- **Frontend:** Módulo `modules/agricola/` com `SafraMetaPage` (tabela + modal de formulário), `ComprasFrutasPage` (filtro por data + grid dinâmica + auto-refresh), `DynamicGrid` (renderização por metadados), modais de drill-down (detalhamento e notas fiscais), exportação CSV, entradas no `ROUTE_MAP` e rotas no `App.tsx`.
- **Correção dos defeitos do legado** (seção 6 — D-01 a D-12).
- **Reescrita do stub incorreto** da Fase 8 (tabela §1.2).

#### Fora do escopo
- Alteração estrutural no Oracle (tabelas, packages, triggers permanecem como estão — Regra de Ouro nº 1; qualquer DDL exige aprovação do `database-engineer`).
- Painel legado "Compras Frutas por Empresa" (`comprasFrutasPorEmpresas`) — **não documentado** em `agricola_regra.md`; será tratado em PRD próprio.
- Alteração do corpo da `pkg_bi_compras` (consumo "as-is").
- Validação de **sobreposição de safras** (regra inexistente no legado — ver D-03; se desejada pelo negócio, vira requisito novo em PRD futuro).
- Exportação `.xls` binária (substituída por CSV — ver D-08).

---

## 2. Arquitetura & Diretrizes Técnicas

### 2.1 Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| Backend API | .NET 9 + ASP.NET Core Minimal APIs |
| ORM / Acesso a Dados | Dapper 2.1.79 + `Oracle.ManagedDataAccess.Core` (ODP.NET Managed) |
| Banco de Dados | Oracle (schema existente, **sem alterações estruturais**) |
| Autenticação | JWT Bearer (Fase 2) |
| Autorização de página | Gate por perfil via `AcessoRepository.GetPaginaByChaveAsync` (mesmo padrão do módulo Acesso B.I.) |
| Logging | Serilog |
| Frontend Framework | React 18 + TypeScript (strict) |
| Build Frontend | Vite |
| UI Framework | Ant Design 5 (Table, DatePicker, Modal, Form, Select, InputNumber, Popconfirm) |
| Gerenciamento de Estado | Zustand |
| Roteamento | React Router DOM 6 |
| Requisições HTTP | Axios (interceptor JWT já configurado em `@/lib/api`) |
| Datas | dayjs |
| Testes | xUnit + Moq (backend) · Vitest (frontend) |

### 2.2 Estrutura de Diretórios

#### Backend (Empresa.Data + Empresa.Api)

```
Empresa.Data/
├── Models/
│   ├── MetaCompra.cs                    # NOVO — entidade META_COMPRAS
│   └── SecundariosModels.cs             # ALTERADO — remover model CompraFruta (incorreto)
├── Oracle/
│   └── OracleProcedures.cs              # ALTERADO — constantes pkg_bi_compras completas
└── Repositories/
    ├── IMetaCompraRepository.cs         # NOVO — CRUD de metas
    ├── MetaCompraRepository.cs          # NOVO — implementação Dapper
    ├── ISecundariosRepository.cs        # ALTERADO — IAgricolaRepository reescrita (3 métodos)
    └── SecundariosRepositories.cs       # ALTERADO — AgricolaRepository reescrito

Empresa.Api/
├── DTOs/
│   ├── Request/
│   │   ├── MetaCompraRequest.cs         # NOVO — create/update de meta
│   │   └── ComprasFrutasRequest.cs      # NOVO — parâmetros dos drill-downs (query)
│   └── Response/
│       ├── MetaCompraResponse.cs        # NOVO — linha da grid de metas
│       ├── EmpresaMetaResponse.cs       # NOVO — item do domínio fixo de empresas
│       └── GridDinamicaResponse.cs      # NOVO — contrato da grid dinâmica (colunas + linhas)
├── Services/
│   ├── IMetaCompraService.cs            # NOVO
│   ├── MetaCompraService.cs             # NOVO — validações + gate cadastroSafraMeta
│   └── SecundariosService.cs            # ALTERADO — IAgricolaService reescrita + gate comprasFrutas
└── Endpoints/
    ├── AgricolaEndpoints.cs             # NOVO — 8 endpoints do módulo
    └── SecundariosEndpoints.cs          # ALTERADO — remover grupo "Agrícola" legado (stub)
```

#### Frontend (Empresa.Web/src/)

```
src/
└── modules/
    └── agricola/                        # NOVO — módulo completo
        ├── components/
        │   ├── DynamicGrid.tsx               # Grid dinâmica (colunas/linhas do contrato)
        │   ├── SafraMetaForm.tsx             # Modal de criar/editar meta
        │   ├── DetalhamentoModal.tsx         # Drill-down nível 1 (variedade/grau)
        │   └── NotaFiscalModal.tsx           # Drill-down nível 2 (notas fiscais)
        ├── services/
        │   ├── safraMetaService.ts           # Axios — 5 chamadas (CRUD + empresas)
        │   └── comprasFrutasService.ts       # Axios — 3 chamadas (painel + 2 drill-downs)
        ├── hooks/
        │   ├── useSafraMeta.ts               # Zustand — estado do CRUD de metas
        │   └── useComprasFrutas.ts           # Zustand — estado do painel + auto-refresh
        ├── utils/
        │   ├── comprasFrutasFormat.ts        # Captions, formatos numéricos, semáforo (regras RF10)
        │   └── exportarCsv.ts                # Exportação client-side (D-08)
        ├── types.ts                          # Tipos TypeScript
        ├── SafraMetaPage.tsx                 # Painel 1 — "Cadastro de Safra / Meta"
        └── ComprasFrutasPage.tsx             # Painel 2 — "Relatório de Compra de Frutas"
```

### 2.3 Contratos Arquiteturais (8 regras — `CLAUDE.md`)

| # | Contrato | Aplicação neste PRD |
|---|----------|---------------------|
| 1 | **Clean Architecture** | `Api` → `Data`. Services dependem de interfaces de repositório. |
| 2 | **Dapper + Oracle** | CRUD de metas com SQL parametrizado (`:param`); 3 procedures consumidas via `CommandType.StoredProcedure` com REF CURSOR (skill `oracle-procedures-dapper.md`). Sem EF, sem ADO.NET fora do Dapper. |
| 3 | **DI nativa** | `IMetaCompraRepository`, `IMetaCompraService` registrados via `AddScoped` em `Program.cs` (padrão já usado para `IAgricolaRepository`). |
| 4 | **Nomenclatura** | `Empresa.Data.Repositories.MetaCompraRepository`, `Empresa.Api.Services.MetaCompraService`. Classes em inglês, comentários/docs em português. |
| 5 | **Async/Await** | Todo I/O `async Task`. Proibido `.Result`/`.Wait()`. |
| 6 | **Models vs DTOs** | `MetaCompra` é model de banco; endpoints recebem/devolvem `MetaCompraRequest`/`MetaCompraResponse`/`GridDinamicaResponse`. |
| 7 | **Tratamento de Erros** | `Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`, `Results.UnprocessableEntity()`, `Results.Forbid()`. Exceções Oracle capturadas pelo middleware global (nunca engolir — corrige defeito legado D-11). |
| 8 | **Senhas** | Não se aplica (módulo não manipula credenciais). |

---

## 3. Requisitos Funcionais Detalhados

### PAINEL A — Cad. Safra/Meta (`cadastroSafraMeta`)

---

### RF01 — Gate de Permissão do Painel Safra/Meta

**Prioridade**: P0 (BLOQUEANTE)
**Orquestrador**: `orchestrator` → delega para `backend-engineer`

- **RF01.1** — Todos os endpoints do painel exigem JWT (`.RequireAuthorization()`) e verificam, no service, a permissão da chave **`cadastroSafraMeta`** via `AcessoRepository.GetPaginaByChaveAsync(chave, idPerfil)` (mesmo padrão do `AcessoBiService`).
- **RF01.2** — O `idPerfil` é lido da claim do JWT. Sem permissão → `403 Forbidden`.
- **RF01.3** — A chave `cadastroSafraMeta` **já existe** em `ACESSO_CADASTRO_PAGINA` (ID 30 — confirmado pelos logs de telemetria da Fase 18). Não é necessário criá-la.
- **RF01.4** — Frontend: a rota `/agricola/safra-meta` é protegida por `MenuGuard` (defesa em profundidade — o backend é a autoridade, RN-12 da Fase 18).

---

### RF02 — Listagem de Metas (Grid Principal)

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `database-engineer` (repositório) + `backend-engineer` (endpoint) + `frontend-engineer` (UI)

#### Backend

- **RF02.1** — Endpoint `GET /api/v1/agricola/metas` — retorna todas as metas.
- **RF02.2** — Query semanticamente idêntica ao `sqlMetaCompras` do legado:

```sql
SELECT ID_META_COMPRA, CD_LINHA, SAFRA, DT_INICIAL, DT_FINAL, META_QTDE, CD_EMPRESA
  FROM META_COMPRAS
 ORDER BY SAFRA DESC
```

- **RF02.3** — Retornar `List<MetaCompraResponse>`:

```json
{
  "idMetaCompra": 1024,
  "cdLinha": 10,
  "safra": "2026",
  "dtInicial": "2026-01-01",
  "dtFinal": "2026-12-31",
  "metaQtde": 1500000,
  "cdEmpresa": 2
}
```

#### Frontend

- **RF02.4** — `<Table>` Ant Design com colunas: **EMPRESA** (nome, via lookup do RF03), **LINHA** (`cdLinha`), **SAFRA** (`safra`), **DATA INICIAL** (`DD/MM/YYYY` via dayjs), **DATA FINAL** (`DD/MM/YYYY`), **META** (`metaQtde`, formato inteiro pt-BR `N0`), **Ações** (Editar / Excluir).
- **RF02.5** — Paginação client-side de **200 registros** (fiel ao `PageSize="200"` do legado).
- **RF02.6** — **Destaque visual (Regra 4 do legado):** linhas cuja `SAFRA` seja igual ao **ano corrente** renderizadas em **negrito** (`rowClassName`).
- **RF02.7** — Botão **"Nova Safra/Meta"** no header — abre o modal `SafraMetaForm` (RF04).
- **RF02.8** — A coluna `ID_META_COMPRA` **nunca é exibida** (fiel ao legado), mas está presente no DTO para as operações de edição/exclusão.
- **RF02.9** — Loading (`Spin`/skeleton) e empty state ("Nenhuma meta cadastrada.").

---

### RF03 — Domínio Fixo de Empresas

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `backend-engineer`

- **RF03.1** — Endpoint `GET /api/v1/agricola/metas/empresas` — retorna o **domínio fixo** do legado (Regra 5 — a combo **não** respeita as empresas do usuário; `sqlEmpresa`/`sqlLinhas` eram configuração morta e **não serão reproduzidas** — D-01):

| `cdEmpresa` | `nome` |
|---|---|
| 2 | TECNOVIN DO BRASIL LTDA |
| 200 | SUVALAN SUCOS DE FRUTAS, INDUSTRIA E COM |
| 300 | SUMABRAS DO BRASIL LTDA |
| 700 | MAISONFORESTIER |

- **RF03.2** — A lista é constante **server-side** (fonte única de verdade), exposta como `List<EmpresaMetaResponse>`. Não consultar `VW_EMPRESA_NEW` (decisão D-02 — fidelidade ao comportamento efetivo do legado).
- **RF03.3** — Frontend usa a lista no `<Select>` do formulário (RF04) e para exibir o **nome da empresa** na grid (RF02.4).

---

### RF04 — Criação de Meta

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `database-engineer` + `backend-engineer` + `frontend-engineer`

#### Backend

- **RF04.1** — Endpoint `POST /api/v1/agricola/metas` — cria uma meta.
- **RF04.2** — Body (`MetaCompraRequest`, com DataAnnotations):

```json
{
  "cdLinha": 10,
  "safra": "2026",
  "dtInicial": "2026-01-01",
  "dtFinal": "2026-12-31",
  "metaQtde": 1500000,
  "cdEmpresa": 2
}
```

- **RF04.3** — **Validações server-side (correção do defeito legado — Regra 2/D-03):**
  - Todos os campos obrigatórios → `400 BadRequest` com mensagem específica (mensagens equivalentes às do legado: "Informe a Empresa", "Informe uma Linha", "Informe uma Safra", "Informe a Data de Inicio da Safra", "Informe a Data final da Safra", "Informe a Meta a Ser Cadastrada").
  - `dtInicial <= dtFinal` → `422 UnprocessableEntity` ("A Data Final deve ser maior ou igual à Data Inicial.") — **regra nova**, inexistente no legado.
  - `cdEmpresa` deve pertencer ao domínio fixo (RF03) → `400`.
  - **Não validar** sobreposição de safras nem duplicidade safra/linha/empresa (fora de escopo — ver §1.5).
- **RF04.4** — **Geração da PK (D-04):** o legado enviava `ID_META_COMPRA` nulo e presumia trigger/sequence no Oracle. A Fase 1 (DB-01) **deve confirmar com o DBA** (`USER_TRIGGERS`, `USER_SEQUENCES`):
  - Se existir trigger/sequence → `INSERT` sem a coluna PK + `RETURNING ID_META_COMPRA INTO :Id` (Padrão 3 da skill Oracle).
  - Se **não** existir → `INSERT` explícito com `META_COMPRAS_SEQ.NEXTVAL` **somente após aprovação do `database-engineer`** para criar a sequence (DDL — Regra de Ouro nº 1); registrar a decisão no `PROGRESS.md`.
- **RF04.5** — Retornar `201 Created` com o `MetaCompraResponse` criado (incluindo `idMetaCompra`).

#### Frontend

- **RF04.6** — Modal `SafraMetaForm` (Ant Design `Modal` + `Form`) com campos: Empresa (`Select` do RF03), Linha (`InputNumber`), Safra (`Input` ex.: "2026"), Data Inicial e Data Final (`DatePicker` formato `DD/MM/YYYY`), Meta (`InputNumber` min 0, formato inteiro).
- **RF04.7** — Validações client-side **espelhando** RF04.3 (mesmas mensagens), incluindo a regra `dtFinal >= dtInicial` (correção D-03).
- **RF04.8** — Sucesso: `message.success("Meta cadastrada com sucesso!")`, fechar modal, recarregar grid. Erro 400/422: exibir mensagem do backend via `message.error`.

---

### RF05 — Atualização de Meta

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `database-engineer` + `backend-engineer` + `frontend-engineer`

- **RF05.1** — Endpoint `PUT /api/v1/agricola/metas/{id}` — atualiza todos os campos editáveis.
- **RF05.2** — Query semanticamente idêntica ao Update legado:

```sql
UPDATE META_COMPRAS
   SET CD_LINHA = :CdLinha, SAFRA = :Safra, DT_INICIAL = :DtInicial,
       DT_FINAL = :DtFinal, META_QTDE = :MetaQtde, CD_EMPRESA = :CdEmpresa
 WHERE ID_META_COMPRA = :IdMetaCompra
```

- **RF05.3** — Mesmas validações do RF04.3. `id` inexistente → `404 NotFound`.
- **RF05.4** — `ID_META_COMPRA` **nunca** é editável (fiel ao legado — chave read-only).
- **RF05.5** — Frontend: mesmo modal `SafraMetaForm` em modo edição (campos pré-preenchidos); sucesso → `message.success("Meta atualizada com sucesso!")` + refresh.

---

### RF06 — Exclusão de Meta

**Prioridade**: P1
**Orquestrador**: `orchestrator` → `database-engineer` + `backend-engineer` + `frontend-engineer`

- **RF06.1** — Endpoint `DELETE /api/v1/agricola/metas/{id}` — exclusão **física** (fiel ao legado — não há coluna de inativação em `META_COMPRAS` e a Regra de Ouro nº 1 proíbe DDL):

```sql
DELETE FROM META_COMPRAS WHERE ID_META_COMPRA = :IdMetaCompra
```

- **RF06.2** — `id` inexistente → `404 NotFound`. Sucesso → `204 NoContent`.
- **RF06.3** — **Auditoria (correção D-05):** registrar via **Serilog** o evento de exclusão (usuário do JWT, `idMetaCompra`, payload da linha excluída, timestamp) — compensação application-level da ausência de auditoria no legado, **sem** criar trigger/tabela de log (proibido DDL).
- **RF06.4** — Frontend: `Popconfirm` com a mensagem do legado **"Deseja Excluir a Meta?"**; sucesso → `message.success("Meta excluída com sucesso!")` + refresh.

---

### RF07 — Rota e Menu do Painel Safra/Meta

**Prioridade**: P1
**Orquestrador**: `orchestrator` → `frontend-engineer`

- **RF07.1** — Rota SPA: `/agricola/safra-meta` → `SafraMetaPage` (lazy-loaded em `App.tsx`, protegida por `MenuGuard`).
- **RF07.2** — Registrar no `ROUTE_MAP`: `cadastroSafraMeta: '/agricola/safra-meta'` (elimina o fallback kebab-case — regra do `routeMap.ts`: "todo novo item de menu DEVE ser registrado aqui").
- **RF07.3** — O item de menu "Agrícola > Cad. Safra/Meta" é **data-driven** (Fase 18) — nenhum item hardcoded no `SideMenu`; basta o `ROUTE_MAP` + a página ativa no banco.
- **RF07.4** — Título da página: **"Cadastro de Safra / Meta"** (fiel ao legado).

---

### PAINEL B — Compras Frutas (`comprasFrutas`)

---

### RF08 — Gate de Permissão do Painel Compras Frutas

**Prioridade**: P0 (BLOQUEANTE)
**Orquestrador**: `orchestrator` → delega para `backend-engineer`

- **RF08.1** — Idem RF01, com a chave **`comprasFrutas`** (já existente em `ACESSO_CADASTRO_PAGINA` — não criar).
- **RF08.2** — Rota `/agricola/compras-frutas` protegida por `MenuGuard`.
- **RF08.3** — Painel **100% read-only**: nenhum endpoint de escrita neste painel.

---

### RF09 — Painel Principal (Nível 0) — Filtro e Consulta

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `database-engineer` (repositório) + `backend-engineer` (endpoint/service) + `frontend-engineer` (UI)

#### Backend

- **RF09.1** — Endpoint `GET /api/v1/agricola/compras-frutas?data=yyyy-MM-dd`.
- **RF09.2** — `data` **opcional**; default = dia corrente do servidor (fiel ao legado: `datePesquisa.Text = hoje`).
- **RF09.3** — O parâmetro de query é **ISO 8601** (`yyyy-MM-dd`) e desserializado diretamente para `DateTime` — **correção D-10** (o legado enviava string `"dd/MM/yyyy"` a parâmetro `DateTime`, com conversão implícita dependente de cultura/NLS).
- **RF09.4** — Repositório executa `pkg_bi_compras.sp_recebimento_frutas` (constante `OracleProcedures.PackageComprasBi` + `Procedures.SpRecebimentoFrutas` — **nunca** string mágica; corrige o stub §1.2):

| Parâmetro | Direção | Tipo | Origem |
|---|---|---|---|
| `p_data` | IN | `DateTime` | query `data` |
| `r_resultado` | OUT | REF CURSOR | — |

- **RF09.5** — **Contrato de grid dinâmica (D-06):** o REF CURSOR tem colunas **variáveis** (definidas pela package); a API retorna metadados + linhas genéricas, sem tipagem estática:

```json
{
  "dataReferencia": "2026-08-13",
  "ultimaAtualizacao": "2026-08-13T14:32:05",
  "colunas": ["EMPRESA", "LINHA", "UF", "DESCRICAO", "VARIEDADE", "EM_SAFRA", "DIA_ANTERIOR", "QTDE_D0", "QTDE_D1", "QTDE_D2", "QTDE_D3", "QTDE_D4", "ACUMULADO", "%_PART"],
  "linhas": [
    { "EMPRESA": "TECNOVIN", "LINHA": "UVAS", "UF": "RS", "DESCRICAO": "...", "VARIEDADE": "TO005 - ...", "DIA_ANTERIOR": 1200, "QTDE_D0": 1500, "ACUMULADO": 45000, "%_PART": 12.5 }
  ]
}
```

  - Implementação Dapper: `QueryAsync` sobre a procedure retorna `IEnumerable<dynamic>` (`DapperRow`) → serializar cada linha como `IDictionary<string, object>`.
  - `ultimaAtualizacao` = `DateTime.Now` do servidor **no momento da consulta** (fiel ao `lblUltimaAtualizacao` do legado).
- **RF09.6** — **Stateless (D-07):** nenhum cache server-side (o legado usava `Session["comprasFrutas"]` para paginação/exportação — eliminado; o frontend pagina/exporta com os dados já carregados).

#### Frontend

- **RF09.7** — Filtro único: `DatePicker` (`datePesquisa`), default = hoje, formato de exibição `DD/MM/YYYY`; ao trocar a data → refetch. Botão **"Recarregar"** (ícone `ReloadOutlined`) → refetch (equivalente ao `btnRecarregar` do legado).
- **RF09.8** — Label **"Última atualização: DD/MM/YYYY HH:mm:ss"** alimentado por `ultimaAtualizacao` da resposta.
- **RF09.9** — Grid renderizada pelo componente `DynamicGrid` (RF10), paginação client-side de **100 registros**, **ordenação desabilitada** (fiel ao `AllowSort="False"`).
- **RF09.10** — Título da página: **"Relatório de Compra de Frutas"**.

---

### RF10 — DynamicGrid — Renderização, Captions, Formatos e Semáforo

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `frontend-engineer`

Toda a lógica de apresentação do legado (que vivia em `FormataGrid`/`HtmlDataCellPrepared`) é portada para o utilitário **`comprasFrutasFormat.ts`** + componente **`DynamicGrid.tsx`**, com testes Vitest (QA-04):

- **RF10.1 — Caption base:** nome da coluna com `_` → espaço e prefixo `CD ` → `CODIGO ` (regra do `FormataGrid` legado).
- **RF10.2 — Colunas-dia (Regra 4 do legado, corrigindo defeito D-09):**

| Coluna | Caption |
|---|---|
| `QTDE_D0` | data pesquisada (`DD/MM/YYYY`) |
| `QTDE_D1` | D-1 (`DD/MM/YYYY`) |
| `QTDE_D2` | D-2 |
| `QTDE_D3` | D-3 |
| `QTDE_D4` | D-4 |
| `DIA_ANTERIOR` | data de D-1 — **exceto às segundas-feiras, quando é D-3** (o legado calculava D-3 e sobrescrevia com D-1; na nova stack o cálculo correto é aplicado) |

- **RF10.3 — Visibilidade/larguras:** coluna `EM_SAFRA` **oculta**; `VARIEDADE` com largura fixa **220px**; colunas com `%` no nome com **80px**.
- **RF10.4 — Formatação numérica (Regra 5):** `QTDE_D*` e `ACUMULADO` → inteiro pt-BR (`N0`); colunas com `%` → 1 casa decimal (`N1`); demais numéricas → inteiro; valor não conversível → exibir bruto (fiel ao fallback do legado).
- **RF10.5 — Semáforo de linhas (Regra 6):** pela coluna `DESCRICAO` (case-insensitive):
  - contém `LINHA` → fundo `#EEE9E9`
  - contém `EMPRESA` → fundo `#D3D3D3`
  - contém `GERAL` → fundo `#77889A`, fonte branca, **negrito** (linhas "TOTAL EMPRESA" / "TOTAL GERAL")
  - Precedência: `GERAL` > `EMPRESA` > `LINHA` (avaliação nessa ordem — uma linha "TOTAL GERAL" também contém substrings das demais regras dependendo do texto).
- **RF10.6 — Células clicáveis (Regra 7 — parte visual):** células das colunas `DIA_ANTERIOR`, `QTDE_D0` e `ACUMULADO` com **valor não vazio** recebem estilo "link" (cursor pointer + sublinhado — equivalente ao `rel="clikNeutro"` do `rotina.js`). `QTDE_D1`–`QTDE_D4` **não** são clicáveis.

---

### RF11 — Drill-Down Nível 1 — Detalhamento por Variedade/Grau (ação `DC`)

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `database-engineer` + `backend-engineer` + `frontend-engineer`

#### Backend

- **RF11.1** — Endpoint `GET /api/v1/agricola/compras-frutas/detalhamento` com query params: `data` (ISO), `empresa`, `linha`, `uf`, `colunaClicada`.
- **RF11.2** — `colunaClicada` ∈ {`ANTERIOR`, `QTDE_D0`, `ACUMULADO`} — validar; fora disso → `400`.
- **RF11.3** — Repositório executa `pkg_bi_compras.sp_recebimento_frutas_det`:

| Parâmetro | Tipo | Origem |
|---|---|---|
| `p_data_emissao` | DateTime | `data` |
| `p_empresa` | VarChar | `empresa` (valor da coluna `EMPRESA` da linha clicada) |
| `p_linha` | VarChar | `linha` |
| `p_uf` | VarChar | `uf` |
| `p_coluna_clicada` | VarChar | `colunaClicada` |
| `r_resultado` | OUT Cursor | — |

- **RF11.4** — Mesmo contrato dinâmico do RF09.5 (`colunas` + `linhas` + `ultimaAtualizacao`).

#### Frontend

- **RF11.5** — Abertura: clique em célula clicável (RF10.6) de linha **não-total** → `DetalhamentoModal` (Ant Design `Modal` **não modal-blocking**, largura ampla, título com contexto: empresa/linha/UF/coluna).
- **RF11.6** — Grid do modal via `DynamicGrid`, **sem paginação** (`pagination={false}` + scroll vertical — equivalente ao `ShowAllRecords` do legado).
- **RF11.7 — Formatação (Regra 14):** colunas contendo `MEDIO` ou `FATURADO` → 2 casas (`N2`); contendo `UNITARIO` → 3 casas (`N3`); demais → inteiro. Linhas cuja `VARIEDADE` contém "GERAL" → fundo `#77889A`, fonte branca, negrito.
- **RF11.8 — Caption `DIA_ANTERIOR`:** aplica a regra de segunda-feira (D-3) de RF10.2 **corretamente** (correção D-09).
- **RF11.9 — Drill nível 2 (Regra 15):** células cujo nome de coluna **não** contenha `MEDIO` e **não** contenha `VARIEDADE` (e valor não vazio) são clicáveis → abrem `NotaFiscalModal` (RF12) com `colunaGrauClicada` = **caption da coluna clicada** (como no legado) e `variedade` = valor da coluna `VARIEDADE` da linha.

---

### RF12 — Drill-Down Nível 2 — Notas Fiscais (ação `NF`)

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `database-engineer` + `backend-engineer` + `frontend-engineer`

#### Backend

- **RF12.1** — Endpoint `GET /api/v1/agricola/compras-frutas/notas-fiscais` com query params: `data`, `empresa`, `linha`, `uf`, `colunaClicada`, `colunaGrauClicada` (opcional), `variedade` (opcional).
- **RF12.2** — Repositório executa `pkg_bi_compras.sp_recebimento_frutas_det_nf`:

| Parâmetro | Tipo | Origem |
|---|---|---|
| `p_data_emissao` | DateTime | `data` |
| `p_empresa` | VarChar | `empresa` |
| `p_linha` | VarChar | `linha` |
| `p_uf` | VarChar | `uf` |
| `p_coluna_clicada` | VarChar | `colunaClicada` |
| `p_coluna_grau_clicada` | VarChar | `colunaGrauClicada` (nulo no fluxo NF direto) |
| `p_cd_variedade` | VarChar | **calculado no service** (RF12.3) |
| `r_resultado` | OUT Cursor | — |

- **RF12.3 — Regra de `p_cd_variedade` (Regra 17, movida para o backend — D-06):** no service, a partir de `variedade`:
  - `null`/vazio (fluxo NF direto) → `DBNull`;
  - primeiros **2 caracteres**; se começar com `"TO"` → **`null`**.
  - Parâmetro vazio/nulo → `DBNull.Value` (comportamento do DTO `Parametros` do legado).
- **RF12.4** — Contrato dinâmico idem RF09.5.

#### Frontend

- **RF12.5** — `NotaFiscalModal` (Ant Design `Modal` **modal**, fiel ao `popDetalhamentoGrauUva`).
- **RF12.6 — Formatação (Regra 18):** `MEDIO`/`FATURADO` → 2 casas; `UNITARIO` → 3 casas (`N3`); colunas contendo `DT` → data `DD/MM/YYYY`; `NR_NOTAFISCAL` → **texto livre**; demais → inteiro. Sem paginação + scroll vertical.
- **RF12.7** — Abertura por 2 fluxos (RF13): (a) clique em linha de **total** no painel principal (NF direto, sem `colunaGrauClicada`/`variedade`); (b) drill a partir do detalhamento (com ambos).

---

### RF13 — Roteamento do Drill-Down (Regras 7 e 8 do legado)

**Prioridade**: P0
**Orquestrador**: `orchestrator` → `frontend-engineer`

Regra de clique no **painel principal** (implementada no `ComprasFrutasPage`/hook):

| Coluna clicada | Tipo de linha | Destino | `colunaClicada` enviado |
|---|---|---|---|
| `DIA_ANTERIOR` | não-total | `DetalhamentoModal` (DC) | `ANTERIOR` |
| `DIA_ANTERIOR` | TOTAL EMPRESA / TOTAL GERAL | `NotaFiscalModal` (NF) | `ANTERIOR` |
| `QTDE_D0` | não-total | `DetalhamentoModal` (DC) | `QTDE_D0` |
| `QTDE_D0` | TOTAL EMPRESA / TOTAL GERAL | `NotaFiscalModal` (NF) | `QTDE_D0` |
| `ACUMULADO` | não-total | `DetalhamentoModal` (DC) | `ACUMULADO` |

- **RF13.1** — "Linha de total" = `DESCRICAO` contém `EMPRESA` ou `GERAL` (mesma detecção do semáforo RF10.5).
- **RF13.2** — Células vazias não são clicáveis; `QTDE_D1`–`QTDE_D4` nunca são clicáveis.
- **RF13.3** — Parâmetros enviados aos endpoints de drill: `data`, `empresa`, `linha`, `uf` (valores das colunas da linha clicada) + `colunaClicada` — equivalente à string pipe-delimitada do legado, agora tipada via query string.

---

### RF14 — Auto-Refresh e Timestamp de Atualização

**Prioridade**: P1
**Orquestrador**: `orchestrator` → `frontend-engineer`

- **RF14.1** — `Select` "Atualizar a cada" com as opções do legado: **30 Minutos** (1.800.000 ms) e **01 Hora** (3.600.000 ms), mais opção default **"Manual"** (desligado).
- **RF14.2** — Implementação via `setInterval` no hook `useComprasFrutas` (limpo no unmount/troca de opção) — substitui `ASPxTimer` + `ASPxCallback` (corrige o defeito do nome errado `tmrJobTempo`, D-09 item 4).
- **RF14.3** — A cada tick: refetch do painel principal → atualiza grid + label "Última atualização".

---

### RF15 — Exportação (CSV client-side — D-08)

**Prioridade**: P1
**Orquestrador**: `orchestrator` → `frontend-engineer`

- **RF15.1** — Botão **"Excel"** (`FileExcelOutlined`) em cada nível: painel principal, `DetalhamentoModal` e `NotaFiscalModal`.
- **RF15.2** — Exportação **client-side** dos dados já carregados, via util `exportarCsv.ts` (BOM UTF-8 + `;` como separador para compatibilidade com Excel pt-BR), gerando download:
  - Nível 0: **`comprasFrutas.csv`** (nome do legado: `comprasFrutas.xls`)
  - Nível 1: **`Compras Frutas.csv`**
  - Nível 2: **`DetalhamentoNotaFiscal.csv`**
- **RF15.3** — Valores exportados **formatados** (mesma formatação da tela) e com as captions visíveis (inclui renomeações de RF10.2).
- **RF15.4** — Correção do defeito legado item 5 (callback de exportação lia a chave de sessão errada e vinha vazio): aqui a fonte de dados é **a mesma** da tela — impossível divergir.

---

### RF16 — Rota e Menu do Painel Compras Frutas

**Prioridade**: P1
**Orquestrador**: `orchestrator` → `frontend-engineer`

- **RF16.1** — Rota SPA: `/agricola/compras-frutas` → `ComprasFrutasPage` (lazy + `MenuGuard`).
- **RF16.2** — `ROUTE_MAP`: `comprasFrutas: '/agricola/compras-frutas'`.
- **RF16.3** — Menu "Agrícola > Compras Frutas" data-driven (idem RF07.3).

---

## 4. Camada de Dados (Banco e Queries)

### 4.1 Objetos Oracle Consumidos

| Objeto | Tipo | Uso | Observação |
|--------|------|-----|-----------|
| `META_COMPRAS` | **Tabela** (CRUD) | Painel 1 | Colunas: `ID_META_COMPRA` (NUMBER, PK presumida), `CD_LINHA` (NUMBER), `SAFRA` (VARCHAR2), `DT_INICIAL` (DATE), `DT_FINAL` (DATE), `META_QTDE` (NUMBER), `CD_EMPRESA` (NUMBER) |
| `pkg_bi_compras.sp_recebimento_frutas` | Procedure (REF CURSOR) | Painel 2, nível 0 | `p_data` IN DateTime, `r_resultado` OUT Cursor |
| `pkg_bi_compras.sp_recebimento_frutas_det` | Procedure (REF CURSOR) | Painel 2, nível 1 | + `p_empresa`, `p_linha`, `p_uf`, `p_coluna_clicada` (VarChar) |
| `pkg_bi_compras.sp_recebimento_frutas_det_nf` | Procedure (REF CURSOR) | Painel 2, nível 2 | + `p_coluna_grau_clicada`, `p_cd_variedade` (VarChar) |
| `ACESSO_CADASTRO_PAGINA` / `ACESSO_PERFIL_PAGINA` | Tabelas | Gates (RF01/RF08) | Via `AcessoRepository` existente |

> ⚠️ **Sem DDL neste PRD.** As tabelas de origem da package (NFs de entrada, variedades etc.) são **encapsuladas** e não constam na base de código — consumo "as-is".

### 4.2 Nova Model (Empresa.Data/Models)

```csharp
// MetaCompra.cs
namespace Empresa.Data.Models;

public class MetaCompra
{
    public long IdMetaCompra { get; set; }
    public long CdLinha { get; set; }
    public string Safra { get; set; } = string.Empty;
    public DateTime DtInicial { get; set; }
    public DateTime DtFinal { get; set; }
    public decimal MetaQtde { get; set; }
    public long CdEmpresa { get; set; }
}
```

O model incorreto `CompraFruta` (SecundariosModels.cs) é **removido** — as consultas do painel 2 retornam linhas dinâmicas (`IDictionary<string, object>`), não entidade tipada (colunas variam conforme a package).

### 4.3 Contratos dos Repositórios

**`IMetaCompraRepository`** (Dapper, SQL direto):

| Método | SQL base | Fonte legado |
|---|---|---|
| `GetAllAsync()` | SELECT ... ORDER BY SAFRA DESC | `sqlMetaCompras` Select |
| `InsertAsync(MetaCompra)` | INSERT (sem PK) + `RETURNING ID_META_COMPRA INTO :Id` | Insert legado + D-04 |
| `UpdateAsync(MetaCompra)` | UPDATE ... WHERE ID_META_COMPRA | Update legado |
| `DeleteAsync(long id)` | DELETE ... WHERE ID_META_COMPRA | Delete legado |
| `GetByIdAsync(long id)` | SELECT ... WHERE ID_META_COMPRA | novo (suporte a 404/auditoria) |

**`IAgricolaRepository`** (reescrito — procedures com REF CURSOR):

| Método | Procedure | Fonte legado |
|---|---|---|
| `GetRecebimentoFrutasAsync(DateTime data)` | `pkg_bi_compras.sp_recebimento_frutas` | `DaoPainel.GetComprasFrutas` |
| `GetRecebimentoFrutasDetAsync(data, empresa, linha, uf, colunaClicada)` | `pkg_bi_compras.sp_recebimento_frutas_det` | `DaoPainel.GetDetalhesComprasFrutas` |
| `GetRecebimentoFrutasDetNfAsync(data, empresa, linha, uf, colunaClicada, colunaGrauClicada, cdVariedade)` | `pkg_bi_compras.sp_recebimento_frutas_det_nf` | `DaoPainel.GetDetalhesNotaFiscalComprasFrutas` |

Retorno de cada método: `(IEnumerable<IDictionary<string, object>> Linhas, IReadOnlyList<string> Colunas)` — nomes de coluna extraídos do primeiro resultado (metadados do cursor).

### 4.4 Constantes Oracle (OracleProcedures.cs — alteração)

```csharp
// Já existentes:
public const string PackageComprasBi = "pkg_bi_compras";   // agrícola
public const string SpRecebimentoFrutas = "sp_recebimento_frutas";

// NOVAS (nested Procedures):
public const string SpRecebimentoFrutasDet   = "sp_recebimento_frutas_det";
public const string SpRecebimentoFrutasDetNf = "sp_recebimento_frutas_det_nf";
```

Uso obrigatório: `$"{OracleProcedures.PackageComprasBi}.{OracleProcedures.Procedures.SpRecebimentoFrutas}"` — elimina a string mágica `"packageCompras.sp_recebimento_frutas"` do stub atual.

### 4.5 Regras Oracle Aplicáveis (skill `oracle-procedures-dapper.md`)

- `DynamicParameters` com bind `:nome` (Dapper mapeia por nome nos comandos Oracle); `CommandType.StoredProcedure`.
- REF CURSOR: parâmetro `DbType.Object` + `ParameterDirection.Output` (padrão já usado no stub); conexão **aberta** antes da execução.
- `await using`/`using` para recursos; `DbSession` por escopo (nunca compartilhar conexão entre requisições).
- Parâmetros VarChar vazios → `DBNull.Value` (equivalente ao `Parametros` legado).
- Datas: `DateTime` tipado (nunca string `"dd/MM/yyyy"` em parâmetro DateTime — correção D-10).

---

## 5. Endpoints da API

### 5.1 Mapa de Endpoints do Módulo

Grupo: `/api/v1/agricola` · Tag Swagger: **"Agrícola"** · Todos com `.RequireAuthorization()` + gate de permissão no service.

| Método | Endpoint | Descrição | Gate | RF |
|--------|---------|-----------|------|-----|
| `GET` | `/api/v1/agricola/metas` | Lista metas (ORDER BY SAFRA DESC) | `cadastroSafraMeta` | RF02 |
| `GET` | `/api/v1/agricola/metas/empresas` | Domínio fixo de empresas (4 itens) | `cadastroSafraMeta` | RF03 |
| `POST` | `/api/v1/agricola/metas` | Cria meta (validações + RETURNING PK) | `cadastroSafraMeta` | RF04 |
| `PUT` | `/api/v1/agricola/metas/{id:long}` | Atualiza meta | `cadastroSafraMeta` | RF05 |
| `DELETE` | `/api/v1/agricola/metas/{id:long}` | Exclui meta (físico + log Serilog) | `cadastroSafraMeta` | RF06 |
| `GET` | `/api/v1/agricola/compras-frutas?data=` | Painel principal (grid dinâmica) | `comprasFrutas` | RF09 |
| `GET` | `/api/v1/agricola/compras-frutas/detalhamento?data=&empresa=&linha=&uf=&colunaClicada=` | Drill nível 1 | `comprasFrutas` | RF11 |
| `GET` | `/api/v1/agricola/compras-frutas/notas-fiscais?data=&empresa=&linha=&uf=&colunaClicada=&colunaGrauClicada=&variedade=` | Drill nível 2 | `comprasFrutas` | RF12 |

### 5.2 Códigos de Resposta

| Cenário | Código |
|---|---|
| Sucesso em consultas | `200 OK` |
| Criação de meta | `201 Created` |
| Atualização/exclusão | `204 NoContent` |
| Campo obrigatório ausente / domínio inválido / `colunaClicada` inválida | `400 BadRequest` |
| `dtFinal < dtInicial` | `422 UnprocessableEntity` |
| Sem JWT ou sem permissão na chave | `401` / `403 Forbidden` |
| `id` de meta inexistente | `404 NotFound` |
| Erro Oracle (package inacessível, ORA-*) | `500` via middleware global (nunca engolir exceção) |

### 5.3 Migração do Endpoint Legado (stub)

O endpoint atual `GET /api/v1/agricola/compras-frutas?empresa=&safra=` (stub da Fase 8) é **substituído** pelo RF09 (mesmo path, contrato novo). Como o frontend atual é apenas um placeholder `ComingSoon`, **não há consumidor a quebrar**. Registrar a quebra de contrato no `PROGRESS.md` e em `docs/README.md` (tabela de endpoints).

---

## 6. Correções de Defeitos do Legado (Decisões)

| # | Origem (legado) | Decisão | RF/Task |
|---|---|---|---|
| D-01 | `sqlEmpresa`/`sqlLinhas` declarados e nunca usados | **Não reproduzir.** Domínio de empresas fixo via endpoint dedicado | RF03 |
| D-02 | Combo de empresa com 4 itens hardcoded (não respeita usuário) | **Manter** o domínio fixo (fidelidade), mas server-side como fonte única | RF03 |
| D-03 | Sem validação server-side; permitia `DT_FINAL < DT_INICIAL` | **Validar** obrigatoriedade + coerência de datas no backend (400/422) e espelhar no frontend. Sobreposição de safra/duplicidade: **fora de escopo** (regra nova — negócio deve solicitar) | RF04.3 |
| D-04 | PK `ID_META_COMPRA` presumida (trigger/sequence não confirmada) | **Confirmar com DBA** (DB-01) antes de codar o INSERT; usar `RETURNING ... INTO`; se não houver geração no banco, propor sequence com aprovação do `database-engineer` | RF04.4, DB-01 |
| D-05 | Exclusão física sem auditoria | Manter exclusão física (sem DDL) + **log Serilog** com usuário/payload/timestamp | RF06.3 |
| D-06 | `p_cd_variedade` (2 chars; `TO*` → nulo) calculado no code-behind | Mover para o **service** (regra de contrato da procedure, testável) | RF12.3 |
| D-07 | Cache em `Session` (`comprasFrutas`, `DetalhamentoNotaFiscal`, chave-lixo `System.Data.DataTable`) | **Stateless**: sem sessão; frontend mantém dados em store Zustand; exportação usa os dados em tela | RF09.6 |
| D-08 | Exportação `.xls` via `ASPxGridViewExporter` (incl. callback que lia sessão errada → arquivo vazio) | **CSV client-side** com os dados em tela (sem nova dependência de backend; KISS) | RF15 |
| D-09 | Defeitos menores: caption de segunda-feira (D-3) sobrescrita por D-1; `tmrJobTempo` inexistente; código morto (`CreateTemplate`, `AbreDetalhamentoColuna`, botões vazios) | Aplicar D-3 na segunda corretamente; auto-refresh via `setInterval`; **não portar código morto** | RF10.2, RF14 |
| D-10 | `DateTime` de procedure alimentado com string `"dd/MM/yyyy"` (conversão implícita NLS) | Contrato ISO 8601 + `DateTime` tipado ponta a ponta | RF09.3 |
| D-11 | `DaoBase.GetDadosProcedure` engolia exceção retornando `null` (inconsistente) | Exceções propagam ao **middleware global** → 500 padronizado + log | §5.2 |
| D-12 | Stub da Fase 8: assinatura errada (`empresa`/`safra`), string mágica de package, model incorreto, endpoint sem autorização | **Reescrita completa** (DB-04, BE-05/BE-06); endpoint antigo descontinuado | §1.2, §5.3 |

---

## 7. Métricas de Aceite (Definition of Done)

**Backend**
- [ ] `dotnet build` sem erros e sem warnings; `dotnet test` com ≥ 80% de cobertura no novo código.
- [ ] Os 8 endpoints respondem conforme §5 (verificar via Swagger).
- [ ] Gate `cadastroSafraMeta`/`comprasFrutas`: usuário sem permissão → 403.
- [ ] `POST /metas` rejeita payload sem campos obrigatórios (400) e `dtFinal < dtInicial` (422).
- [ ] INSERT de meta retorna `idMetaCompra` gerado (D-04 resolvido e registrado).
- [ ] `p_cd_variedade`: "TO005 - X" → nulo; "NI001 - X" → "NI"; nulo (NF direto) → nulo (testes de service).
- [ ] Nenhuma string mágica de package/procedure (constantes `OracleProcedures`).
- [ ] Endpoint antigo do stub removido; contrato novo documentado em `docs/README.md`.

**Frontend**
- [ ] `npm run lint` e `npm run build` sem erros; `npm run test` verde.
- [ ] Grid de metas: paginação 200, ano corrente em negrito, exclusão com `Popconfirm` "Deseja Excluir a Meta?".
- [ ] Painel Compras Frutas: data default hoje; captions `QTDE_D0..D4` corretas (incl. **D-3 na segunda-feira**); `EM_SAFRA` oculta; formatos `N0`/`N1`/`N2`/`N3`; semáforo `#EEE9E9`/`#D3D3D3`/`#77889A`.
- [ ] Drill-down: matriz de cliques RF13 respeitada (DC vs NF); células vazias não clicáveis.
- [ ] Auto-refresh 30min/1h funcionando e limpo no unmount.
- [ ] CSV dos 3 níveis com nomes/conteúdo corretos.
- [ ] `ROUTE_MAP` com `cadastroSafraMeta` e `comprasFrutas`; menu data-driven navega para as rotas novas.

**Fidelidade & Governança**
- [ ] Checklist de fidelidade de `agricola_regra.md` revisado item a item (QA-06).
- [ ] Nenhuma DDL executada (Regra de Ouro nº 1).
- [ ] `TASKS.md` e `PROGRESS.md` atualizados.

---

## 8. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|-------------|---------|-----------|
| Oracle inalcançável no ambiente (histórico: `ORA-50000` na Fase 18) | Alta | Alto | DB-01 fica bloqueado até acesso; contratos cobertos por testes com `FakeDbConnection`/Moq; validação com DBA assim que disponível |
| `ID_META_COMPRA` sem trigger/sequence no banco | Média | Alto | DB-01 confirma via `USER_TRIGGERS`/`USER_SEQUENCES`; fallback = proposta de sequence com aprovação do `database-engineer` (única DDL possível, fora do escopo padrão) |
| Colunas dos REF CURSORs divergirem das inferidas | Média | Médio | Contrato dinâmico (colunas + linhas genéricas) absorve variações; regras de formatação são por **padrão de nome**, não por posição |
| Corpo da `pkg_bi_compras` desconhecido (tabelas de origem) | Alta | Baixo | Consumo "as-is" — o contrato de parâmetros documentado em `agricola_regra.md §2.2` é estável; mapeamento interno com DBA é atividade de DB-01, não bloqueante para o consumo |
| Performance da serialização de linhas dinâmicas | Baixa | Baixo | Volume pequeno (paginação 100/200; drills `ShowAllRecords` mas escopo por empresa+linha+UF); medir no QA |
| Parâmetro VarChar nulo em procedure (NF direto) | Média | Médio | Regra explícita `null → DBNull.Value` (RF12.3) + teste de service |

---

## 9. Plano de Execução (Orquestrado)

| Fase | Descrição | Agentes | Tasks |
|------|-----------|---------|-------|
| **Fase 1** | Camada de Dados | `orchestrator` → `database-engineer` | DB-01 a DB-06 |
| **Fase 2** | Camada de API | `orchestrator` → `backend-engineer` | BE-01 a BE-10 |
| **Fase 3** | Camada Frontend | `orchestrator` → `frontend-engineer` + `architect` | FE-01 a FE-12 |
| **Fase 4** | Qualidade & Testes | `orchestrator` → `qa-engineer` | QA-01 a QA-06 |
| **Fase 5** | Review & Entrega | `orchestrator` → `architect` + `devops-engineer` | RE-01 a RE-04 |

### 9.1 Mapeamento de Tarefas e Agentes

#### 🗄️ Fase 1 — Camada de Dados (`database-engineer`)

| ID | Tarefa | RF | Dependências |
|----|--------|-----|--------------|
| DB-01 | Validar dicionário Oracle com DBA: PK/trigger/sequence de `META_COMPRAS` (`USER_CONSTRAINTS`, `USER_TRIGGERS`, `USER_SEQUENCES`) e spec da `pkg_bi_compras` (3 procedures, parâmetros) — registrar em `PROGRESS.md` | D-04 | — |
| DB-02 | Criar `Models/MetaCompra.cs`; remover model `CompraFruta` de `SecundariosModels.cs` | RF02 | — |
| DB-03 | Criar `IMetaCompraRepository` + `MetaCompraRepository` (5 métodos — §4.3; INSERT com `RETURNING` conforme resultado do DB-01) | RF02, RF04, RF05, RF06 | DB-01, DB-02 |
| DB-04 | Reescrever `IAgricolaRepository`/`AgricolaRepository` — 3 métodos fiéis (§4.3) com retorno dinâmico (colunas + linhas) | RF09, RF11, RF12 | DB-01 |
| DB-05 | Atualizar `OracleProcedures.cs` — constantes `SpRecebimentoFrutasDet`/`SpRecebimentoFrutasDetNf`; eliminar string mágica do stub | RF09 | — |
| DB-06 | Aplicar skills Oracle: bind `:param`, `await using`, conexão aberta antes de REF CURSOR, `DBNull` para VarChar vazio, sem `SELECT *` | — | DB-03, DB-04 |

#### ⚙️ Fase 2 — Camada de API (`backend-engineer`)

| ID | Tarefa | RF | Dependências |
|----|--------|-----|--------------|
| BE-01 | Criar DTOs: `MetaCompraRequest` (DataAnnotations), `MetaCompraResponse`, `EmpresaMetaResponse`, `GridDinamicaResponse` | RF02–RF05, RF09 | — |
| BE-02 | Criar `IMetaCompraService`/`MetaCompraService` — gate `cadastroSafraMeta`, validações (RF04.3), domínio fixo de empresas, log de exclusão | RF01–RF06 | DB-03 |
| BE-03 | Reescrever `IAgricolaService`/`AgricolaService` — gate `comprasFrutasPorEmpresas`, montagem do contrato dinâmico (`ultimaAtualizacao`), regra `p_cd_variedade` (RF12.3), validação de `colunaClicada` | RF08–RF12 | DB-04 |
| BE-04 | Criar `AgricolaEndpoints.cs` — grupo `/api/v1/agricola`, 5 endpoints de metas | RF02–RF06 | BE-02 |
| BE-05 | Adicionar 3 endpoints de compras-frutas (painel + 2 drills) | RF09, RF11, RF12 | BE-03 |
| BE-06 | Remover grupo "Agrícola" legado de `SecundariosEndpoints.cs` (stub) e registros órfãos; documentar quebra de contrato (§5.3) | D-12 | BE-05 |
| BE-07 | Registrar DI em `Program.cs`: `IMetaCompraRepository`, `IMetaCompraService` | — | BE-04 |
| BE-08 | `.RequireAuthorization()` + `.WithTags("Agrícola")` + `.WithSummary()/.WithDescription()/.Produces<>()` em todos os endpoints | — | BE-04, BE-05 |
| BE-09 | Padronizar `IResult` (§5.2): 200/201/204/400/403/404/422; nunca engolir exceções | D-11 | BE-04, BE-05 |
| BE-10 | `dotnet build` 0 erros/0 warnings; endpoints visíveis no Swagger | — | BE-01 a BE-09 |

#### 🎨 Fase 3 — Camada Frontend (`frontend-engineer` + `architect`)

| ID | Tarefa | RF | Dependências |
|----|--------|-----|--------------|
| FE-01 | Criar estrutura `modules/agricola/` (components/services/hooks/utils, `types.ts`) — `architect` | — | — |
| FE-02 | Definir tipos TS: `MetaCompra`, `MetaCompraRequest`, `EmpresaMeta`, `GridDinamica`, `ColunaClicada`, params dos drills — `architect` | RF09 | FE-01 |
| FE-03 | `services/safraMetaService.ts` (5 chamadas) + `services/comprasFrutasService.ts` (3 chamadas, query ISO) | RF02–RF06, RF09, RF11, RF12 | FE-02 |
| FE-04 | Hooks Zustand `useSafraMeta` (CRUD) e `useComprasFrutas` (painel + drills + auto-refresh `setInterval`) | RF02, RF09, RF14 | FE-03 |
| FE-05 | `SafraMetaPage.tsx` — tabela (200/pág, negrito ano corrente, ações editar/excluir com `Popconfirm`) | RF02, RF06, RF07 | FE-04 |
| FE-06 | `components/SafraMetaForm.tsx` — modal criar/editar com validações espelhadas (obrigatórios + datas) | RF04, RF05 | FE-04 |
| FE-07 | `components/DynamicGrid.tsx` — renderiza colunas/linhas dinâmicas, caption/visibilidade/larguras, células clicáveis | RF10 | FE-02 |
| FE-08 | `utils/comprasFrutasFormat.ts` — captions (D0..D-4, **D-3 na segunda**), formatos N0/N1/N2/N3/datas, semáforo de linhas | RF10, RF11, RF12 | FE-02 |
| FE-09 | `ComprasFrutasPage.tsx` — filtro data (default hoje), botão recarregar, label última atualização, select de auto-refresh, grid 100/pág sem sort | RF09, RF10, RF14 | FE-04, FE-07, FE-08 |
| FE-10 | `DetalhamentoModal.tsx` + `NotaFiscalModal.tsx` — drill-downs com matriz de cliques RF13 (DC × NF) | RF11, RF12, RF13 | FE-07, FE-08 |
| FE-11 | `utils/exportarCsv.ts` + botões "Excel" nos 3 níveis (nomes RF15.2, valores formatados) | RF15 | FE-08 |
| FE-12 | `ROUTE_MAP` (`cadastroSafraMeta`, `comprasFrutas`) + rotas lazy em `App.tsx` (`/agricola/safra-meta`, `/agricola/compras-frutas` com `MenuGuard`); `npm run lint` + `npm run build` sem erros | RF07, RF16 | FE-05, FE-09 |

#### 🧪 Fase 4 — Qualidade & Testes (`qa-engineer`)

| ID | Tarefa | RF | Dependências |
|----|--------|-----|--------------|
| QA-01 | `MetaCompraServiceTests` — gate 403, obrigatórios (400), `dtFinal < dtInicial` (422), empresa fora do domínio (400), CRUD ok, 404, log de exclusão | RF01–RF06 | BE-10 |
| QA-02 | `AgricolaServiceTests` — gate `comprasFrutasPorEmpresas`; regra `p_cd_variedade` (`TO*`→nulo; 2 chars; nulo→DBNull); validação `colunaClicada`; `ultimaAtualizacao` preenchido | RF08–RF12 | BE-10 |
| QA-03 | `MetaCompraRepositoryTests` — contrato SQL via `FakeDbConnection` (SELECT ORDER BY, INSERT RETURNING, UPDATE por PK, DELETE por PK) | RF02–RF06 | DB-03 |
| QA-04 | Vitest: `comprasFrutasFormat` (captions D0..D-4, segunda-feira D-3, ocultação `EM_SAFRA`, formatos, semáforo) + `exportarCsv` (nomes, separador `;`, BOM) + matriz de cliques (RF13) | RF10–RF15 | FE-12 |
| QA-05 | `dotnet test` + `npm run test` verdes; cobertura ≥ 80% no novo código | — | QA-01 a QA-04 |
| QA-06 | Checklist de fidelidade `agricola_regra.md` (Regras 1–19) + registro no `PROGRESS.md` | — | QA-05 |

#### 🚀 Fase 5 — Review & Entrega (`architect` + `devops-engineer`)

| ID | Tarefa | Dependências |
|----|--------|--------------|
| RE-01 | Code review completo (8 contratos `CLAUDE.md`, DI, async/await, DTOs vs Models, sem strings mágicas) | QA-06 |
| RE-02 | Documentação: `docs/README.md` (8 endpoints), Swagger, nota de quebra de contrato do stub (§5.3) | RE-01 |
| RE-03 | Build de produção: `dotnet publish` + `npm run build` | RE-02 |
| RE-04 | Validação final: métricas de aceite §7 + decisões D-01 a D-12 | RE-03 |

### 9.2 Sequenciamento

```
FASE 1 ──── database-engineer ────► FASE 2 ──── backend-engineer ────► FASE 3 ──── architect + frontend-engineer
                                                                                          │
                                                                                          ▼
                                                              FASE 5 ◄──── qa-engineer ──── FASE 4
                                                          (architect + devops)
```

**Próximo handoff imediato (Orchestrator → database-engineer):** delegar **DB-01** e **DB-02** em paralelo (DB-01 pode ficar bloqueado por indisponibilidade do Oracle — seguir com contratos cobertos por testes, como na Fase 18).

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

- `docs/agricola_regra.md` — Especificação técnica completa dos painéis legados, incluindo:
  - SQL exato do CRUD de `META_COMPRAS` (Select/Insert/Update/Delete) e domínio fixo de empresas
  - Contrato das 3 procedures `pkg_bi_compras` (parâmetros IN/OUT)
  - 6 regras do painel Safra/Meta + 19 regras do painel Compras Frutas
  - Defeitos latentes documentados (4 + 8 itens) — tratados nas decisões D-01 a D-12
  - Itens presumidos/inferidos a validar no dicionário Oracle (DB-01)

### Anexo B — Stack Legada × Nova

| Item | Legado | Novo |
|------|--------|------|
| Framework | ASP.NET WebForms (.NET 4.8) | ASP.NET Core Minimal API (.NET 9) |
| UI | DevExpress Web v16.2.8 (`ASPxGridView`, `ASPxPopupControl`, `ASPxCallbackPanel`, `ASPxTimer`) | React 18 + Ant Design 5 (`Table`, `Modal`, `DatePicker`, `Select`) + `setInterval` |
| Acesso a dados | `asp:SqlDataSource` (SQL ad-hoc) + `System.Data.OracleClient` + `OracleCommand` | Dapper + `Oracle.ManagedDataAccess.Core` via `DbSession` scoped |
| Estado | `Session` (cache de grids) | Stateless (JWT + store Zustand client-side) |
| Permissão | `DaoAcesso.GetPaginaByChave` + redirect `SemPermissao.aspx` | Gate no service (`AcessoRepository.GetPaginaByChaveAsync`) → 403 + `MenuGuard` na SPA |
| Edição da grid | Inline (`SettingsEditing Mode="Inline"`) | Modal `Form` Ant Design (validações client + server) |
| Exportação | `ASPxGridViewExporter` (.xls) | CSV client-side (BOM UTF-8, separador `;`) |
| Auto-refresh | `ASPxTimer` + `ASPxCallback` | `setInterval` no hook |
| Roteamento | `~/interna/agricola/*.aspx` em abas | `/agricola/safra-meta`, `/agricola/compras-frutas` (React Router + `ROUTE_MAP`) |

### Anexo C — Matriz de Rastreabilidade (Regras do legado → RFs)

| Regra legado (`agricola_regra.md`) | Onde está no PRD |
|---|---|
| Painel 1 — Regra 1 (obrigatoriedade) | RF04.3 + validação client (RF04.7) |
| Painel 1 — Regra 2 (sem validação de conflito) | RF04.3 + D-03 (coerência de datas adicionada; sobreposição fora de escopo) |
| Painel 1 — Regra 3 (geração do ID) | RF04.4 + D-04 + DB-01 |
| Painel 1 — Regra 4 (negrito ano corrente) | RF02.6 |
| Painel 1 — Regra 5 (empresas fixas) | RF03 + D-01/D-02 |
| Painel 1 — Regra 6 (permissão por perfil) | RF01 |
| Painel 2 — Regras 1–2 (filtro único + procedure) | RF09 |
| Painel 2 — Regras 3–5 (grid dinâmica, captions, formatos) | RF09.5, RF10 |
| Painel 2 — Regra 6 (semáforo) | RF10.5 |
| Painel 2 — Regras 7–8 (drill-down + roteamento) | RF10.6, RF11, RF12, RF13 |
| Painel 2 — Regra 9 (sessão/paginação/sem sort) | RF09.6 (stateless), RF09.9 |
| Painel 2 — Regra 10 (auto-refresh) | RF14 |
| Painel 2 — Regra 11 (Excel nível 0) | RF15 |
| Painel 2 — Regras 12–16 (nível 1) | RF11, RF15 |
| Painel 2 — Regras 17–19 (nível 2) | RF12, RF15 |
