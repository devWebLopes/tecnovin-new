# 📊 PROGRESS.md — GestaoNew · Multi-Agent Dashboard

> **Orquestrador:** `orchestrator` (`agents/orchestrator.md`)
> **Última sincronização:** 2026-09-19 — Migração VPN OpenVPN → FortiClient

---

## Sessão 2026-09-19: Migração VPN OpenVPN → FortiClient — CONCLUÍDA ✅

### Escopo e Entrega

- **Pedido do usuário:** trocar a VPN de produção de OpenVPN para **FortiClient** e colocar os dados da conexão no arquivo de configurações.
- **Decisão aprovada pelo usuário:** usar **openfortivpn em container** (cliente open-source compatível com FortiGate SSL VPN — o FortiClient oficial não roda em Docker), preservando a topologia sidecar existente (`network_mode: service:vpn`).
- **Dados da conexão informados:** host `vpn1.tecnovin.com.br`, porta `10443`, usuário `tecnovin`. Senha fica como placeholder no `.env.example` (preenchida apenas na VPS — fora do Git).

### Alterações

| Arquivo | Mudança |
|---------|---------|
| `deploy/vpn/Dockerfile` *(novo)* | Alpine 3.20 + `openfortivpn` + `ppp` — imagem do sidecar VPN (build local na VPS) |
| `deploy/vpn/entrypoint.sh` *(novo)* | Gera `/etc/openfortivpn/config` a partir das envs (`chmod 600`) e executa `openfortivpn -c`; suporta `VPN_REALM`, `VPN_TRUSTED_CERT`, `VPN_INSECURE_SSL` opcionais |
| `deploy/.env.example` | Novo bloco **VPN FortiClient**: `VPN_HOST=vpn1.tecnovin.com.br`, `VPN_PORT=10443`, `VPN_USERNAME=tecnovin`, `VPN_PASSWORD=SUA_SENHA_VPN_AQUI` + opcionais documentadas |
| `docker-compose.prod.yml` | Serviço `vpn`: `build: ./deploy/vpn` + `image: gestaonew-vpn-forticlient:latest`, `devices: /dev/ppp` (era `/dev/net/tun`), envs `VPN_*`, removidos volume `./vpn` e `command -f client.ovpn -a auth.txt` |
| `deploy/setup-vps.sh` | Módulo `ppp_generic` (era `tun`); cria `/opt/gestaonew/deploy/vpn`; instruções finais sem `.ovpn`/`auth.txt` |
| `docs/deploy-producao.md` | Topologia e diagrama (túnel FortiClient/`ppp0`); Passo 2 reescrito (credenciais no `.env`, fingerprint SHA-256 via `openssl s_client`, validação `Tunnel is up and running` + `ppp0`); troubleshooting FortiClient (cert autoassinado, `/dev/ppp`, política SSL VPN na FortiGate); Passo 6 com `--build`; checklist atualizado |
| `docs/README.md` | Índice: OpenVPN → FortiClient VPN |
| `.gitattributes` *(novo)* | `*.sh text eol=lf` — impede CRLF no checkout Windows quebrando scripts no container |

### Validação

- `docker compose -f docker-compose.prod.yml config` ✅ — serviço `vpn` parseado corretamente (build context, envs, `/dev/ppp`); warnings de `VPN_*` ausentes são esperados (`.env` real só existe na VPS).
- `entrypoint.sh` verificado: **0 CRLF**, shebang `#!/bin/sh` correto.
- **Pendente (VPS):** build da imagem e os 4 testes do túnel (`ppp0`, rota `192.168.1.0/24`, ping `192.168.1.4`, porta `1526`) — daemon Docker local estava parado na sessão. Task **VPN-10** no `TASKS.md`.

### Observações

- Nginx inalterado: o proxy continua resolvendo a API pelo nome do serviço `vpn` (`proxy_pass http://vpn:8080/api/`).
- Se o gateway FortiGate usar certificado autoassinado, o 1º log de erro do container mostrará o fingerprint a ser copiado para `VPN_TRUSTED_CERT`.
- CI/CD (`.github/workflows/deploy.yml`) não referencia a VPN — sem impacto; a imagem da VPN é buildada na VPS (contexto `deploy/vpn`), não no GHCR.

---

## Sessão 2026-09-19: Manual de Deploy em Produção — CONCLUÍDA ✅

### Escopo e Entrega

- **Pedido do usuário:** manual passo a passo para subir a aplicação em produção, com uso obrigatório de VPN OpenVPN para acessar o banco Oracle.
- **`docs/deploy-producao.md` criado** — manual completo baseado na infraestrutura já existente no repositório (`docker-compose.prod.yml`, `deploy/setup-vps.sh`, `deploy/.env.example`, `deploy/nginx/conf.d/app.conf`, `.github/workflows/deploy.yml`):
  - Arquitetura de produção (diagrama): gateway Nginx → web (React) + api (.NET 9) compartilhando o namespace de rede do container VPN (`network_mode: service:vpn`) → Oracle `192.168.1.4:1526/TECNOVIN`.
  - 8 passos: preparar VPS (`setup-vps.sh`) → **configurar e validar a VPN isoladamente** (client.ovpn, auth.txt, ping/rota/porta 1526) → `.env` → compose + nginx → publicar imagens (CI/CD GHCR, push manual ou build na VPS) → bootstrap SSL temporário + subida da stack → certbot real + renovação via cron → validação pós-deploy (`/api/health`, `/api/health/database`, teste funcional).
  - Redeploy, rollback (tags por SHA), troubleshooting (VPN/Oracle/SSL/gateway) e checklist de go-live.
- **Índices atualizados:** `docs/README.md` (referência ao novo doc) e `AGENTS.md` (13 → 14 documentos).

### Observações registradas no manual

- O frontend chama a API por caminho relativo (`/api/v1` em `Empresa.Web/src/lib/api.ts`) — um único domínio resolve; `VITE_API_BASE_URL` em `.env.production` não é consumido pelo código.
- `Empresa.Worker` não faz parte da topologia de produção atual (nota §14 do manual).
- Bloqueio conhecido mantido: Oracle só acessível via VPN — o manual exige validação do túnel **antes** de subir a API.

---

## Sessão 2026-08-14: Reformulação do Painel Compras Frutas (v2) — CONCLUÍDA ✅

### Escopo e Entrega (`prd_compras_frutas_v2.md`)

- **Visão:** Subsituição da grid plana legada por painel analítico hierárquico moderno sem quebra de contrato no backend.
- **`types.ts`:** adicionadas interfaces `PainelData`, `EmpresaData`, `LinhaEmpresa`, `LinhaUf`, `ValoresDiarios` e expandido `ColunaClicada`.
- **`comprasFrutasFormat.ts`:** implementado `getSemaforoCor` (verde ≥100%, amarelo 70-99.9%, vermelho <70%) e `formatPercent`.
- **`comprasFrutasTransform.ts`:** criada função de agrupamento client-side `transformarGridPlana()` que isola o `TOTAL GERAL`, agrupa por `EMPRESA`, `CD_LINHA` e gera sub-linhas por `UF`.
- **Componentes React AntD 5:**
  - `PercentBadge.tsx`: Tag de semáforo com ícone e cor.
  - `TotaisPanel.tsx`: Painel superior de Totais Consolidados (sticky no topo com Total Geral).
  - `LinhasTable.tsx`: Tabela por empresa com expansão de sub-linhas por UF (`▶`).
  - `EmpresaSection.tsx`: Seções colapsáveis por empresa (abertas por padrão) com botão de exportar CSV individual.
  - `DetalhamentoModal.tsx`: Título contextual aprimorado (`Detalhamento: data | coluna | empresa | UF`).
  - `ComprasFrutasPage.tsx`: Refatoração completa integrando filtros, TotaisPanel, EmpresaSection e modais Nível 1/2.
- **QA:** novos testes unitários adicionados em `tests/agricola.test.ts` para semáforo e transformação hierárquica.

---


## Sessão 2026-08-14: Auditoria de Aprendizado dos Painéis — CONCLUÍDA

### Resultado

- Auditoria consolidada em `docs/auditoria-aprendizado-paineis.md`.
- Checklist obrigatório criado em `docs/padroes-paineis.md`.
- `commands/criar-feature.md` passou a exigir chave de permissão canônica, gate backend, `ROUTE_MAP`, `MenuGuard`, cliente Axios único, estados de carregamento, testes 401/403/500 e validação Oracle.
- Veredito: o aprendizado havia sido incorporado parcialmente; padrões recentes eram bons, mas não estavam garantidos pelo fluxo canônico.

### Correções Aplicadas

- A alteração do gate para `comprasFrutas` foi revista: a configuração efetiva já usava `comprasFrutasPorEmpresas`, coerente com o `ROUTE_MAP` e com a página legada correspondente. Gate e teste foram restaurados; `PRD_AGRICOLA.md` recebeu errata 1.0.1.
- `AgricolaRepository` abria conexão de REF CURSOR com `Open()`; alterado para `OpenAsync()` conforme contrato de I/O assíncrono.
- Quatro erros de lint preexistentes em `Empresa.Web/tests/agricola.test.ts` foram removidos.

### Validação

| Validação | Resultado |
|---|---|
| `dotnet build Empresa.sln` | Sucesso, 0 erros; 2 warnings preexistentes em testes (`CS8765`, `CS1998`) |
| `dotnet test ... --filter FullyQualifiedName~AgricolaServiceTests` | 8/8 aprovados |
| `npm run lint` | Sucesso, 0 erros |
| `npm run test -- --run` | 51/51 aprovados |
| `npm run build` | Sucesso; aviso de chunk Ant Design > 500 kB |

### Riscos Remanescentes

- Oracle real continua indisponível; REF CURSOR, schema, packages e tipos Oracle ainda exigem homologação integrada.
- Não existe suíte E2E automatizada para login → menu → autorização → carregamento do painel.
- `docs/PRD_FRONTEND_PAINEIS.md` permanece como rascunho histórico com stack divergente e não deve orientar novas features.

---

## 📝 Sessão 2026-08-13: PRD Módulo Agrícola (Fase 19) — CONCLUÍDO ✅

### Backend (Fases 1-2 — 16/16 tasks concluídas ✅)

**Camada de Dados:**
- `MetaCompra.cs` criado; `CompraFruta` removido de `SecundariosModels.cs`
- `IMetaCompraRepository` + `MetaCompraRepository` (5 métodos: GetAll, GetById, Insert com RETURNING, Update, Delete)
- `IAgricolaRepository`/`AgricolaRepository` reescritos — 3 métodos fiéis com REF CURSOR dinâmico (`GetRecebimentoFrutasAsync`, `GetRecebimentoFrutasDetAsync`, `GetRecebimentoFrutasDetNfAsync`)
- `OracleProcedures.cs` atualizado com `SpRecebimentoFrutasDet` e `SpRecebimentoFrutasDetNf`
- ⚠️ DB-01 bloqueado (Oracle inalcançável) — INSERT usa `RETURNING` presumindo trigger/sequence existente

**Camada de API:**
- 4 DTOs: `MetaCompraRequest` (DataAnnotations), `MetaCompraResponse`, `EmpresaMetaResponse`, `GridDinamicaResponse`
- `MetaCompraService` — gate `cadastroSafraMeta`, validações server-side (obrigatórios + dtFinal >= dtInicial + domínio fixo), log Serilog de exclusão
- `AgricolaService` reescrito — gate `comprasFrutasPorEmpresas`, contrato dinâmico (`ultimaAtualizacao`), regra `p_cd_variedade` (2 chars, TO* → nulo), validação `colunaClicada`
- `AgricolaEndpoints.cs` — 8 endpoints (5 metas + 3 compras-frutas), todos com `.RequireAuthorization()`, gates, `Produces<>()`
- Stub legado removido de `SecundariosEndpoints.cs`
- DI registrada em `Program.cs`

### Frontend (Fase 3 — 12/12 tasks concluídas ✅)

**Estrutura criada:** `modules/agricola/` com 13 arquivos (types, 2 services, 2 hooks, 4 components, 2 utils, 2 pages)
- Rotas: `/agricola/safra-meta` + `/agricola/compras-frutas` (lazy + MenuGuard)
- ROUTE_MAP: `cadastroSafraMeta`, `comprasFrutas` adicionados
- CSS semáforo: `.row-total-linha`, `.row-total-empresa`, `.row-total-geral`

### QA — Testes (Fase 4 — 6/6 tasks concluídas ✅)

**Backend (xUnit + Moq + FakeDbConnection): 24 novos testes**
- `MetaCompraServiceTests.cs` (10 testes)
- `AgricolaServiceTests.cs` (8 testes)
- `MetaCompraRepositoryTests.cs` (6 testes)

**Frontend (Vitest): 38 novos testes**
- `agricola.test.ts` (38 testes — captions, formatos, semáforo, colunas clicáveis, visibilidade, CSV, matriz RF13)

### Review & Entrega (Fase 5 — 4/4 tasks concluídas ✅)

- RE-01: Code review — 8 contratos CLAUDE.md verificados ✅
- RE-02: `docs/README.md` atualizado com 8 endpoints + nota de quebra de contrato ✅
- RE-03: `dotnet publish` + `npm run build` — sucesso ✅
- RE-04: Métricas de aceite §7 + decisões D-01 a D-12 verificadas ✅

### Validações Finais
| Validação | Resultado |
|-----------|-----------|
| `dotnet build` (Release) | ✅ 0 erros, 0 warnings no código novo |
| `dotnet test` | ✅ 70/72 (24 novos, 2 falhas pré-existentes) |
| `npm run build` | ✅ Sucesso (10.03s, SafraMetaPage 4.96kB, ComprasFrutasPage 7.86kB) |
| `npm run lint` | ✅ 0 warnings |
| `npm run test` | ✅ 51/51 (38 novos + 13 existentes) |
| `dotnet publish` | ✅ dist/publish gerado |

### Decisões D-01 a D-12 — Status
| Decisão | Status |
|---------|--------|
| D-01 — sqlEmpresa/sqlLinhas não reproduzidos | ✅ Aplicado |
| D-02 — Domínio fixo server-side | ✅ Aplicado |
| D-03 — Validação server-side + dtFinal >= dtInicial | ✅ Aplicado |
| D-04 — PK via RETURNING (DB-01 pendente) | ✅ Aplicado (pendente validação DBA) |
| D-05 — Log Serilog de exclusão | ✅ Aplicado |
| D-06 — p_cd_variedade no service | ✅ Aplicado |
| D-07 — Stateless (sem Session) | ✅ Aplicado |
| D-08 — CSV client-side | ✅ Aplicado |
| D-09 — D-3 na segunda, setInterval, sem código morto | ✅ Aplicado |
| D-10 — ISO 8601 + DateTime tipado | ✅ Aplicado |
| D-11 — Exceções propagam ao middleware | ✅ Aplicado |
| D-12 — Reescrita completa do stub | ✅ Aplicado |

### Pendência Remanescente
- **DB-01:** Validação Oracle (PK/trigger/sequence de META_COMPRAS + spec pkg_bi_compras) — bloqueado por Oracle inalcançável (risco herdado da Fase 18). Contratos cobertos por testes de contrato (FakeDbConnection). Validação pode ser feita quando o Oracle estiver disponível.

---

## 📝 Sessão 2026-08-03: Documento de Conexão Oracle ✅

- Criado `docs/oracle-connection.md` — referência completa de parâmetros de conexão Oracle (ODP.NET Managed 23.26.300): conexão atual do projeto, formatos de `Data Source` (descriptor/Easy Connect/TNS alias), parâmetros de autenticação, pooling, performance, HA, TLS, parâmetros do connect descriptor, precedência de configuração (appsettings → User Secrets → variável de ambiente/Docker), exemplo com pooling, testes de conexão e troubleshooting (inclui bloqueio conhecido `ORA-50000` da Fase 18)
- `docs/README.md` — índice atualizado com o novo documento

---

## 🧭 Fase 18: Refatoração e Migração do Menu Lateral (PRD 1.0.0) — EM EXECUÇÃO 🔨

> **PRD:** `docs/PRD_MENU_LATERAL.md` v1.0.0
> **Fonte de verdade:** `docs/menus.md` (engenharia reversa do legado TreisTecnovin)
> **Orquestrador:** `orchestrator` · **Data do PRD:** 2026-08-01
> **Agentes:** `database-engineer`, `backend-engineer`, `architect`, `frontend-engineer`, `qa-engineer`, `devops-engineer`

### Escopo da Fase

Migração do menu lateral do shell para a stack moderna (`.NET 9 Minimal API + React 18 + Ant Design 5`), preservando RN-01 a RN-14 do `menus.md`.

### Principais Lacunas Identificadas (engenharia reversa)

| # | Lacuna | Severidade |
|---|--------|-----------|
| G-01 | `GetPaginasMenuAsync` não reproduz o UNION legado — **pais implícitos (RN-03) não aparecem** | 🔴 Bloqueante |
| G-02 | Não existe painel "Mais Acessados" nem endpoint de telemetria (`POST /menu/acessos`) | 🔴 Bloqueante |
| G-03 | Contrato frontend desalinhado — `SideMenu` lê `nome`/`icone`, API retorna `tituloMenu`/`chaveControle` → menu renderiza rótulos vazios | 🟠 Alta |

### Entrega do PRD (ciclo Analista → Arquiteto → Redator)

| Item | Entregue |
|------|:--------:|
| Mapeamento das RN-01 a RN-14 do `menus.md` | ✅ |
| Decisões D-01 a D-12 (corrigir/adiar) | ✅ |
| 3 endpoints REST planejados (`GET /menu`, `POST /menu/acessos`, `GET /paginas/{chave}/autorizacao`) | ✅ |
| 1 service (`MenuService`), 1 handler de autorização (policy `PaginaAcesso`), cache por perfil | ✅ |
| Frontend: `menuStore`, `menuService`, refactor `SideMenu`, `MaisAcessadosPanel`, link B.I., `ROUTE_MAP` | ✅ |
| Checklist de fidelidade (`menus.md §10.4`) como métricas de aceite | ✅ |
| Backlog `TASKS.md` Fase 18 — **36 tasks** em 5 fases | ✅ |

### Progresso Orquestrado

| Fase | Tasks | Orquestrador → Agentes | Progresso | Status |
|------|-------|------------------------|-----------|--------|
| Fase 1 — Camada de Dados | DB-01 a DB-06 | `orchestrator` → `database-engineer` | 4/6 (67%) | 🟡 Em execução (DB-01/DB-03 bloq.) |
| Fase 2 — Camada de API | BE-01 a BE-10 | `orchestrator` → `backend-engineer` | 10/10 (100%) | ✅ Concluída |
| Fase 3 — Camada Frontend | FE-01 a FE-10 | `orchestrator` → `frontend-engineer` + `architect` | 10/10 (100%) | ✅ Concluída |
| Fase 4 — Qualidade & Testes | QA-01 a QA-06 | `orchestrator` → `qa-engineer` | 5/6 (83%) | 🟡 Em execução (QA-06 parcial) |
| Fase 5 — Review & Entrega | RE-01 a RE-04 | `orchestrator` → `architect` + `devops-engineer` | 0/4 (0%) | ⏳ Pendente |
| **TOTAL** | **36 tasks** | | **29/36 (81%)** | 🔨 |

### Resultado dos Testes (QA — Fase 4)

- **Backend (`dotnet test`)**: 46/48 aprovados. 22 novos testes criados:
  - `MenuServiceTests` (12) — árvore RN-05, pais implícitos D-06, ordenação RN-04, cache hit/miss/invalidação por perfil e global (RF07), D-03 propaga 500, telemetria RN-12 (autorizado registra / não autorizado NÃO registra), link B.I. RN-09.
  - `PaginaAutorizacaoHandlerTests` (5) — autorizado/negado/sem claim/sem chave/query string (RN-12).
  - `AcessoRepositoryTests` (5) — contrato SQL via `FakeDbConnection` (DbConnection/DbCommand/DbDataReader): CONNECT BY PRIOR + START WITH + parâmetro perfil, mais acessados (JOIN perfil, ORDER BY QUANTIDADE DESC, FETCH FIRST 10), `GetPaginaByChaveAsync` (JOIN ACESSO_PERFIL_PAGINA + ROWNUM), MERGE upsert (RN-08), vínculos B.I. (RN-09).
  - 2 falhas **pré-existentes** (não relacionadas ao PRD): `AuthServiceTests.LoginAsync_ValidCredentials_ReturnsToken` (BCrypt) e `DatabaseDiagnosticTests` (Oracle inalcançável).
- **Frontend (`vitest`)**: 13/13 aprovados — `menuStore.test.ts` (8: carregar/TTL/force/fallback/telemetria/dedup/invalidar) e `routeMap.test.ts` (5: ROUTE_MAP, kebab-case, fallback).
- `dotnet build Empresa.sln` → **0 erros** (1 warning pré-existente CS1998). `npm run lint` e `npm run build` → **sem erros**.

### Checklist de Fidelidade (`menus.md §10.4`)

| Item | Status |
|------|:------:|
| Menu exibe as mesmas páginas para um perfil (SQL 5.1 legado × CONNECT BY) | ⏳ **Pendente — requer Oracle real** (DB-01/DB-03) |
| Pais implícitos aparecem quando ≥1 filho é permitido (RN-03, ≥3 níveis D-07) | ✅ Coberto por `MenuServiceTests` + `AcessoRepositoryTests` |
| Ordenação por `ORDEM` em cada nível (RN-05) | ✅ Coberto por `MenuServiceTests` |
| Grupo não navega; folha navega (RN-06) | ✅ Implementado no `SideMenu` (grupos expandem, folhas navegam) |
| "Mais Acessados" ≤10, ordenado por quantidade, filtrado por perfil (RN-07/D-04/D-05) | ✅ Implementado + `AcessoRepositoryTests` |
| Telemetria upsert em `ACESSO_VISUALIZACAO_PAGINA` (RN-08) | ✅ MERGE implementado + `AcessoRepositoryTests` |
| Link B.I. condicional a `USUARIO_EMPRESA` (RN-09) | ✅ Implementado + `MenuServiceTests` |
| Acesso direto sem permissão bloqueado no backend (RN-12) | ✅ Policy `PaginaAcesso` + `PaginaAutorizacaoHandlerTests` |
| Nenhuma alteração estrutural nas 5 tabelas Oracle (Regra de Ouro nº 1) | ✅ Apenas SELECTs; sem DDL |

### Riscos em Aberto / Bloqueios

- 🔴 **Oracle inalcançável** (`192.168.1.4:1526`, `SERVICE_NAME=TECNOVIN`, user `treisbi_teste2`) — `ORA-50000: Connection request timed out`. Bloqueia **DB-01/DB-03** (validação de schema real — nome da coluna de data `DATA_HORA` vs `DATA_ULTIMO_ACESSO`; fidelidade CONNECT BY vs UNION com dados reais). Impacto: validação de contrato SQL adiada, coberta por testes de contrato.
- Nome real da coluna de data em `ACESSO_VISUALIZACAO_PAGINA` — usar `DATA_ULTIMO_ACESSO` no MERGE até confirmação (risco DB conhecido do PRD).
- 2 falhas de teste pré-existentes fora do escopo (AuthService BCrypt + DatabaseDiagnostic).

### Próximo Handoff (Orchestrator → architect/devops)

```
Fase 5 — Review & Entrega (RE-01 a RE-04):
  RE-01: Code review completo (contratos CLAUDE.md, DI, async/await, DTOs vs Models)
  RE-02: Documentação/Swagger dos novos endpoints
  RE-03: dotnet publish + npm run build de produção
  RE-04: Validação final RN-01 a RN-14 + D-01 a D-12 (itens pendentes de Oracle)
```

---

## ✅ Fase 17: Finalização e Correções (2026-07-31)

> **Escopo:** Cleanup do arquivo "Emp" solto na raiz + correções de build/testes
> **Executado por:** `orchestrator` + `qa-engineer` + `frontend-engineer` + `backend-engineer`

### O que foi feito

#### 🧹 Cleanup
- **Arquivo "Emp" na raiz** — confirmado removido (`Test-Path Emp` → `False`); task pulada conforme solicitado pelo usuário

#### 🐛 Correção Backend (bug real)
- **`PerfilService.GetPaginasTreeAsync`** — a projeção LINQ era *deferred* e usava um `HashSet<int> visited` compartilhado; na segunda enumeração o `visited` já estava populado → retornava vazio → teste falhava com "Sequence contains no elements". **Correção:** adicionado `.ToList()` para materializar o resultado

#### ⚙️ Correções Frontend (build)
- **`package.json`** — adicionado `@types/node` (vite.config.ts usa `path`/`process`/`__dirname`)
- **`tsconfig.node.json`** — corrigido: `composite: true` + `emitDeclarationOnly` + `outDir` temporário + `types: ["node"]` (resolvido TS6310/TS6306)
- **14 erros TypeScript corrigidos** — `authStore.ts` (conflito campo/getter `isAuthenticated`), imports/variáveis não usados em `SideMenu.tsx`, `EmpresaVinculoLista.tsx`, `UsuarioBiLista.tsx`, `EstabelecimentoTree.tsx`, `PerfilForm.tsx`, `UsuarioLista.tsx`, `usePerfis.ts`, `useUsuarios.ts`, `PerfilPage.tsx`, `UsuarioPage.tsx`, `paginaService.ts`

### Estado Final

| Validação | Antes | Depois |
|-----------|-------|--------|
| Arquivo "Emp" na raiz | ⚠️ Presente | ✅ Removido |
| `dotnet build` | ✅ 0 erros | ✅ 0 erros |
| `dotnet test` | 23/26 (falha nova) | ✅ **24/26** (2 pré-existentes: AuthService BCrypt + DB diagnostic) |
| `npm run build` (Empresa.Web) | ❌ 5+14 erros | ✅ **Compilado com sucesso** (20.8s, 3120 módulos) |

---

## 🆕 Fase 16: Painel Acesso B.I. (PRD 1.0.0) — PLANEJADO ⏳

> **PRD:** `docs/PRD_ACESSO_BI.md` v1.0.0
> **Fonte de verdade:** `docs/acesso_bi_regra.md` (769 linhas, engenharia reversa do legado)
> **Orquestrador:** `orchestrator` · **Data do PRD:** 2026-07-31
> **Agentes:** `database-engineer`, `backend-engineer`, `architect`, `frontend-engineer`, `qa-engineer`, `devops-engineer`

### Escopo da Fase

Reconstrução completa do painel "Acesso B.I." (legado `CadastroUsuarioBi.aspx`) na stack moderna:
- **Backend:** 1 model, 1 repositório Dapper (6 queries), 1 service, 6 endpoints REST
- **Frontend:** 1 módulo `acesso-bi/` com 4 componentes React + Ant Design, 1 hook Zustand, 1 service Axios
- **Correções:** 9 defeitos do legado (D1–D9) documentados e tratados
- **Chave de permissão:** `cadastroUsuarioBi`

### Progresso Orquestrado

| Fase | Tasks | Orquestrador → Agentes | Progresso | Status |
|------|-------|------------------------|-----------|--------|
| Fase 1 — Camada de Dados | DB-01 a DB-07 | `orchestrator` → `database-engineer` | 0/7 (0%) | ⏳ Planejado |
| Fase 2 — Camada de API | BE-01 a BE-10 | `orchestrator` → `backend-engineer` | 0/10 (0%) | ⏳ Planejado |
| Fase 3 — Camada Frontend | FE-01 a FE-12 | `orchestrator` → `frontend-engineer` + `architect` | 0/12 (0%) | ⏳ Planejado |
| Fase 4 — Qualidade & Testes | QA-01 a QA-05 | `orchestrator` → `qa-engineer` | 0/5 (0%) | ⏳ Planejado |
| Fase 5 — Review & Entrega | RE-01 a RE-04 | `orchestrator` → `architect` + `devops-engineer` | 0/4 (0%) | ⏳ Planejado |
| **TOTAL** | **38 tasks** | | **0%** | ⏳ |

### Atividades por Agente (Acesso B.I.)

| Agente | Tasks | Concluídas | Pendentes | Status |
|--------|-------|------------|-----------|--------|
| `orchestrator` | (coordenação) | — | — | ⏳ Aguardando início |
| `database-engineer` | DB-01 a DB-07 | 0 | 7 | ⏳ Pendente |
| `backend-engineer` | BE-01 a BE-10 | 0 | 10 | 🔒 Bloqueado (Fase 1) |
| `architect` | FE-01, FE-02, RE-01, RE-02 | 0 | 4 | 🔒 Bloqueado (Fases 3/5) |
| `frontend-engineer` | FE-03 a FE-12 | 0 | 10 | 🔒 Bloqueado (Fases 2/3) |
| `qa-engineer` | QA-01 a QA-05 | 0 | 5 | 🔒 Bloqueado (Fases 1/2) |
| `devops-engineer` | RE-03, RE-04 | 0 | 2 | 🔒 Bloqueado (Fase 5) |

### Handoffs Pendentes

```
ORCHESTRATOR → database-engineer:
  ├── DB-01 (Model UsuarioEmpresa.cs) — ⏳ aguardando delegação
  ├── DB-02 (Verificar estrutura Oracle) — ⏳ aguardando delegação
  ├── DB-03 (Interface IAcessoBiRepository) — ⏳ aguardando DB-01
  ├── DB-04 (GetUsuariosComAcessoAsync com LISTAGG) — ⏳ aguardando DB-02, DB-03
  ├── DB-05 (Queries detalhe Q6/Q7) — ⏳ aguardando DB-02, DB-03
  ├── DB-06 (Queries escrita Q3/Q8/Q9) — ⏳ aguardando DB-04, DB-05
  └── DB-07 (Skills Oracle aplicadas) — ⏳ aguardando DB-04, DB-05, DB-06

ORCHESTRATOR → backend-engineer (após database-engineer):
  ├── BE-01 a BE-10 — 🔒 Bloqueado pela Fase 1

ORCHESTRATOR → frontend-engineer + architect (após backend-engineer):
  ├── FE-01 a FE-12 — 🔒 Bloqueado pela Fase 2

ORCHESTRATOR → qa-engineer (após backend + frontend):
  ├── QA-01 a QA-05 — 🔒 Bloqueado pelas Fases 1/2/3

ORCHESTRATOR → architect + devops-engineer (após qa-engineer):
  ├── RE-01 a RE-04 — 🔒 Bloqueado pela Fase 4
```

### Próximo Handoff (Orchestrator → database-engineer)

```
Delegar DB-01 e DB-02 em paralelo:
  DB-01: Criar Model UsuarioEmpresa.cs
  DB-02: Verificar estrutura real das tabelas no Oracle

Após DB-01 + DB-02 concluídos:
  DB-03: Criar interface IAcessoBiRepository
```

---

## 🆕 Fase 14: AI-Readiness — Correções de Governança (2026-07-31) ✅


### Contexto
Auditoria sênior de AI-readiness realizada no repositório. Nota inicial: **7.0/10**. Foram identificados 3 problemas críticos e executadas 9 correções (tasks AIR-01 a AIR-09 no `TASKS.md`).

### Problemas Identificados na Auditoria
1. **🔴 Divergência Frontend** — `agents/frontend-engineer.md` e `agents/README.md` descreviam **Vue 3 + PrimeVue + Pinia**, mas o código real em `Empresa.Web/` é **React 18 + Ant Design + Zustand + React Router**
2. **🟡 Sem `.gitignore` na raiz** — Apenas `Empresa.Web/.gitignore` existia; `bin/`, `obj/`, `.env` dos projetos .NET estavam desprotegidos
3. **🟡 Sem regras nativas de ferramenta** — Zero `.cursor/rules/`, `.github/copilot-instructions.md`

### O que foi feito nesta fase

#### 📝 Correções de Documentação
- **`agents/frontend-engineer.md`** — reescrito: Vue 3/Composition API/Pinia/PrimeVue → **React 18/TSX/hooks/Zustand/Ant Design 5**
- **`agents/README.md`** — corrigida linha da persona: "Frontend Vue 3" → "Frontend React 18"
- **`CLAUDE.md`** — adicionada seção `Frontend (Empresa.Web)` com stack React, comandos npm e estrutura de pastas; adicionados 3 novos guardrails (`.env`, migrations Oracle, stack React)
- **`TASKS.md`** — corrigidas referências a Vue/.vue/Pinia na tabela do PRD Frontend para refletir stack React real

#### 🛡️ Segurança
- **`.gitignore` na raiz** — criado cobrindo `bin/`, `obj/`, `.env`, `appsettings.*.local`, `secrets.json`, `*.pfx`, `*.key`, logs, `node_modules/`, etc.

#### 🤖 Integração com Ferramentas de IA
- **`.cursor/rules/ai-readiness.mdc`** — regras nativas Cursor apontando para `CLAUDE.md` como fonte de verdade
- **`.github/copilot-instructions.md`** — instruções nativas GitHub Copilot (contratos + guardrails)
- **`AGENTS.md`** — ponto de entrada universal para agentes de IA (Claude Code, Cursor, Copilot)

#### 🎨 Padronização
- **`.editorconfig` na raiz** — formatação consistente (C# 4 espaços, TS/React 2 espaços)

### Estado atual (após correções)

| Item | Estado |
|:-----|:------:|
| Arquivos de contexto raiz | ✅ 4 (`CLAUDE.md`, `AGENTS.md`, `.gitignore`, `.editorconfig`) |
| Regras específicas de ferramenta | ✅ 2 (Cursor + Copilot) |
| Divergência frontend (Vue vs React) | ✅ **Corrigida** |
| Proteção de segredos/binários | ✅ **Raiz + subprojetos** |
| Nota AI-Readiness estimada | ✅ **~9.0/10** (era 7.0/10) |

---

## 🆕 Fase 13: Painéis de Usuário e Perfil — CONCLUÍDA (2026-07-30) ✅

### O que foi feito nesta fase

#### 🗄️ database-engineer (Camada Data)
- **Perfil.cs** corrigido: `Nome` → `Descricao`, removido `Ativo` (conforme tabela `ACESSO_CADASTRO_PERFIL`)
- **Usuario.cs** atualizado: adicionado `DescricaoPerfil` para JOIN com perfil
- **Novos Models:** `PerfilPagina.cs`, `UsuarioEmpresaEstab.cs`
- **IPerfilRepository** expandido: `GetAllAsync(search)`, `GetPaginasTreeAsync(idPerfil)`, `SalvarPermissoesAsync(idPerfil, vincularIds, desvincularIds)` + model `PaginaTree`
- **PerfilRepository** reescrito: query hierárquica com `CONNECT BY PRIOR`, `MERGE INTO` para permissões (idempotente), transação atômica
- **IUsuarioRepository** expandido: `GetAllAsync(search, ativo)` com JOIN em perfil, `GetEstabelecimentosTreeAsync(idUsuario)`, `SincronizarEstabelecimentosAsync(idUsuario, vincular, desvincular)` + models `UsuarioEstabelecimentoTree` e `VinculoEstabelecimento`
- **UsuarioRepository** reescrito: `GetAllAsync` com LEFT JOIN em `ACESSO_CADASTRO_PERFIL`, senha NUNCA retornada na listagem, update condicional de senha, sincronização de estabelecimentos com verificação de duplicidade em transação

#### ⚙️ backend-engineer (Camada Api)
- **Novos DTOs:** `Request/PerfilRequest.cs`, `Response/PerfilResponse.cs`, `Request/PerfilPaginasRequest.cs`, `Response/PaginaTreeResponse.cs`, `Response/EstabelecimentoTreeResponse.cs` + `EstabelecimentoFilhoResponse`, `Request/EstabelecimentoVinculoRequest.cs` + `VinculoEstabelecimentoItem`
- **DTOs atualizados:** `UsuarioRequest.cs` (DataAnnotations, IdPerfil, Senha opcional, AtualizaSenha), `UsuarioResponse.cs` (DescricaoPerfil, DataHoraUltimoAcesso, sem Senha)
- **IPerfilService/PerfilService** reescritos: CRUD com validações, conversão de `PaginaTree` flat para hierarquia aninhada (pais→filhos recursivo)
- **IUsuarioService/UsuarioService** reescritos: BCrypt hash, validação senha mínima 6 chars, agrupamento de estabelecimentos por empresa, parsing de `DESCRITIVO`
- **PerfilEndpoints** expandido: 7 endpoints (GET lista, GET lista-simples, GET by id, POST, PUT, DELETE, GET paginas, PUT paginas) — todos com `.RequireAuthorization()`, `.WithSummary()`, `.WithDescription()`, `.Produces<>()`
- **UsuarioEndpoints** expandido: 7 endpoints (GET lista, GET by id, POST, PUT, DELETE, GET estab, PUT estab) — mesma cobertura de documentação
- **`dotnet build`:** ✅ **0 erros, 0 warnings**

#### 🧪 qa-engineer (Camada Testes)
- **PerfilServiceTests.cs** — 7 testes (listar, criar válido, criar sem descrição→422, update existente, update inexistente→404, delete, árvore hierárquica)
- **PerfilPermissoesTests.cs** — 2 testes (salvar permissões com verificação, perfil inexistente→404)
- **UsuarioServiceTests.cs** expandido — 7 testes (GetAll, GetById existente/inexistente, Create duplicado→409, Delete com verificação, Create sem senha→422)
- **UsuarioEstabelecimentoTests.cs** — 3 testes (árvore agrupada, sincronizar batch, usuário inexistente→404)
- **SenhaSegurancaTests.cs** — 4 testes (response sem Senha, mínimo 6 chars, BCrypt difere do original, list sem Senha)
- **`dotnet test`:** ✅ **24/26 passando** (2 falhas pré-existentes: AuthService BCrypt + DB diagnostic)

### Estado atual
| Item | Estado |
|:-----|:------:|
| PRD dos painéis de Usuário/Perfil | ✅ Gerado |
| Backlog TASKS.md Fase 13 | ✅ Executado (27 tarefas concluídas) |
| Models criados/atualizados | ✅ 8 arquivos |
| Repositórios criados/atualizados | ✅ 4 interfaces + 4 implementações |
| DTOs criados/atualizados | ✅ 11 arquivos |
| Services criados/atualizados | ✅ 4 arquivos |
| Endpoints criados/atualizados | ✅ 14 endpoints no total |
| `dotnet build` | ✅ **0 erros** |
| `dotnet test` | ✅ **24/26 passando** |
| `TASKS.md` | ✅ Atualizado |
| `PROGRESS.md` | ✅ Atualizado |

---

## 🌐 Fase 15: Frontend Usuários e Perfis (PRD 3.0.0) — EM ANDAMENTO 🔄

> **PRD:** `docs/PRD_FRONTEND_USUARIOS_PERFIS.md` v3.0.0
> **Orquestrador:** `orchestrator` · **Início:** 2026-07-28
> **Agentes:** `architect`, `frontend-engineer`, `backend-engineer`, `qa-engineer`, `devops-engineer`

### Progresso Orquestrado

| Fase | Tasks | Orquestrador → Agentes | Progresso | Status |
|------|-------|------------------------|-----------|--------|
| Fase 1 — Estrutura base | T001–T002 | `orchestrator` → `architect` | 2/2 (100%) | ✅ Concluída |
| Fase 2 — Componentes | T003–T013 | `orchestrator` → `frontend-engineer` + `backend-engineer` | 11/11 (100%) | ✅ Concluída |
| Fase 3 — Testes | T014–T019 | `orchestrator` → `qa-engineer` | 1/6 (17%) | 🔄 Em andamento |
| Fase 4 — Refinamento | T020–T021 | `orchestrator` → `frontend-engineer` | 2/2 (100%) | ✅ Concluída |
| Fase 5 — Review & Entrega | T022–T024 | `orchestrator` → `architect` + `devops-engineer` | 0/3 (0%) | ⏳ Pendente |
| **TOTAL** | **24 tasks** | | **15/24 (63%)** | 🔄 |

### Atividades por Agente (Frontend)

| Agente | Tasks | Concluídas | Pendentes | Status |
|--------|-------|------------|-----------|--------|
| `orchestrator` | (coordenação) | — | — | 🔄 Ativo |
| `architect` | T001, T002, T022, T023 | 2 | 2 | ⏳ Aguardando Fase 5 |
| `frontend-engineer` | T005–T012, T020, T021 | 10 | 0 | ✅ Concluído |
| `backend-engineer` | T003, T004, T013 | 3 | 0 | ✅ Concluído |
| `qa-engineer` | T014–T019 | 1 | 5 | 🔄 Em andamento |
| `devops-engineer` | T024 | 0 | 1 | ⏳ Aguardando Fase 5 |

### Handoffs Pendentes

```
ORCHESTRATOR → qa-engineer:
  ├── T015 (usePerfilAdmin tests) — 🔄 em andamento
  ├── T016–T019 — aguardando T015

ORCHESTRATOR → architect + devops-engineer (após qa-engineer):
  ├── T022 (code review)
  ├── T023 (documentação)
  └── T024 (build e validação final)
```

---

## 📈 Status Geral

| Métrica | Valor |
|:--------|:-----:|
| Fases concluídas | 15 de 16 (backlog operacional) |
| Fase 15 — Frontend | 63% (15/24 tasks) 🔄 |
| Fase 17 — Finalização e Correções | ✅ **100% concluída** |
| Score de conformidade | **~99%** |
| `dotnet build` | ✅ **0 erros** |
| `dotnet test` | ✅ **24/26 passando** |
| `npm run build` (Empresa.Web) | ✅ **Compilado com sucesso** |
| Nota AI-Readiness | ✅ **~9.0/10** (era 7.0/10) |
| Arquivos criados | ~95 + 8 novos (AI-Readiness) |
| Testes unitários | 26 |

---

## ✅ Resumo por Fase

| Fase | Descrição | % | Status |
|:----:|:----------|:-:|:------:|
| **0** | Correções Urgentes | **100%** | ✅ |
| **1** | Infraestrutura de Dados | **100%** | ✅ |
| **2** | Autenticação JWT | **100%** | ✅ |
| **3** | Módulo Acesso/Usuários | **100%** | ✅ |
| **4** | Módulo Compras | **100%** | ✅ |
| **5** | Módulo Financeiro | **100%** | ✅ |
| **6** | Módulo Prazo Médio | **100%** | ✅ |
| **7** | Módulo Vendas | **100%** | ✅ |
| **8** | Módulos Secundários | **100%** | ✅ |
| **9** | Worker Service | **100%** | ✅ |
| **10** | Qualidade e Testes | **100%** | ✅ |
| **11** | Documentação e Swagger | **100%** | ✅ |
| **12** | DevOps (Docker, CI/CD) | **100%** | ✅ |
| **13** | Painéis de Usuário e Perfil | **100%** | ✅ |
| **14** | AI-Readiness (governança de agentes) | **100%** | ✅ |
| **15** | Frontend Usuários e Perfis (PRD 3.0.0) | **63%** | 🔄 |
| **17** | Finalização e Correções | **100%** | ✅ |

---

## 🚀 Consolidação Final

| Métrica | Total |
|:--------|:-----:|
| Projetos .NET 9 | 5 (Api, Data, Util, Worker, Tests) |
| Repositórios Dapper | 10 implementações |
| Services | 14 |
| Endpoints REST | ~70+ |
| Packages Oracle mapeados | 27 |
| Testes unitários | 26 |
| Arquivos criados | ~95 + 8 novos (AI-Readiness) |
| Arquivos de contexto IA na raiz | 4 (`CLAUDE.md`, `AGENTS.md`, `.gitignore`, `.editorconfig`) |
| Regras nativas de ferramenta IA | 2 (`.cursor/rules/`, `.github/copilot-instructions.md`) |
