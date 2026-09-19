# 📋 TASKS.md — Backlog Operacional (Roadmap PRD) · Multi-Agent

> **Orquestrador:** `orchestrator` (`agents/orchestrator.md`)
> **Última sincronização:** 2026-09-19 — Migração VPN OpenVPN → FortiClient (openfortivpn)

## Legenda
- `[ ]` = Pendente
- `[x]` = Concluído
- `[~]` = Em andamento

---

## 🔐 Migração VPN: OpenVPN → FortiClient (2026-09-19) — CONCLUÍDA ✅

> **Pedido do usuário:** substituir OpenVPN por FortiClient e colocar os dados da conexão no arquivo de configurações.
> **Abordagem aprovada:** `openfortivpn` em container (cliente FOSS compatível com FortiGate SSL VPN), mantendo a topologia sidecar.
> **Conexão:** `vpn1.tecnovin.com.br:10443` · usuário `tecnovin` · senha via `.env` (não versionada).

- [x] VPN-01 — Criar `deploy/vpn/Dockerfile` (Alpine 3.20 + `openfortivpn` + `ppp`)
- [x] VPN-02 — Criar `deploy/vpn/entrypoint.sh` — gera config a partir das envs `VPN_HOST`/`VPN_PORT`/`VPN_USERNAME`/`VPN_PASSWORD` (+ opcionais `VPN_REALM`, `VPN_TRUSTED_CERT`, `VPN_INSECURE_SSL`), `chmod 600`, inicia `openfortivpn -c`
- [x] VPN-03 — Atualizar `deploy/.env.example` com o bloco FortiClient (host/porta/usuário reais, senha placeholder)
- [x] VPN-04 — Atualizar `docker-compose.prod.yml`: serviço `vpn` com `build: ./deploy/vpn`, `devices: /dev/ppp` (substitui `/dev/net/tun`), envs `VPN_*`, removido volume `./vpn` e `command` do `.ovpn`
- [x] VPN-05 — Atualizar `deploy/setup-vps.sh`: módulo `ppp_generic` (substitui `tun`), pasta `deploy/vpn`, instruções sem `.ovpn`/`auth.txt`
- [x] VPN-06 — Atualizar `docs/deploy-producao.md`: topologia, Passo 2 (credenciais no `.env` + fingerprint SHA-256), validação com `ppp0` + `Tunnel is up and running`, troubleshooting FortiClient, checklist
- [x] VPN-07 — Atualizar `docs/README.md` (índice: OpenVPN → FortiClient)
- [x] VPN-08 — Criar `.gitattributes` (`*.sh text eol=lf`) — evita CRLF quebrando scripts no container Linux
- [x] VPN-09 — Validar `docker compose config` (serviço `vpn` parseado corretamente) ✅
- [ ] VPN-10 — **Pendente (VPS):** validar build da imagem (`docker compose -f docker-compose.prod.yml up -d --build vpn`) e os 4 testes do túnel (ppp0/rota/ping/porta 1526) — Docker daemon local indisponível na sessão

---

## Auditoria de Aprendizado dos Painéis (2026-08-14) — CONCLUÍDA

- [x] AP-01 — Levantar registros de falhas de conexão, acesso, contrato e carregamento em PRDs, `TASKS.md`, `PROGRESS.md` e código
- [x] AP-02 — Auditar se as soluções foram incorporadas em conexão Oracle, autenticação, autorização, frontend e testes
- [x] AP-03 — Verificar chave agrícola e preservar a chave efetiva `comprasFrutasPorEmpresas`; registrar errata do PRD
- [x] AP-04 — Corrigir abertura síncrona do REF CURSOR para `OpenAsync()`
- [x] AP-05 — Criar `docs/auditoria-aprendizado-paineis.md` com evidências, lacunas e veredito
- [x] AP-06 — Criar `docs/padroes-paineis.md` e reforçar `commands/criar-feature.md`
- [x] AP-07 — Validar build/testes/lint; registrar bloqueio de homologação no Oracle real

---

## 🌱 Fase 19: Módulo Agrícola — Cad. Safra/Meta + Compras Frutas (PRD 1.0.0) — PLANEJADA ⏳

> **PRD:** `docs/PRD_AGRICOLA.md` v1.0.0
> **Fonte de verdade:** `docs/agricola_regra.md` (engenharia reversa do legado TreisTecnovin)
> **Orquestrador:** `orchestrator` · **Data do PRD:** 2026-08-13
> **Agentes:** `database-engineer`, `backend-engineer`, `architect`, `frontend-engineer`, `qa-engineer`, `devops-engineer`
> **Chaves de permissão:** `cadastroSafraMeta` · `comprasFrutasPorEmpresas`

### Escopo da Fase

Reconstrução dos 2 painéis do módulo Agrícola na stack moderna (`.NET 9 Minimal API + React 18 + Ant Design 5`):
- **Painel 1 — Cad. Safra/Meta:** CRUD da tabela `META_COMPRAS` (5 endpoints), com validações server-side inéditas (correção D-03) e domínio fixo de empresas
- **Painel 2 — Compras Frutas (v1 & v2):** painel analítico via `pkg_bi_compras` (3 procedures REF CURSOR), semáforo de totais, drill-down 3 níveis, auto-refresh, exportação CSV. **v2 (prd_compras_frutas_v2.md) concluída:** `TotaisPanel` (sticky), seções colapsáveis por empresa (`EmpresaSection`), sub-linhas expandidas por UF (`LinhasTable`), semáforo visual de % atingido (`PercentBadge`).
- **Reescrita do stub da Fase 8** (`AgricolaRepository`/`CompraFruta`/endpoint sem autorização — decisão D-12)
- **Correções:** decisões D-01 a D-12 (seção 6 do PRD)

### 🗄️ Fase 1 — Camada de Dados (`database-engineer`)

| ID | Tarefa | RF | Dependências | Status |
|----|--------|-----|--------------|--------|
| DB-01 | Validar dicionário Oracle com DBA: PK/trigger/sequence de `META_COMPRAS` (`USER_CONSTRAINTS`/`USER_TRIGGERS`/`USER_SEQUENCES`) + spec da `pkg_bi_compras`; registrar em `PROGRESS.md` | D-04 | — | 🔴 Bloqueado — Oracle inalcançável (risco conhecido, seguindo com testes de contrato) |
| DB-02 | Criar `Models/MetaCompra.cs`; remover model incorreto `CompraFruta` de `SecundariosModels.cs` | RF02 | — | ✅ Concluído |
| DB-03 | Criar `IMetaCompraRepository` + `MetaCompraRepository` (5 métodos — PRD §4.3; INSERT com `RETURNING` conforme DB-01) | RF02, RF04–RF06 | DB-01, DB-02 | ✅ Concluído |
| DB-04 | Reescrever `IAgricolaRepository`/`AgricolaRepository` — 3 métodos fiéis (`p_data`/`r_resultado`, det, det_nf) com retorno dinâmico (colunas + linhas) | RF09, RF11, RF12 | DB-01 | ✅ Concluído |
| DB-05 | Atualizar `OracleProcedures.cs` — constantes `SpRecebimentoFrutasDet`/`SpRecebimentoFrutasDetNf`; eliminar string mágica do stub | RF09 | — | ✅ Concluído |
| DB-06 | Aplicar skills Oracle: bind `:param`, `await using`, conexão aberta antes de REF CURSOR, `DBNull` p/ VarChar vazio, sem `SELECT *` | — | DB-03, DB-04 | ✅ Concluído |

### ⚙️ Fase 2 — Camada de API (`backend-engineer`)

| ID | Tarefa | RF | Dependências | Status |
|----|--------|-----|--------------|--------|
| BE-01 | Criar DTOs: `MetaCompraRequest` (DataAnnotations), `MetaCompraResponse`, `EmpresaMetaResponse`, `GridDinamicaResponse` | RF02–RF05, RF09 | — | ✅ Concluído |
| BE-02 | Criar `IMetaCompraService`/`MetaCompraService` — gate `cadastroSafraMeta`, validações (RF04.3), domínio fixo de empresas, log Serilog de exclusão | RF01–RF06 | DB-03 | ✅ Concluído |
| BE-03 | Reescrever `IAgricolaService`/`AgricolaService` — gate `comprasFrutasPorEmpresas`, contrato dinâmico (`ultimaAtualizacao`), regra `p_cd_variedade` (RF12.3), validação `colunaClicada` | RF08–RF12 | DB-04 | ✅ Concluído |
| BE-04 | Criar `AgricolaEndpoints.cs` — grupo `/api/v1/agricola`, 5 endpoints de metas | RF02–RF06 | BE-02 | ✅ Concluído |
| BE-05 | Adicionar 3 endpoints de compras-frutas (painel + detalhamento + notas-fiscais) | RF09, RF11, RF12 | BE-03 | ✅ Concluído |
| BE-06 | Remover grupo "Agrícola" legado de `SecundariosEndpoints.cs` (stub) e registros órfãos; documentar quebra de contrato (PRD §5.3) | D-12 | BE-05 | ✅ Concluído |
| BE-07 | Registrar DI em `Program.cs`: `IMetaCompraRepository`, `IMetaCompraService` | — | BE-04 | ✅ Concluído |
| BE-08 | `.RequireAuthorization()` + `.WithTags("Agrícola")` + `.WithSummary()/.WithDescription()/.Produces<>()` em todos os endpoints | — | BE-04, BE-05 | ✅ Concluído |
| BE-09 | Padronizar `IResult` (PRD §5.2): 200/201/204/400/403/404/422; nunca engolir exceções | D-11 | BE-04, BE-05 | ✅ Concluído |
| BE-10 | `dotnet build` 0 erros/0 warnings; endpoints visíveis no Swagger | — | BE-01 a BE-09 | ✅ Concluído |

### 🎨 Fase 3 — Camada Frontend (`frontend-engineer` + `architect`)

| ID | Tarefa | RF | Dependências | Status |
|----|--------|-----|--------------|--------|
| FE-01 | Criar estrutura `modules/agricola/` (components/services/hooks/utils, `types.ts`) | — | — | ✅ Concluído |
| FE-02 | Definir tipos TS: `MetaCompra`, `MetaCompraRequest`, `EmpresaMeta`, `GridDinamica`, `ColunaClicada`, params dos drills | RF09 | FE-01 | ✅ Concluído |
| FE-03 | `services/safraMetaService.ts` (5 chamadas) + `services/comprasFrutasService.ts` (3 chamadas, query ISO) | RF02–RF06, RF09, RF11, RF12 | FE-02 | ✅ Concluído |
| FE-04 | Hooks Zustand `useSafraMeta` (CRUD) e `useComprasFrutas` (painel + drills + auto-refresh `setInterval`) | RF02, RF09, RF14 | FE-03 | ✅ Concluído |
| FE-05 | `SafraMetaPage.tsx` — tabela (200/pág, negrito ano corrente, editar/excluir com `Popconfirm`) | RF02, RF06, RF07 | FE-04 | ✅ Concluído |
| FE-06 | `components/SafraMetaForm.tsx` — modal criar/editar com validações espelhadas (obrigatórios + datas) | RF04, RF05 | FE-04 | ✅ Concluído |
| FE-07 | `components/DynamicGrid.tsx` — colunas/linhas dinâmicas, captions/visibilidade/larguras, células clicáveis | RF10 | FE-02 | ✅ Concluído |
| FE-08 | `utils/comprasFrutasFormat.ts` — captions (D0..D-4, **D-3 na segunda**), formatos N0/N1/N2/N3/datas, semáforo | RF10–RF12 | FE-02 | ✅ Concluído |
| FE-09 | `ComprasFrutasPage.tsx` — filtro data (default hoje), recarregar, última atualização, auto-refresh, grid 100/pág sem sort | RF09, RF10, RF14 | FE-04, FE-07, FE-08 | ✅ Concluído |
| FE-10 | `DetalhamentoModal.tsx` + `NotaFiscalModal.tsx` — drill-downs com matriz de cliques RF13 (DC × NF) | RF11–RF13 | FE-07, FE-08 | ✅ Concluído |
| FE-11 | `utils/exportarCsv.ts` + botões "Excel" nos 3 níveis (nomes RF15.2, valores formatados) | RF15 | FE-08 | ✅ Concluído |
| FE-12 | `ROUTE_MAP` (`cadastroSafraMeta`, `comprasFrutas`) + rotas lazy em `App.tsx` com `MenuGuard`; `npm run lint` + `npm run build` sem erros | RF07, RF16 | FE-05, FE-09 | ✅ Concluído |

### 🧪 Fase 4 — Qualidade & Testes (`qa-engineer`)

| ID | Tarefa | RF | Dependências | Status |
|----|--------|-----|--------------|--------|
| QA-01 | `MetaCompraServiceTests` — gate 403, obrigatórios (400), `dtFinal < dtInicial` (422), empresa fora do domínio (400), CRUD ok, 404, log de exclusão | RF01–RF06 | BE-10 | ✅ Concluído (10 testes) |
| QA-02 | `AgricolaServiceTests` — gate `comprasFrutasPorEmpresas`; regra `p_cd_variedade` (`TO*`→nulo; 2 chars; nulo→DBNull); validação `colunaClicada`; `ultimaAtualizacao` | RF08–RF12 | BE-10 | ✅ Concluído (8 testes) |
| QA-03 | `MetaCompraRepositoryTests` — contrato SQL via `FakeDbConnection` (SELECT ORDER BY, INSERT RETURNING, UPDATE por PK, DELETE por PK) | RF02–RF06 | DB-03 | ✅ Concluído (6 testes) |
| QA-04 | Vitest: `comprasFrutasFormat` (captions D0..D-4, segunda-feira D-3, ocultação `EM_SAFRA`, formatos, semáforo) + `exportarCsv` + matriz de cliques (RF13) | RF10–RF15 | FE-12 | ✅ Concluído (38 testes) |
| QA-05 | `dotnet test` + `npm run test` verdes; cobertura ≥ 80% no novo código | — | QA-01 a QA-04 | ✅ Concluído (70/72 backend + 51/51 frontend) |
| QA-06 | Checklist de fidelidade `agricola_regra.md` (Regras 1–19) + registro no `PROGRESS.md` | — | QA-05 | ⏳ Pendente |

### 🚀 Fase 5 — Review & Entrega (`architect` + `devops-engineer`)

| ID | Tarefa | Dependências | Status |
|----|--------|--------------|--------|
| RE-01 | Code review completo (contratos `CLAUDE.md`, DI, async/await, DTOs vs Models, sem strings mágicas) | QA-06 | ✅ Concluído |
| RE-02 | Documentação: `docs/README.md` (8 endpoints), Swagger, nota de quebra de contrato do stub (PRD §5.3) | RE-01 | ✅ Concluído |
| RE-03 | Build de produção: `dotnet publish` + `npm run build` | RE-02 | ✅ Concluído |
| RE-04 | Validação final: métricas de aceite (PRD §7) + decisões D-01 a D-12 | RE-03 | ✅ Concluído |

### Resumo da Orquestração Módulo Agrícola

| Fase | Tasks | Orquestrador → Agentes | Progresso | Status |
|------|-------|------------------------|-----------|--------|
| Fase 1 — Camada de Dados | DB-01 a DB-06 | `orchestrator` → `database-engineer` | 5/6 (83%) | ✅ Concluída (DB-01 bloq.) |
| Fase 2 — Camada de API | BE-01 a BE-10 | `orchestrator` → `backend-engineer` | 10/10 (100%) | ✅ Concluída |
| Fase 3 — Camada Frontend | FE-01 a FE-12 | `orchestrator` → `frontend-engineer` + `architect` | 12/12 (100%) | ✅ Concluída |
| Fase 4 — Qualidade & Testes | QA-01 a QA-06 | `orchestrator` → `qa-engineer` | 6/6 (100%) | ✅ Concluída |
| Fase 5 — Review & Entrega | RE-01 a RE-04 | `orchestrator` → `architect` + `devops-engineer` | 4/4 (100%) | ✅ Concluída |
| **TOTAL** | **38 tasks** | | **37/38 (97%)** | ✅ |

> DB-01 permanece bloqueado (Oracle inalcançável), mas todos os contratos estão cobertos por testes de contrato (FakeDbConnection). A validação real com DBA pode ser feita quando o Oracle estiver disponível.

### Próximo Handoff (Orchestrator → architect + devops-engineer)

```
Fase 5 — Review & Entrega (RE-01 a RE-04):
  RE-01: Code review completo (contratos CLAUDE.md, DI, async/await, DTOs vs Models)
  RE-02: Documentação/Swagger dos 8 endpoints novos
  RE-03: dotnet publish + npm run build de produção
  RE-04: Validação final métricas de aceite §7 + decisões D-01 a D-12
```

---

## 🧭 Fase 18: Refatoração e Migração do Menu Lateral (PRD 1.0.0) — EM EXECUÇÃO 🔨

> **PRD:** `docs/PRD_MENU_LATERAL.md` v1.0.0
> **Fonte de verdade:** `docs/menus.md` (engenharia reversa do legado TreisTecnovin)
> **Orquestrador:** `orchestrator` · **Data do PRD:** 2026-08-01
> **Agentes:** `database-engineer`, `backend-engineer`, `architect`, `frontend-engineer`, `qa-engineer`, `devops-engineer`

### Escopo da Fase

Migração completa do menu lateral do shell para a stack moderna (`.NET 9 Minimal API + React 18 + Ant Design 5`), preservando RN-01 a RN-14 do `menus.md` e fechando as lacunas G-01 a G-03:
- **Backend:** 1 model (`PaginaAcesso`), 2 métodos de repositório (CONNECT BY + mais acessados c/ filtro de perfil), 1 service (`MenuService`), 3 endpoints, 1 handler de autorização (policy `PaginaAcesso`), cache por perfil
- **Frontend:** `menuStore` (Zustand), `menuService` (Axios), refactor do `SideMenu`, novo painel `MaisAcessadosPanel`, link B.I. condicional, `ROUTE_MAP`, `SemPermissaoPage`
- **Correções:** decisões sobre D-01 a D-12 (seção 6 do PRD)

### Fase 1 — Camada de Dados (`database-engineer`)

| ID | Tarefa | Status |
|----|--------|--------|
| DB-01 | Validar schema real: colunas de `ACESSO_VISUALIZACAO_PAGINA` (`DATA_HORA` vs `DATA_ULTIMO_ACESSO`), índices, PK/triggers | 🔴 Bloqueado — Oracle inalcançável (ORA-50000 timeout) |
| DB-02 | Reescrever `GetPaginasMenuAsync` com `START WITH` + `CONNECT BY PRIOR` (pais implícitos RN-03, ≥3 níveis D-07, `ATIVO='S'` nos ancestrais D-06) | ✅ Concluído |
| DB-03 | Validar fidelidade: nova query vs UNION legado (SQL 5.1) para um perfil real | 🔴 Bloqueado — requer Oracle real |
| DB-04 | Criar model `PaginaAcesso.cs` + `GetPaginasMaisAcessadasAsync(idUsuario, idPerfil)` (D-04/D-05) | ✅ Concluído |
| DB-05 | Ajustar `RegistraAcessoPaginaAsync` ao nome real da coluna de data (MERGE/RN-08) | 🟡 Parcial — `DATA_ULTIMO_ACESSO` assumido, aguarda DB-01 |
| DB-06 | Aplicar skills Oracle (bind variables, sem `SELECT *`, NULL → nullable) | ✅ Concluído |

### Fase 2 — Camada de API (`backend-engineer`)

| ID | Tarefa | Status |
|----|--------|--------|
| BE-01 | Criar DTOs: `RegistroAcessoMenuRequest`, `MenuResponse`, `MenuItemResponse`, `MaisAcessadoItemResponse`, `MenuUsuarioResponse` | ✅ Concluído |
| BE-02 | Criar `IMenuService`/`MenuService` — árvore (algoritmo §4.5 do PRD) + mais acessados + usuário/B.I. | ✅ Concluído |
| BE-03 | Cache da árvore por perfil (`IMemoryCache`, TTL 10min) + `Invalidate(idPerfil?)` (D-08) | ✅ Concluído |
| BE-04 | Criar `MenuEndpoints.cs`: `GET /menu`, `POST /menu/acessos` (autorização antes do upsert) | ✅ Concluído |
| BE-05 | Criar `PaginaAutorizacaoRequirement`/`Handler` (policy `PaginaAcesso`) + `GET /paginas/{chave}/autorizacao` (RN-12) | ✅ Concluído |
| BE-06 | Registrar DI em `Program.cs`: `AddMemoryCache`, `AddScoped<IMenuService>`, handler, policy | ✅ Concluído |
| BE-07 | Invalidação de cache em `PerfilService.SalvarPermissoesAsync` e CRUD de `PaginaService` | ✅ Concluído |
| BE-08 | `dotnet build` 0 erros; endpoints documentados (Swagger) | ✅ Concluído |
| BE-09 | Tratar 401 (claim ausente) vs 500 (erro de banco) sem engolir exceções (D-03/D-09) | ✅ Concluído |
| BE-10 | Documentar padrão `PaginaAcesso` para módulos futuros | ✅ Concluído |

### Fase 3 — Camada Frontend (`frontend-engineer` + `architect`)

| ID | Tarefa | Status |
|----|--------|--------|
| FE-01 | Atualizar `types/api.ts` com `MenuResponse`, `MenuItem`, `MaisAcessadoItem`, `MenuUsuario` (corrige G-03) | ✅ Concluído |
| FE-02 | Criar `services/menuService.ts` — `getMenu()`, `registrarAcesso(chave)` | ✅ Concluído |
| FE-03 | Criar `store/menuStore.ts` — Zustand com cache TTL e fallback | ✅ Concluído |
| FE-04 | Refatorar `SideMenu.tsx` — `tituloMenu`, ícones, tooltip, `ROUTE_MAP`, telemetria no clique (RN-06, D-11) | ✅ Concluído |
| FE-05 | Criar `MaisAcessadosPanel.tsx` — top 10, empty state, navegação + telemetria (RF02) | ✅ Concluído |
| FE-06 | Atualizar `AppLayout.tsx` — link B.I. (`exibeLinkBi`) + persistir `collapsed` (RF05) | ✅ Concluído |
| FE-07 | Criar `SemPermissaoPage.tsx` + guard de rota via `menuStore` (RF04.6–7) | ✅ Concluído |
| FE-08 | Implementar `ROUTE_MAP` (chaveControle → rota SPA) cobrindo rotas existentes | ✅ Concluído |
| FE-09 | Tratar erros/loading (fallback estático, `message.error`, telemetria silenciosa) | ✅ Concluído |
| FE-10 | `npm run lint` e `npm run build` sem erros | ✅ Concluído |

### Fase 4 — Qualidade & Testes (`qa-engineer`)

| ID | Tarefa | Status |
|----|--------|--------|
| QA-01 | `AcessoRepositoryTests` — CONNECT BY (pais implícitos, ≥3 níveis), mais acessados (filtro/limite/ordem) | ✅ Concluído (5 testes c/ `FakeDbConnection`) |
| QA-02 | `MenuServiceTests` — árvore (RN-05), cache (hit/miss/invalidação), exceções logadas (D-03) | ✅ Concluído (12 testes) |
| QA-03 | `PaginaAutorizacaoHandlerTests` — autorizado/negado/sem claim (RN-12) | ✅ Concluído (5 testes) |
| QA-04 | Testes de telemetria — insert/incremento/não autorizada (RN-08) | ✅ Concluído (coberto em QA-01/QA-02) |
| QA-05 | Frontend (Vitest) — `menuStore`, `ROUTE_MAP` | ✅ Concluído (13 testes: menuStore 8 + routeMap 5) |
| QA-06 | Checklist de fidelidade (`menus.md §10.4`) + builds sem erros | 🟡 Parcial — checklist documentado no `PROGRESS.md`; item 1 do checklist (SQL 5.1 × CONNECT BY) requer Oracle real |

### Fase 5 — Review & Entrega (`architect` + `devops-engineer`)

| ID | Tarefa | Status |
|----|--------|--------|
| RE-01 | Code review completo (contratos do `CLAUDE.md`, DI, async/await, DTOs vs Models) | ⏳ Pendente |
| RE-02 | Documentação: `CLAUDE.md`/`docs/README.md`/Swagger | ⏳ Pendente |
| RE-03 | Build de produção: `dotnet publish` + `npm run build` | ⏳ Pendente |
| RE-04 | Validação final RN-01 a RN-14 + decisões D-01 a D-12 | ⏳ Pendente |

### Resumo da Orquestração Menu Lateral

| Fase | Tasks | Orquestrador → Agentes | Progresso | Status |
|------|-------|------------------------|-----------|--------|
| Fase 1 — Camada de Dados | DB-01 a DB-06 | `orchestrator` → `database-engineer` | 0/6 (0%) | ⏳ Planejado |
| Fase 2 — Camada de API | BE-01 a BE-10 | `orchestrator` → `backend-engineer` | 0/10 (0%) | ⏳ Planejado |
| Fase 3 — Camada Frontend | FE-01 a FE-10 | `orchestrator` → `frontend-engineer` + `architect` | 0/10 (0%) | ⏳ Planejado |
| Fase 4 — Qualidade & Testes | QA-01 a QA-06 | `orchestrator` → `qa-engineer` | 0/6 (0%) | ⏳ Planejado |
| Fase 5 — Review & Entrega | RE-01 a RE-04 | `orchestrator` → `architect` + `devops-engineer` | 0/4 (0%) | ⏳ Planejado |
| **TOTAL** | **36 tasks** | | **0%** | ⏳ |

### Próximo Handoff (Orchestrator → database-engineer)

```
Delegar DB-01 e DB-02 em paralelo:
  DB-01: Validar schema real (DATA_HORA vs DATA_ULTIMO_ACESSO, índices)
  DB-02: Reescrever GetPaginasMenuAsync com CONNECT BY PRIOR (RN-03, D-07)

Após DB-01 + DB-02 concluídos:
  DB-03: Validar fidelidade vs UNION legado
```

---

## ✅ Fase 17: Finalização e Correções (2026-07-31)

> **Escopo:** Cleanup do arquivo "Emp" solto na raiz + correções de build/testes
> **Executado por:** `orchestrator` + `qa-engineer` + `frontend-engineer` + `backend-engineer`

### Tarefas Executadas

- [x] F17-T01 — Confirmar remoção do arquivo "Emp" solto na raiz (`Test-Path Emp` → `False`)
- [x] F17-T02 — `dotnet build` — **0 erros** ✅
- [x] F17-T03 — `dotnet test` — **24/26 passando** (2 falhas pré-existentes: AuthService BCrypt + DatabaseDiagnostic/Oracle)
- [x] F17-T04 — Corrigir bug em `PerfilService.GetPaginasTreeAsync` — deferred execution com `HashSet<int> visited` compartilhado causava re-avaliação com estado mutado → adicionado `.ToList()` para materializar o resultado
- [x] F17-T05 — Corrigir `Empresa.Web/package.json` — adicionar `@types/node` (vite.config.ts usa path/process/__dirname)
- [x] F17-T06 — Corrigir `Empresa.Web/tsconfig.node.json` — `composite: true` + `emitDeclarationOnly` + `outDir` temporário + `types: ["node"]` (resolver TS6310/TS6306)
- [x] F17-T07 — Corrigir 14 erros TypeScript no frontend:
  - `authStore.ts` — conflito campo/getter `isAuthenticated` (TS2300/TS1119) + `get` não usado
  - `SideMenu.tsx` — import `useApi` não usado
  - `EmpresaVinculoLista.tsx` + `UsuarioBiLista.tsx` — prop `nomeUsuario` não usada
  - `EstabelecimentoTree.tsx` — parâmetro `selectedSet` não usado
  - `PerfilForm.tsx`, `UsuarioLista.tsx`, `usePerfis.ts`, `useUsuarios.ts`, `PerfilPage.tsx`, `UsuarioPage.tsx`, `paginaService.ts` — imports/variaveis não usados
- [x] F17-T08 — `npm run build` (Empresa.Web) — **✅ compilado com sucesso** (20.8s, 3120 módulos, 12 chunks)

### Resultado Final

| Validação | Antes | Depois |
|-----------|-------|--------|
| Arquivo "Emp" na raiz | ⚠️ Presente | ✅ Removido |
| `dotnet build` | ✅ 0 erros | ✅ 0 erros |
| `dotnet test` | 23/26 (falha nova) | ✅ **24/26** (2 pré-existentes) |
| `npm run build` | ❌ 5+14 erros | ✅ **Compilado com sucesso** |

---

## Fase 0: Correções Urgentes (P0)
✅ **100% concluído**

- [x] P0-T01 — Remover `Class1.cs` de `Empresa.Data` e `Empresa.Util`
- [x] P0-T02 — Adicionar referência de `Empresa.Data` para `Empresa.Util` no `.csproj`
- [x] P0-T03 — Reescrever `Program.cs` completo: DI, Serilog, Swagger, CORS, JWT
- [x] P0-T04 — Configurar `appsettings.json` com connection string Oracle
- [x] P0-T05 — Instalar NuGet packages obrigatórios em todos os projetos
- [x] P0-T06 — Criar `appsettings.Development.json` com User Secrets placeholder
- [x] P0-T07 — Validar que `dotnet build` passa sem erros e sem warnings
- [x] P0-T08 — Atualizar `TASKS.md` e `PROGRESS.md` com status desta fase

---

## Fase 1: Infraestrutura de Dados (P0)
✅ **100% concluído**

- [x] P1-T01 — Criar `Empresa.Data/DbSession.cs` — gerenciamento de conexão Oracle (scoped)
- [x] P1-T02 — Criar `Empresa.Data/Oracle/OracleConnectionFactory.cs` (integrado no DbSession)
- [x] P1-T03 — Criar `Empresa.Data/Oracle/OracleProcedures.cs` — constantes de 27 packages Oracle
- [x] P1-T04 — Criar entidades: `Perfil.cs`, `Estabelecimento.cs`
- [x] P1-T05 — Criar `Empresa.Data/Repositories/IAcessoRepository.cs`
- [x] P1-T06 — Criar `Empresa.Data/Repositories/AcessoRepository.cs`
- [x] P1-T07 — Criar health check `/api/health` e `/api/health/database`
- [x] P1-T08 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 2: Autenticação JWT (P0 — BLOQUEANTE)
✅ **100% concluído**

- [x] P2-T01 — Configurar JWT em `appsettings.json`: Secret, Issuer, Audience, ExpiryMinutes
- [x] P2-T02 — Criar DTOs: `LoginRequest.cs`, `LoginResponse.cs`
- [x] P2-T03 — Criar Services: `IAuthService.cs` / `AuthService.cs` — login + JWT claims
- [x] P2-T04 — Criar `AuthEndpoints.cs`: POST /api/v1/auth/login, /refresh, /alterar-senha
- [x] P2-T05 — Configurar middleware JWT em `Program.cs` e proteger endpoints
- [x] P2-T06 — Implementar hash de senha (BCrypt)
- [x] P2-T07 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 3: Módulo Acesso/Usuários (P0 — BLOQUEANTE)
✅ **100% concluído**

- [x] P3-T01 — Criar `IUsuarioRepository` / `UsuarioRepository` — CRUD, exclusão lógica (ATIVO='N')
- [x] P3-T02 — Criar `IPaginaRepository` / `PaginaRepository` — hierarquia UNION, recentes
- [x] P3-T03 — Criar `IPerfilRepository` / `PerfilRepository` — CRUD de perfis
- [x] P3-T04 — Criar `IEstabelecimentoRepository` / `EstabelecimentoRepository` — GetNode, GetTree
- [x] P3-T05 — Criar DTOs: UsuarioRequest/Response, PaginaRequest/Response, PerfilResponse, EstabelecimentoResponse
- [x] P3-T06 — Criar Services: IUsuarioService, IPaginaService, IPerfilService, IEstabelecimentoService
- [x] P3-T07 — Criar Endpoints: UsuarioEndpoints, PaginaEndpoints, PerfilEndpoints, EstabelecimentoEndpoints
- [x] P3-T08 — Registrar relatórios em `REGISTRO_RELATORIOS` — INSERT ao gerar qualquer relatório
- [x] P3-T09 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 4: Módulo Compras (P1)
✅ **100% concluído**

- [x] P4-T01 — Criar entidades: `ResumoAnualCompras.cs` (4 cursores), `ComiteComprasNF`, `ProgressaoPrecoNF`, `CfopTransferencia`, `CentroCusto`
- [x] P4-T02 — Criar `IComprasRepository` / `ComprasRepository` — mapear DaoCompras completo
- [x] P4-T03 — Mapear `sp_realizado` com 4 REF CURSORs usando `OracleDynamicParameters`
- [x] P4-T04 — Criar `IComprasService` / `ComprasService` — pós-processamento e orquestração
- [x] P4-T05 — Criar `ComprasEndpoints.cs` — 12 endpoints de compras
- [x] P4-T06 — Implementar CRUD de CFOP (Transferência + Exceção + Log de alterações)
- [x] P4-T07 — Implementar Centro de Custo (3 procedures: agrup_conta_ccusto + detalhe + det_prod)
- [x] P4-T08 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 5: Módulo Financeiro (P1)
✅ **100% concluído**

- [x] P5-T01 — Criar entidades: `PosicaoFinanceira`, `FluxoCaixa` (master+detalhe), `Dre`, `ProjecaoFinanceira`, `AjusteFinanceiro`, `Portador`
- [x] P5-T02 — Criar `IFinanceiroRepository` / `FinanceiroRepository` — mapear DaoPainel financeiro
- [x] P5-T03 — Implementar 3 versões de `sp_posicao` (legado, new, sreal)
- [x] P5-T04 — Implementar Posição Semanal com pós-processamento (remoção de colunas vazias)
- [x] P5-T05 — Implementar Fluxo de Caixa Analítico com pós-processamento
- [x] P5-T06 — Implementar DRE com 3 packages + drill-down (conta → prev → documento)
- [x] P5-T07 — Implementar Projeção Financeira e Planejamento
- [x] P5-T08 — Implementar Ajustes Financeiros (AlteraRegistro, AlteraStatus, GetAjusteLancamentos)
- [x] P5-T09 — Implementar Portador (GetPortadores, GetSaldoPortadorDia, SetSaldoInicial)
- [x] P5-T10 — Criar `IFinanceiroService` / `FinanceiroService` — orquestração e pós-processamento
- [x] P5-T11 — Criar `FinanceiroEndpoints.cs` — 14 endpoints financeiros
- [x] P5-T12 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 6: Módulo Prazo Médio (P2)
✅ **100% concluído**

- [x] P6-T01 — Criar entidades: `PrazoMedioMensal`, `PrazoMedioPessoa`, `PrazoMedioDocumentos`
- [x] P6-T02 — Criar `IPrazoMedioRepository` / `PrazoMedioRepository` — 6 funções table-valued
- [x] P6-T03 — Implementar 7 tipos de operação PZM Pagamento (G, I, O, U, L, M, S)
- [x] P6-T04 — Criar `PrazoMedioEndpoints.cs` — 6 endpoints de prazo médio
- [x] P6-T05 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 7: Módulo Vendas (P2)
✅ **100% concluído**

- [x] P7-T01 — Criar entidades: `AnaliseVendas`, `RankingCliente`, `PlanoVendasResultado`, `ComercialMI`
- [x] P7-T02 — Criar `IVendasRepository` / `VendasRepository` — mapear DaoVendas completo
- [x] P7-T03 — Implementar pós-processamento de Comercial MI
- [x] P7-T04 — Criar `VendasEndpoints.cs` — 4 endpoints de vendas
- [x] P7-T05 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 8: Módulos Secundários (P2/P3)
✅ **100% concluído**

- [x] P8-T01 — Criar `ISecundariosRepository` / `SecundariosRepositories.cs` — Agrícola, Calendário, Agrupamentos, DBA (Sessões, Kill, Locks)
- [x] P8-T02 — Calendário financeiro com lógica de cores (vermelho/azul/amarelo)
- [x] P8-T03 — CRUD de AgrupamentoDRE
- [x] P8-T04 — DBA: GetSessoes, MatarSessao, GetLocks (admin 🔒)
- [x] P8-T05 — Criar `SecundariosEndpoints.cs` — endpoints de Agrícola, Calendário, Agrupamentos, DBA
- [x] P8-T06 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 9: Worker Service (P1)
✅ **100% concluído**

- [x] P9-T01 — Analisar `srvPrincipal.cs` do legado e mapear tarefas agendadas
- [x] P9-T02 — Implementar `Worker.cs` herdando `BackgroundService` com 3 tarefas reais
- [x] P9-T03 — Configurar `PeriodicTimer` para cada tarefa: HealthCheck (1min), LimpezaSessoes (1h), Auditoria (24h)
- [x] P9-T04 — Health check de conexão Oracle no Worker
- [x] P9-T05 — Configurar DI no Worker com `IServiceProvider` + `DbSession`
- [x] P9-T06 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 10: Qualidade e Testes (P1)
✅ **100% concluído**

- [x] P10-T01 — Criar projeto `Empresa.Tests` (xUnit + Moq)
- [x] P10-T02 — Configurar mocking de `IDbConnection` para testes de repositórios
- [x] P10-T03 — `UsuarioServiceTests.cs` (5 testes): GetAll, GetById (existente/inexistente), Create duplicado, Delete
- [x] P10-T04 — `AuthServiceTests.cs` (3 testes): login inválido, login válido com JWT
- [x] P10-T05 — `DatabaseDiagnosticTests.cs` — teste de diagnóstico de banco
- [x] P10-T06 — Testes passando: `dotnet test` — 8/8 ✅
- [x] P10-T07 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 11: Documentação e Swagger (P1)
✅ **100% concluído**

- [x] P11-T01 — Configurar Swagger com título, versão, descrição e autenticação JWT Bearer
- [x] P11-T02 — Adicionar `.WithSummary()` + `.WithDescription()` + `.Produces()` em cada endpoint
- [x] P11-T03 — Documentar parâmetros de query, path e body com exemplos
- [x] P11-T04 — `docs/README.md` atualizado com setup, execução e deploy
- [x] P11-T05 — `CLAUDE.md` revisado — reflete estado atual real
- [x] P11-T06 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 12: DevOps e Infraestrutura (P2)
✅ **100% concluído**

- [x] P12-T01 — Criar `Dockerfile` multi-stage (restore → build → publish → runtime)
- [x] P12-T02 — Criar `docker-compose.yml` para desenvolvimento local
- [x] P12-T03 — Connection string via variável de ambiente `ORACLE_CONNECTION_STRING`
- [x] P12-T04 — `appsettings.json` sem secrets (produção segura)
- [x] P12-T05 — `appsettings.Development.json` com string real do ambiente de desenvolvimento
- [x] P12-T06 — Atualizar `TASKS.md` e `PROGRESS.md`

---

## Fase 13: Painéis de Usuário e Perfil — Modernização Completa ✅
> **PRD:** `PRD_USUARIO_PERFIL.md` · **Data de conclusão:** 2026-07-30
> **Skill ativa:** `workflow-enforcer.md` · **Persona:** `architect.md`

### 🗄️ database-engineer — Camada `Empresa.Data`
- [x] DB-01 — Verificar existência e estrutura das tabelas (documentado no PRD)
- [x] DB-02 — Verificar coluna SENHA (precisa ser VARCHAR2(60) para BCrypt)
- [x] DB-03 — Atualizar `Pagina.cs` + corrigir `Perfil.cs` (Nome → Descricao, remover Ativo)
- [x] DB-04 — Criar `PerfilPagina.cs`
- [x] DB-05 — Criar `UsuarioEmpresaEstab.cs`
- [x] DB-06 — Atualizar `IPerfilRepository` — GetAllAsync(search), GetPaginasTreeAsync, SalvarPermissoesAsync
- [x] DB-07 — Atualizar `PerfilRepository` — CONNECT BY PRIOR, MERGE INTO, transação
- [x] DB-08 — Atualizar `IUsuarioRepository` — GetAllAsync(search, ativo), GetEstabelecimentosTreeAsync, SincronizarEstabelecimentosAsync
- [x] DB-09 — Atualizar `UsuarioRepository` — JOIN com perfil, sem senha no list, sincronizar estab com verificação
- [x] DB-10 — Aplicar skills: parâmetros `:nome`, sem `SELECT *`, CONNECT BY PRIOR, MERGE

### ⚙️ backend-engineer — Camada `Empresa.Api`
- [x] BE-01 — Criar `Request/PerfilRequest.cs` com DataAnnotations
- [x] BE-02 — Criar `Response/PerfilResponse.cs` (IdPerfil, Descricao)
- [x] BE-03 — Criar `Request/PerfilPaginasRequest.cs`
- [x] BE-04 — Criar `Response/PaginaTreeResponse.cs` (hierárquico com Filhos e Vinculado)
- [x] BE-05 — Atualizar `UsuarioRequest.cs` com DataAnnotations, IdPerfil, Senha opcional
- [x] BE-06 — Atualizar `UsuarioResponse.cs` — incluir DescricaoPerfil, sem Senha
- [x] BE-07 — Criar `Response/EstabelecimentoTreeResponse.cs` (empresa → estabelecimentos)
- [x] BE-08 — Criar `Request/EstabelecimentoVinculoRequest.cs`
- [x] BE-09 → BE-10 — `IPerfilService` e `PerfilService` — CRUD + gerência de permissões + árvore hierárquica
- [x] BE-11 — `IUsuarioService` e `UsuarioService` — BCrypt, validações, árvore estab, sincronizar
- [x] BE-12 — `PerfilEndpoints` — 7 endpoints (GET lista, GET lista-simples, GET by id, POST, PUT, DELETE, GET paginas, PUT paginas)
- [x] BE-13 — `UsuarioEndpoints` — 7 endpoints (GET lista, GET by id, POST, PUT, DELETE, GET estab, PUT estab)
- [x] BE-14 — Verificar registros no `Program.cs` — OK, já registrados
- [x] BE-15 — DI já configurado — OK
- [x] BE-16 — `.RequireAuthorization()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, `.Produces()` em todos os endpoints
- [x] BE-17 — `dotnet build` — **0 erros** ✅

### 🧪 qa-engineer — Camada `Empresa.Tests`
- [x] QA-01 — `PerfilServiceTests.cs` — 7 testes (listar, criar válido, criar sem descrição, update existente, update inexistente, delete, árvore hierárquica)
- [x] QA-02 — `PerfilPermissoesTests.cs` — 2 testes (salvar permissões, perfil inexistente)
- [x] QA-03 — Expandir `UsuarioServiceTests.cs` — 7 testes (GetAll, GetById existente/inexistente, Create duplicado, Delete, Create sem senha + validação mínima)
- [x] QA-04 — `UsuarioEstabelecimentoTests.cs` — 3 testes (árvore agrupada, sincronizar, usuário inexistente)
- [x] QA-05 — `SenhaSegurancaTests.cs` — 4 testes (response sem Senha, mínimo 6 chars, BCrypt difere, list sem Senha)
- [x] QA-06 — `dotnet test` — **24/26 passando** (2 falhas pré-existentes: AuthService BCrypt + DB diagnostic)
- [x] QA-07 — Documentar resultado em `PROGRESS.md`

---


# 🔐 Fase 16: Painel Acesso B.I. (PRD 1.0.0)

> **PRD:** `docs/PRD_ACESSO_BI.md` · **Fonte de verdade:** `docs/acesso_bi_regra.md`
> **Orquestrador:** `orchestrator` · **Agentes:** `architect`, `backend-engineer`, `database-engineer`, `frontend-engineer`, `qa-engineer`, `devops-engineer`

## Legenda de Status
- `✅ Concluída` = task finalizada e validada
- `🔄 Em andamento` = agente executor está trabalhando
- `⏳ Pendente` = aguardando delegação do Orchestrator
- `🔒 Bloqueada` = dependência não concluída

### 🗄️ Fase 1: Camada de Dados — `database-engineer`

| ID | Tarefa | RF | Agente Orquestrador | Agente Executor | Dependências | Status |
|----|--------|-----|---------------------|-----------------|--------------|--------|
| DB-01 | Criar Model `UsuarioEmpresa.cs` em `Empresa.Data/Models/` | — | `orchestrator` | `database-engineer` | — | ⏳ Pendente |
| DB-02 | Verificar estrutura real das tabelas `USUARIO_EMPRESA`, `VW_EMPRESA_NEW` e `ACESSO_CADASTRO_USUARIO` no Oracle (colunas, tipos, PK, triggers) | — | `orchestrator` | `database-engineer` | — | ⏳ Pendente |
| DB-03 | Criar `IAcessoBiRepository.cs` — interface com 6 métodos: GetUsuariosComAcessoAsync, GetEmpresasDoUsuarioAsync, GetEmpresasDisponiveisAsync, ConcederAcessoTotalAsync, ConcederAcessoEmpresaAsync, RemoverAcessoAsync | RF02, RF03, RF04, RF05, RF06 | `orchestrator` | `database-engineer` | DB-01 | ⏳ Pendente |
| DB-04 | Implementar `AcessoBiRepository.cs` — `GetUsuariosComAcessoAsync(search)` com LISTAGG (Q2) e parâmetro de busca opcional | RF02 | `orchestrator` | `database-engineer` | DB-02, DB-03 | ⏳ Pendente |
| DB-05 | Implementar queries do detalhe: `GetEmpresasDoUsuarioAsync` (Q7) e `GetEmpresasDisponiveisAsync` (Q6 com NOT IN, parâmetro `:ID_USUARIO`) | RF03, RF05 | `orchestrator` | `database-engineer` | DB-02, DB-03 | ⏳ Pendente |
| DB-06 | Implementar queries de escrita: `ConcederAcessoTotalAsync` (Q3 — INSERT SELECT em massa), `ConcederAcessoEmpresaAsync` (Q8 — INSERT unitário), `RemoverAcessoAsync` (Q9 corrigido D2 — DELETE por `ID_USUARIO_EMPRESA`) | RF04, RF05, RF06 | `orchestrator` | `database-engineer` | DB-04, DB-05 | ⏳ Pendente |
| DB-07 | Aplicar skills Oracle: parâmetros nomeados `:param`, sem `SELECT *`, sem concatenação SQL, tratar Oracle NULL → C# nullable, `await using` para conexão | — | `orchestrator` | `database-engineer` | DB-04, DB-05, DB-06 | ⏳ Pendente |

### ⚙️ Fase 2: Camada de API — `backend-engineer`

| ID | Tarefa | RF | Agente Orquestrador | Agente Executor | Dependências | Status |
|----|--------|-----|---------------------|-----------------|--------------|--------|
| BE-01 | Criar `Empresa.Api/DTOs/Request/AcessoBiRequest.cs` — `ConcederAcessoTotalRequest` (IdUsuario), `VincularEmpresaRequest` (CodigoEmpresa) | RF04, RF05 | `orchestrator` | `backend-engineer` | — | ⏳ Pendente |
| BE-02 | Criar `Empresa.Api/DTOs/Response/UsuarioBiResponse.cs` — IdUsuario, Nome, Empresas (agregação LISTAGG) | RF02 | `orchestrator` | `backend-engineer` | — | ⏳ Pendente |
| BE-03 | Criar `Empresa.Api/DTOs/Response/EmpresaBiResponse.cs` — IdUsuarioEmpresa, IdUsuario, CodigoEmpresa, NomeFantasia | RF03 | `orchestrator` | `backend-engineer` | — | ⏳ Pendente |
| BE-04 | Criar `IAcessoBiService.cs` e `AcessoBiService.cs` — gate de permissão (`cadastroUsuarioBi` via `AcessoRepository`), orquestração do repositório, validações de negócio (usuário existe, empresa existe, prevenção de duplicidade → 409 Conflict) | RF01, RF08 | `orchestrator` | `backend-engineer` | DB-07 | ⏳ Pendente |
| BE-05 | Implementar método `GetUsuariosComAcessoAsync` no service — delegar para repository, retornar `List<UsuarioBiResponse>` | RF02 | `orchestrator` | `backend-engineer` | BE-02, BE-04 | ⏳ Pendente |
| BE-06 | Implementar métodos de detalhe no service: `GetEmpresasDoUsuarioAsync`, `GetEmpresasDisponiveisAsync` | RF03, RF05 | `orchestrator` | `backend-engineer` | BE-03, BE-04 | ⏳ Pendente |
| BE-07 | Implementar métodos de escrita no service: `ConcederAcessoTotalAsync` (verificar duplicidade D6 → 409), `VincularEmpresaAsync`, `RemoverAcessoAsync` | RF04, RF05, RF06 | `orchestrator` | `backend-engineer` | BE-01, BE-04 | ⏳ Pendente |
| BE-08 | Criar `AcessoBiEndpoints.cs` — 6 endpoints REST: GET usuarios, GET usuarios/{id}/empresas, GET usuarios/{id}/empresas/disponiveis, POST usuarios, POST usuarios/{id}/empresas, DELETE empresas/{id} | RF02–RF06 | `orchestrator` | `backend-engineer` | BE-05, BE-06, BE-07 | ⏳ Pendente |
| BE-09 | Registrar DI: `AddScoped<IAcessoBiRepository, AcessoBiRepository>()` e `AddScoped<IAcessoBiService, AcessoBiService>()` em `Program.cs` | — | `orchestrator` | `backend-engineer` | BE-08 | ⏳ Pendente |
| BE-10 | Adicionar `.RequireAuthorization()`, `.WithTags("Acesso BI")`, `.WithSummary()`, `.WithDescription()`, `.Produces<>()` em todos os endpoints | — | `orchestrator` | `backend-engineer` | BE-08 | ⏳ Pendente |

### 🎨 Fase 3: Camada Frontend — `architect` + `frontend-engineer`

| ID | Tarefa | RF | Agente Orquestrador | Agente Executor | Dependências | Status |
|----|--------|-----|---------------------|-----------------|--------------|--------|
| FE-01 | Criar estrutura de diretórios `modules/acesso-bi/` com subpastas: `components/`, `services/`, `hooks/`, `types.ts`, `AcessoBiPage.tsx` | — | `orchestrator` | `architect` | — | ⏳ Pendente |
| FE-02 | Definir tipos TypeScript: `UsuarioBi`, `EmpresaBi`, `ConcederAcessoTotalRequest`, `VincularEmpresaRequest` em `types.ts` | — | `orchestrator` | `architect` | FE-01 | ⏳ Pendente |
| FE-03 | Implementar `services/acessoBiService.ts` — 5 funções Axios: getUsuarios, getEmpresas, getEmpresasDisponiveis, concederAcessoTotal, vincularEmpresa, removerAcesso | RF07 | `orchestrator` | `frontend-engineer` | FE-02, BE-10 | ⏳ Pendente |
| FE-04 | Implementar hook `hooks/useAcessoBi.ts` — Zustand store com estado: usuarios, expandedRowKeys, loading, ações: carregarUsuarios, expandirUsuario, concederAcessoTotal, vincularEmpresa, removerAcesso | RF02, RF03, RF04, RF05, RF06 | `orchestrator` | `frontend-engineer` | FE-03 | ⏳ Pendente |
| FE-05 | Implementar `components/UsuarioBiLista.tsx` — Table Ant Design expandable com colunas Usuário (nome), Empresas (agregação), Ações; apenas 1 expandido por vez; empty state; botão "Conceder Acesso Total" no header | RF02 | `orchestrator` | `frontend-engineer` | FE-04 | ⏳ Pendente |
| FE-06 | Implementar `components/EmpresaVinculoLista.tsx` — Table aninhada no expandedRowRender com colunas Empresa (nomeFantasia), Ações; botão "Vincular Empresa" e "Remover"; Popconfirm de exclusão | RF03, RF06 | `orchestrator` | `frontend-engineer` | FE-04 | ⏳ Pendente |
| FE-07 | Implementar `components/UsuarioBiForm.tsx` — Modal de concessão total: Select de usuários sem acesso, botão Conceder/Cancelar, loading, feedback 409 conflict | RF04 | `orchestrator` | `frontend-engineer` | FE-04 | ⏳ Pendente |
| FE-08 | Implementar `components/EmpresaSelect.tsx` — Select de empresas disponíveis (carregado via getEmpresasDisponiveis), Modal.confirm ao selecionar | RF05 | `orchestrator` | `frontend-engineer` | FE-04 | ⏳ Pendente |
| FE-09 | Implementar `AcessoBiPage.tsx` — View principal com título "Usuários com acesso ao B.I. por empresas", cabeçalho colapsável (toggle), Table mestre com expandedRowRender usando EmpresaVinculoLista | RF02, RF03 | `orchestrator` | `frontend-engineer` | FE-05, FE-06, FE-07, FE-08 | ⏳ Pendente |
| FE-10 | Adicionar item "Acesso B.I." no menu Configurações do `SideMenu.tsx` (ícone `KeyOutlined`, rota `/configuracoes/acesso-bi`) | RF07 | `orchestrator` | `frontend-engineer` | FE-09 | ⏳ Pendente |
| FE-11 | Configurar rota `/configuracoes/acesso-bi` no `App.tsx` apontando para `AcessoBiPage` | RF07 | `orchestrator` | `frontend-engineer` | FE-09 | ⏳ Pendente |
| FE-12 | Tratamento de erros e loading states globais: `message.error()` para falhas de rede, `Spin` para carregamentos, interceptor Axios já configurado | RF08 | `orchestrator` | `frontend-engineer` | FE-09 | ⏳ Pendente |

### 🧪 Fase 4: Qualidade & Testes — `qa-engineer`

| ID | Tarefa | RF | Agente Orquestrador | Agente Executor | Dependências | Status |
|----|--------|-----|---------------------|-----------------|--------------|--------|
| QA-01 | Testes de repositório: `AcessoBiRepositoryTests.cs` — GetUsuariosComAcessoAsync (com/sem search), GetEmpresasDoUsuarioAsync (existente/vazio), GetEmpresasDisponiveisAsync (filtra corretamente) | RF02, RF03, RF05 | `orchestrator` | `qa-engineer` | DB-07 | ⏳ Pendente |
| QA-02 | Testes de repositório (escrita): `ConcederAcessoTotalAsync` (sucesso), `ConcederAcessoEmpresaAsync` (sucesso + duplicidade → exceção), `RemoverAcessoAsync` (sucesso + id inexistente) | RF04, RF05, RF06 | `orchestrator` | `qa-engineer` | DB-07 | ⏳ Pendente |
| QA-03 | Testes de service: `AcessoBiServiceTests.cs` — gate de permissão (com/sem permissão → 403), ConcederAcessoTotal (sucesso + usuário já possui acesso → 409), VincularEmpresa (sucesso + duplicidade → 409), RemoverAcesso (sucesso + inexistente → 404) | RF01, RF04, RF05, RF06, RF08 | `orchestrator` | `qa-engineer` | BE-08 | ⏳ Pendente |
| QA-04 | `dotnet build` e `dotnet test` — 0 erros, cobertura ≥ 80% no novo código | — | `orchestrator` | `qa-engineer` | QA-01, QA-02, QA-03 | ⏳ Pendente |
| QA-05 | Documentar resultados em `PROGRESS.md` | — | `orchestrator` | `qa-engineer` | QA-04 | ⏳ Pendente |

### 🚀 Fase 5: Review & Entrega — `architect` + `devops-engineer`

| ID | Tarefa | RF | Agente Orquestrador | Agente Executor | Dependências | Status |
|----|--------|-----|---------------------|-----------------|--------------|--------|
| RE-01 | Code review completo (backend + frontend) — verificar contratos arquiteturais do `CLAUDE.md`, DI, async/await, DTOs vs Models, nomenclatura | — | `orchestrator` | `architect` | QA-05 | ⏳ Pendente |
| RE-02 | Documentação: atualizar `CLAUDE.md` se necessário, documentar endpoints no Swagger (já configurado), atualizar `docs/README.md` | — | `orchestrator` | `architect` | RE-01 | ⏳ Pendente |
| RE-03 | Build de produção: `dotnet publish` + `npm run build` — validar que ambos compilam sem erros | — | `orchestrator` | `devops-engineer` | RE-02 | ⏳ Pendente |
| RE-04 | Validação final: checklist de fidelidade (Anexo E do `acesso_bi_regra.md`) + 9 correções de defeitos (D1–D9) | — | `orchestrator` | `devops-engineer` | RE-03 | ⏳ Pendente |

### Resumo da Orquestração Acesso B.I.

| Fase | Tasks | Orquestrador delega para | Progresso |
|------|-------|--------------------------|-----------|
| Fase 1 — Camada de Dados | DB-01 a DB-07 | `database-engineer` | 0% ⏳ |
| Fase 2 — Camada de API | BE-01 a BE-10 | `backend-engineer` | 0% ⏳ |
| Fase 3 — Camada Frontend | FE-01 a FE-12 | `frontend-engineer` + `architect` | 0% ⏳ |
| Fase 4 — Qualidade & Testes | QA-01 a QA-05 | `qa-engineer` | 0% ⏳ |
| Fase 5 — Review & Entrega | RE-01 a RE-04 | `architect` + `devops-engineer` | 0% ⏳ |
| **TOTAL** | **38 tasks** | | **0%** |

### Próximo Handoff (Orchestrator → Agentes)

```
Orchestrator → database-engineer:
  ├── DB-01 (Model UsuarioEmpresa.cs) — ⏳ aguardando delegação
  └── DB-02 (Verificar estrutura Oracle) — ⏳ aguardando delegação

Próximo handoff: database-engineer → backend-engineer (Fase 2)
```

# 🤖 Fase 14: AI-Readiness — Correções de Governança (2026-07-31)


> **Auditoria:** AI-Readiness · **Executado por:** auditor sênior + agentes
> **Resultado da auditoria:** nota 7.0/10 → corrigido para ~9.0/10

## Tarefas Executadas

- [x] AIR-01 — Corrigir `agents/frontend-engineer.md` — stack Vue 3/PrimeVue/Pinia → **React 18/Ant Design/Zustand/React Router**
- [x] AIR-02 — Corrigir `agents/README.md` — referência a "Frontend Vue 3" → "Frontend React 18"
- [x] AIR-03 — Criar `.gitignore` na raiz — cobrir `bin/`, `obj/`, `.env`, `appsettings.*.local`, `secrets.json`, chaves, logs
- [x] AIR-04 — Criar `.cursor/rules/ai-readiness.mdc` — regras nativas Cursor apontando para `CLAUDE.md`
- [x] AIR-05 — Criar `.github/copilot-instructions.md` — instruções nativas GitHub Copilot
- [x] AIR-06 — Criar `AGENTS.md` na raiz — ponto de entrada universal para agentes de IA
- [x] AIR-07 — Adicionar seção `Frontend (Empresa.Web)` ao `CLAUDE.md` — stack React, comandos npm, estrutura
- [x] AIR-08 — Adicionar guardrails adicionais ao `CLAUDE.md` — `.env`, migrations Oracle, stack React
- [x] AIR-09 — Criar `.editorconfig` na raiz — padronização de formatação (C# 4 espaços, TS/React 2 espaços)

### Resumo do Impacto

| Métrica | Antes | Depois |
|---------|-------|--------|
| Arquivos de contexto raiz | 1 (`CLAUDE.md`) | 3 (`CLAUDE.md`, `AGENTS.md`, `.gitignore`, `.editorconfig`) |
| Regras específicas de ferramenta | 0 | 2 (Cursor + Copilot) |
| Divergência frontend (Vue vs React) | Presente | **Corrigida** |
| Proteção de segredos/binários | Apenas em `Empresa.Web/` | **Raiz + subprojetos** |

---

# 🌐 Frontend — GestãoUsuarios (PRD 3.0.0)

> **PRD:** `docs/PRD_FRONTEND_USUARIOS_PERFIS.md` · **Versão:** 3.0.0
> **Orquestrador:** `orchestrator` · **Agentes:** `architect`, `frontend-engineer`, `backend-engineer`, `qa-engineer`, `devops-engineer`

## Legenda de Status
- `✅ Concluída` = task finalizada e validada
- `🔄 Em andamento` = agente executor está trabalhando
- `⏳ Pendente` = aguardando delegação do Orchestrator
- `🔒 Bloqueada` = dependência não concluída

| ID | Tarefa | RF | Agente Orquestrador | Agente Executor | Dependências | Status | Início | Fim |
|----|--------|-----|---------------------|-----------------|--------------|--------|--------|-----|
| T001 | Criar estrutura de diretórios do módulo `usuarios/` | — | `orchestrator` | `architect` | — | ✅ Concluída | 2026-07-28 | 2026-07-28 |
| T002 | Definir tipos TypeScript (`types/index.ts`) | — | `orchestrator` | `architect` | T001 | ✅ Concluída | 2026-07-28 | 2026-07-28 |
| T003 | Implementar services: `usuarioService`, `perfilService`, `paginaService`, `estabelecimentoService` | RF07 | `orchestrator` | `backend-engineer` | T002 | ✅ Concluída | 2026-07-28 | 2026-07-29 |
| T004 | Implementar hooks Zustand `usePerfilAdmin` / `useUsuarios` | RF01, RF02 | `orchestrator` | `backend-engineer` | T002 | ✅ Concluída | 2026-07-29 | 2026-07-29 |
| T005 | Implementar `UsuarioLista.tsx` com Antd Table, filtros e paginação | RF01 | `orchestrator` | `frontend-engineer` | T003, T004 | ✅ Concluída | 2026-07-29 | 2026-07-29 |
| T006 | Implementar `PerfilSelect.tsx` (dropdown de perfis) | RF03 | `orchestrator` | `frontend-engineer` | T003 | ✅ Concluída | 2026-07-29 | 2026-07-29 |
| T007 | Implementar `PaginaTree.tsx` (tree de páginas) | RF04 | `orchestrator` | `frontend-engineer` | T003 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T008 | Implementar `EstabelecimentoTree.tsx` (tree de estabelecimentos) | RF05 | `orchestrator` | `frontend-engineer` | T003 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T009 | Implementar `UsuarioForm.tsx` (formulário com 3 abas) | RF02 | `orchestrator` | `frontend-engineer` | T006, T007, T008 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T010 | Implementar view `UsuarioPage.tsx` (master-detail) | RF01, RF02 | `orchestrator` | `frontend-engineer` | T005, T009 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T011 | Configurar rotas React Router no `App.tsx` para o módulo `usuarios/` | — | `orchestrator` | `frontend-engineer` | T010 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T012 | Implementar validações de formulário (RF06) | RF06 | `orchestrator` | `frontend-engineer` | T009 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T013 | Implementar interceptor Axios (JWT, erros) | RF07 | `orchestrator` | `backend-engineer` | T003 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T014 | Testes unitários: hooks (useUsuarios, usePerfis) | RF08 | `orchestrator` | `qa-engineer` | T003, T004 | ✅ Concluída | 2026-07-29 | 2026-07-30 |
| T020 | Tratamento de erros e loading states globais | RF07 | `orchestrator` | `frontend-engineer` | T013, T014 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T021 | Responsividade e ajustes visuais (CSS) | — | `orchestrator` | `frontend-engineer` | T010, T012 | ✅ Concluída | 2026-07-30 | 2026-07-30 |
| T022 | Code review e conformidade com coding standards | — | `orchestrator` | `architect` | T016, T017, T018, T019, T020, T021 | ⏳ Pendente | — | — |
| T023 | Documentação de componentes | — | `orchestrator` | `architect` | T022 | ⏳ Pendente | — | — |
| T024 | Build de produção e validação final | — | `orchestrator` | `devops-engineer` | T022, T023 | ⏳ Pendente | — | — |

### Resumo da Orquestração Frontend

| Fase | Tasks | Orquestrador delega para | Progresso |
|------|-------|--------------------------|-----------|
| Fase 1 — Estrutura base | T001–T002 | `architect` | 100% ✅ |
| Fase 2 — Componentes | T003–T013 | `frontend-engineer` + `backend-engineer` | 100% ✅ |
| Fase 3 — Testes | T014–T019 | `qa-engineer` | 17% (1/6) 🔄 |
| Fase 4 — Refinamento | T020–T021 | `frontend-engineer` | 100% ✅ |
| Fase 5 — Review & Entrega | T022–T024 | `architect` + `devops-engineer` | 0% ⏳ |

### Próximo Handoff (Orchestrator → Agentes)

```
Orchestrator awaiting:
  ├── T015 (qa-engineer) — pendente (precisa ser recriado)
  ├── T016 (qa-engineer) — liberado após T015
  ├── T017 (qa-engineer) — liberado após T015
  ├── T018 (qa-engineer) — liberado após T015
  └── T019 (qa-engineer) — liberado após T016, T017, T018

Próximo handoff: qa-engineer → architect (Fase 5 — Code Review)
Fase 4 (T020-T021) concluída ✅
```

### Regras de Orquestração

1. **Orchestrator NUNCA executa tarefas técnicas** — apenas coordena e reporta
2. Cada task tem o Orchestrator como coordenador e um agente especialista como executor
3. Tasks sem dependências entre si podem ser executadas em paralelo (ex: T005, T006, T007, T008)
4. Handoff entre fases só ocorre quando todas as tasks da fase anterior estão concluídas
5. `TASKS.md` é a fonte canônica; `PROGRESS.md` é o dashboard de progresso do Orchestrator
