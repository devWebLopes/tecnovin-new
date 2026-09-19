# PRD — Modernização TreisTecnovin: GestaoNew
## Versão: 1.0 | Data: 28/07/2026 | Status: ATIVO

---

### 1. Resumo Executivo

#### O que foi encontrado (estado atual)

O projeto GestaoNew encontra-se em estado embrionário. A estrutura da solution com 4 projetos (`Empresa.Api`, `Empresa.Data`, `Empresa.Util`, `Empresa.Worker`) foi criada, mas apenas o scaffold inicial de templates .NET 9 existe. Não há código de negócio implementado.

**Estado real dos artefatos:**

| Projeto | Status | Descrição |
|---------|--------|-----------|
| `Empresa.Api` | 🔴 Template | `Program.cs` é o template WeatherForecast padrão do `dotnet new webapi`. Apenas `Microsoft.AspNetCore.OpenApi` instalado. |
| `Empresa.Data` | 🟡 Parcial | Possui Dapper 2.1.79 e Oracle.ManagedDataAccess.Core 23.26.300 instalados. Apenas 2 modelos existem: `Usuario.cs` e `Pagina.cs`. `Class1.cs` placeholder ainda presente. Sem referência ao `Empresa.Util`. |
| `Empresa.Util` | 🔴 Template | `Class1.cs` placeholder presente. Possui `Dados.cs`, `Email.cs`, `StringUtils.cs` criados mas com conteúdo mínimo. Sem referência ao `Empresa.Data`. |
| `Empresa.Worker` | 🔴 Template | Worker template com loop `Task.Delay(1000)` fazendo log. Nenhuma lógica de negócio. Referencia `Empresa.Data` e `Empresa.Util`. |
| `Empresa.Tests` | ❌ Inexistente | Projeto de testes não foi criado. |

#### Lacunas críticas identificadas

1. **Program.cs é template WeatherForecast** — precisa ser completamente reescrito com DI, Serilog, Swagger, CORS, JWT, health checks
2. **`appsettings.json` sem connection string Oracle** — não há configuração de banco
3. **`Class1.cs` placeholders** existem em `Empresa.Data` e `Empresa.Util`
4. **Apenas 2 de ~30 modelos** implementados (Usuario, Pagina). Faltam: Perfil, Estabelecimento e todas as entidades de negócio
5. **Zero repositórios** implementados — equivalente a 0 de 6 DAOs do legado mapeados
6. **Zero endpoints** de negócio — equivalente a 0 de 43+ endpoints REST mapeados
7. **Nenhum pacote de segurança** instalado (JWT, BCrypt)
8. **Nenhum middleware** implementado (ExceptionHandling, RequestLogging)
9. **Worker não tem lógica** — apenas scaffold com loop vazio
10. **Projeto de testes ausente**

#### Riscos de negócio

| Risco | Impacto | Probabilidade |
|-------|---------|---------------|
| Prazo insuficiente para 43+ endpoints com regras de negócio complexas | Alto | Alta |
| Complexidade dos 3 níveis de drill-down (DRE, Prazo Médio) subestimada | Alto | Média |
| Múltiplas versões de packages Oracle (3 DRE, 3 Posição Financeira) com comportamentos distintos | Médio | Alta |
| SQL injection no legado — migração incorreta pode reintroduzir vulnerabilidades | Crítico | Média |
| Worker Service sem acesso ao código legado do `srvPrincipal.cs` para mapear tarefas | Médio | Alta |

#### Esforço estimado total

| Fase | Descrição | Esforço Estimado |
|------|-----------|:---:|
| Fase 0 | Correções Urgentes | 4h |
| Fase 1 | Infraestrutura de Dados | 13h |
| Fase 2 | Autenticação JWT | 11h |
| Fase 3 | Módulo Acesso/Usuários | 21h |
| Fase 4 | Módulo Compras | 24h |
| Fase 5 | Módulo Financeiro | 34h |
| Fase 6 | Módulo Prazo Médio | 12h |
| Fase 7 | Módulo Vendas | 13h |
| Fase 8 | Módulos Secundários | 16h |
| Fase 9 | Worker Service | 9h |
| Fase 10 | Qualidade e Testes | 18h |
| Fase 11 | Documentação e Swagger | 12h |
| Fase 12 | DevOps e Infraestrutura | 12h |
| **TOTAL** | | **~199h** (~5 semanas) |

---

### 2. Score de Conformidade Atual

#### 2.1. Estrutura Base (20 itens)

| # | Item | Status |
|---|------|--------|
| 1 | `Program.cs` configurado com DI, Serilog, Swagger, CORS, JWT middleware | ❌ Ausente |
| 2 | Connection string Oracle em `appsettings.json` | ❌ Ausente |
| 3 | Health check `/api/health` configurado | ❌ Ausente |
| 4 | `Class1.cs` removidos de todos os projetos | ❌ Ausente (2 placeholders) |
| 5 | Referências entre projetos corretas | ⚠️ Parcial (Data não referencia Util; Data não tem ref. do Util) |
| 6 | `DbSession.cs` — gerenciamento de conexão Oracle (scoped) | ❌ Ausente |
| 7 | `OracleConnectionFactory.cs` | ❌ Ausente |
| 8 | `OracleProcedures.cs` — constantes de packages/procedures | ❌ Ausente |
| 9 | Pasta `Repositories/` com interfaces e implementações | ❌ Ausente |
| 10 | `IAcessoRepository.cs` / `AcessoRepository.cs` | ❌ Ausente |
| 11 | `IComprasRepository.cs` / `ComprasRepository.cs` | ❌ Ausente |
| 12 | `IVendasRepository.cs` / `VendasRepository.cs` | ❌ Ausente |
| 13 | `IFinanceiroRepository.cs` / `FinanceiroRepository.cs` | ❌ Ausente |
| 14 | `IPrazoMedioRepository.cs` / `PrazoMedioRepository.cs` | ❌ Ausente |
| 15 | `IAgricolaRepository.cs` / `AgricolaRepository.cs` | ❌ Ausente |
| 16 | `ICalendarioRepository.cs` / `CalendarioRepository.cs` | ❌ Ausente |
| 17 | `IDbaRepository.cs` / `DbaRepository.cs` | ❌ Ausente |
| 18 | `IAgrupamentoRepository.cs` / `AgrupamentoRepository.cs` | ❌ Ausente |
| 19 | Controllers/Endpoints com Minimal APIs | ❌ Ausente |
| 20 | DTOs/ para Request e Response | ❌ Ausente |

**Estrutura: 1/20 (5%)**

#### 2.2. Modelos (12 itens)

| # | Modelo | Status |
|---|--------|--------|
| 1 | `Usuario.cs` | ✅ Implementado |
| 2 | `Pagina.cs` | ✅ Implementado |
| 3 | `Perfil.cs` | ❌ Ausente |
| 4 | `Estabelecimento.cs` | ❌ Ausente |
| 5 | `ComiteCompras.cs` / `ComiteComprasItem.cs` | ❌ Ausente |
| 6 | `ResumoAnualCompras.cs` | ❌ Ausente |
| 7 | `FluxoCaixa.cs` | ❌ Ausente |
| 8 | `PosicaoFinanceira.cs` | ❌ Ausente |
| 9 | `Dre.cs` | ❌ Ausente |
| 10 | `PrazoMedio.cs` | ❌ Ausente |
| 11 | `AnaliseVendas.cs` / `RankingClientes.cs` | ❌ Ausente |
| 12 | `CalendarioFinanceiro.cs` | ❌ Ausente |

**Modelos: 2/12 (17%)**

#### 2.3. Regras de Negócio — Autenticação e Segurança (7 itens)

| # | Regra | Status |
|---|-------|--------|
| 1 | Login valida `ATIVO = 'S'` | ❌ Não implementado |
| 2 | Campo `ATUALIZA_SENHA` tratado no login | ❌ Não implementado |
| 3 | `SalvaAcessoUsuario()` — incrementa `QUANTIDADE_ACESSO` | ❌ Não implementado |
| 4 | Menu hierárquico com UNION (filhas + pais) | ❌ Não implementado |
| 5 | TOP 10 páginas mais acessadas | ❌ Não implementado |
| 6 | UPSERT em `ACESSO_VISUALIZACAO_PAGINA` | ❌ Não implementado |
| 7 | Registro de relatórios em `REGISTRO_RELATORIOS` | ❌ Não implementado |

**Regras Autenticação: 0/7 (0%)**

#### 2.4. Regras de Negócio — Compras (5 itens)

| # | Regra | Status |
|---|-------|--------|
| 1 | `sp_realizado` — 4 cursores mapeados | ❌ Não implementado |
| 2 | `sp_previsto_realizado_item` | ❌ Não implementado |
| 3 | `sp_progressao_preco` | ❌ Não implementado |
| 4 | Gestão de CFOP (transferência + exceção + log) | ❌ Não implementado |
| 5 | Centro de custo (agrup_conta_ccusto + detalhe + det_prod) | ❌ Não implementado |

**Regras Compras: 0/5 (0%)**

#### 2.5. Regras de Negócio — Financeiro/DRE (10 itens)

| # | Regra | Status |
|---|-------|--------|
| 1 | 3 versões de `sp_posicao` (legado, new, sreal) | ❌ Não implementado |
| 2 | Posição Semanal com remoção de colunas vazias | ❌ Não implementado |
| 3 | Fluxo de caixa analítico — remoção de colunas PORT/PORTADOR/CLIENTE | ❌ Não implementado |
| 4 | `sp_resumo_geral_flxcxa` — totais fluxo | ❌ Não implementado |
| 5 | 3 versões de DRE (padrão, homologado, out) | ❌ Não implementado |
| 6 | Drill-down DRE: conta → prev → documento (2 cursores) | ❌ Não implementado |
| 7 | 3 níveis drill-down PZM recebimento | ❌ Não implementado |
| 8 | 3 níveis drill-down PZM pagamento | ❌ Não implementado |
| 9 | 7 tipos de operação PZM Pagamento | ❌ Não implementado |
| 10 | Calendário financeiro com lógica de cores | ❌ Não implementado |

**Regras Financeiro: 0/10 (0%)**

#### 2.6. Arquitetura .NET 9 (14 itens)

| # | Item | Status |
|---|------|--------|
| 1 | Separação clara Api → Data → Util | ⚠️ Parcial (Data não referencia Util) |
| 2 | Sem referências circulares | ✅ OK |
| 3 | Minimal APIs ou Controllers consistentemente | ❌ Ausente |
| 4 | `IResult` retornado em todos os endpoints | ❌ Ausente |
| 5 | Global exception middleware | ❌ Ausente |
| 6 | Request logging middleware | ❌ Ausente |
| 7 | Swagger/OpenAPI documentado | ⚠️ Parcial (apenas `/openapi/v1.json`) |
| 8 | Apenas `Microsoft.Extensions.DependencyInjection` | ✅ OK |
| 9 | Repositórios registrados como Scoped | ❌ Ausente |
| 10 | DbSession Scoped | ❌ Ausente |
| 11 | Todas operações I/O são `async Task` | ❌ Ausente (não há código de I/O) |
| 12 | JWT Bearer configurado | ❌ Ausente |
| 13 | Parâmetros Dapper em todas as queries | ❌ Ausente |
| 14 | Models vs DTOs separados | ❌ Ausente |

**Arquitetura: 3/14 (21%)**

#### 2.7. Qualidade e Testes (3 itens)

| # | Item | Status |
|---|------|--------|
| 1 | Projeto `Empresa.Tests` existe | ❌ Ausente |
| 2 | `dotnet build` sem erros | ❓ Não verificado (provável que passe, pois é template) |
| 3 | Testes passando | ❌ Ausente |

**Qualidade: 0/3 (0%)**

#### Score Consolidado

| Categoria | Total Itens | Implementados | % |
|-----------|:-----------:|:-------------:|:--:|
| Estrutura | 20 | 1 | 5% |
| Modelos | 12 | 2 | 17% |
| Regras Autenticação | 7 | 0 | 0% |
| Regras Compras | 5 | 0 | 0% |
| Regras Financeiro/DRE | 10 | 0 | 0% |
| Arquitetura .NET 9 | 14 | 3 | 21% |
| Qualidade/Testes | 3 | 0 | 0% |
| **TOTAL** | **71** | **6** | **8,5%** |

---

### 3. Gap Analysis — O que está faltando

#### Lacunas Críticas (P0 — Bloqueiam qualquer progresso)

| ID | Lacuna | Impacto | Agente |
|----|--------|---------|--------|
| G01 | `Program.cs` é template WeatherForecast — sem DI, sem middleware, sem configuração | **CRÍTICO** — API não funcional | Backend Engineer |
| G02 | `Class1.cs` placeholders em Data e Util | **CRÍTICO** — código morto que polui o build | Backend Engineer |
| G03 | `appsettings.json` sem connection string Oracle | **CRÍTICO** — sem acesso a banco | DevOps Engineer |
| G04 | Ausência de `DbSession.cs` para gerenciamento de conexão | **CRÍTICO** — sem padronização de acesso a dados | Database Engineer |
| G05 | Nenhum pacote de segurança instalado (JWT, BCrypt) | **CRÍTICO** — autenticação impossível | Backend Engineer |
| G06 | Nenhum middleware de exception handling | **CRÍTICO** — erros não tratados expõem stack trace | Backend Engineer |

#### Lacunas Altas (P1 — Bloqueiam módulos de negócio)

| ID | Lacuna | Impacto | Agente |
|----|--------|---------|--------|
| G07 | Zero repositórios implementados (0 de 9 previstos) | **ALTO** — acesso a dados inexistente | Database Engineer |
| G08 | Zero endpoints de negócio (0 de 43+ previstos) | **ALTO** — API sem funcionalidade | Backend Engineer |
| G09 | Apenas 2 de ~30 modelos implementados | **ALTO** — estruturas de dados incompletas | Database Engineer |
| G10 | `OracleProcedures.cs` não criado (constantes de 20+ packages) | **ALTO** — magic strings nas queries | Database Engineer |
| G11 | Sem `Empresa.Data` referenciando `Empresa.Util` | **ALTO** — viola contrato Clean Architecture | Architect |
| G12 | Worker Service sem lógica de negócio | **ALTO** — tarefas agendadas do legado não migradas | Backend Engineer |

#### Lacunas Médias (P2)

| ID | Lacuna | Impacto | Agente |
|----|--------|---------|--------|
| G13 | Zero DTOs criados | **MÉDIO** — exposição de entidades na API | Backend Engineer |
| G14 | Sem Serilog configurado | **MÉDIO** — sem logging estruturado | Backend Engineer |
| G15 | Sem health checks | **MÉDIO** — sem monitoramento de disponibilidade | DevOps Engineer |
| G16 | Projeto de testes ausente | **MÉDIO** — sem cobertura de qualidade | QA Engineer |
| G17 | Sem rate limiting nos endpoints de auth | **MÉDIO** — vulnerável a brute force | Backend Engineer |

#### Lacunas Baixas (P3)

| ID | Lacuna | Impacto | Agente |
|----|--------|---------|--------|
| G18 | Sem versionamento de API (`/api/v1/`) | **BAIXO** — implementar antes do primeiro deploy | Backend Engineer |
| G19 | Sem Dockerfile/docker-compose | **BAIXO** — necessário para CI/CD | DevOps Engineer |
| G20 | Sem pipeline CI/CD | **BAIXO** — automatização futura | DevOps Engineer |

---

### 4. Roadmap de Implementação (Fases e Etapas)

---

### FASE 0 — CORREÇÕES URGENTES (Agente: Backend Engineer + DevOps Engineer + Architect)

**Critério de saída:** Build limpo, sem placeholders, DI configurado corretamente.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P0-T01 | Remover `Class1.cs` de `Empresa.Data` e `Empresa.Util` | Backend Engineer | - | `dotnet build` sem warnings de classe não utilizada | 15min | P0 |
| P0-T02 | Adicionar referência de `Empresa.Data` para `Empresa.Util` no `.csproj` | Architect | - | Referência aparece no Solution Explorer | 5min | P0 |
| P0-T03 | Reescrever `Program.cs` completo: DI, Serilog, Swagger, CORS, JWT | Backend Engineer | - | `dotnet build` + GET /swagger carrega | 2h | P0 |
| P0-T04 | Configurar `appsettings.json` com connection string Oracle real (via User Secrets no dev) | DevOps Engineer | - | App sobe sem erro de configuração | 30min | P0 |
| P0-T05 | Instalar NuGet packages obrigatórios em todos os projetos | Backend Engineer | - | `dotnet restore` sem erros | 30min | P0 |
| P0-T06 | Criar `appsettings.Development.json` com User Secrets placeholder | DevOps Engineer | P0-T04 | Documentação de como configurar secrets | 15min | P0 |
| P0-T07 | Validar que `dotnet build` passa sem erros e sem warnings | QA Engineer | P0-T01..06 | Zero erros e zero warnings | 15min | P0 |
| P0-T08 | Atualizar `TASKS.md` e `PROGRESS.md` com status desta fase | Architect | P0-T07 | Arquivos de controle atualizados | 15min | P0 |

---

### FASE 1 — INFRAESTRUTURA DE DADOS (Agente: Database Engineer)

**Critério de saída:** Conexão Oracle validada, repositório de Acesso funcionando com Dapper.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P1-T01 | Criar `Empresa.Data/DbSession.cs` — gerenciamento de conexão Oracle (scoped, com `IDisposable` e `await using`) | Database Engineer | P0-T04 | Conexão Oracle abre e fecha sem memory leak | 1h | P0 |
| P1-T02 | Criar `Empresa.Data/Oracle/OracleConnectionFactory.cs` — cria `IDbConnection` a partir de config | Database Engineer | P1-T01 | Retorna conexão válida | 30min | P0 |
| P1-T03 | Criar `Empresa.Data/Oracle/OracleProcedures.cs` — constantes de todas as packages/procedures do legado | Database Engineer | - | Todas as 20+ packages do doc_legado mapeadas como constantes | 2h | P0 |
| P1-T04 | Criar entidades em `Empresa.Data/Models/`: Perfil, Estabelecimento | Database Engineer | - | Classes com todos os campos do Oracle mapeados | 1h | P0 |
| P1-T05 | Criar `Empresa.Data/Repositories/IAcessoRepository.cs` | Database Engineer | - | Interface com assinaturas completas | 30min | P0 |
| P1-T06 | Criar `Empresa.Data/Repositories/AcessoRepository.cs` — GetUsuario (login+senha), GetPaginas (com UNION), SalvaAcessoUsuario, GetNode/GetTree | Database Engineer | P1-T01..05 | Método GetUsuario retorna usuário via Oracle | 4h | P0 |
| P1-T07 | Criar health check endpoint `/api/health` e `/api/health/database` validando conexão Oracle | Backend Engineer | P1-T01 | GET /api/health/database retorna 200 com versão Oracle | 1h | P0 |
| P1-T08 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P1-T07 | Arquivos de controle atualizados | 15min | P0 |

**Pacotes NuGet a instalar no P0-T05:**
- `Empresa.Api`: `Oracle.ManagedDataAccess.Core`, `Dapper`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Serilog.AspNetCore`, `Swashbuckle.AspNetCore`, `BCrypt.Net-Next`
- `Empresa.Data`: Já tem Dapper e Oracle (OK)
- `Empresa.Util`: Já tem `Microsoft.Extensions.Configuration` (OK)

---

### FASE 2 — AUTENTICAÇÃO JWT (P0 — BLOQUEANTE) (Agente: Backend Engineer)

**Critério de saída:** Login retornando JWT válido, middleware protegendo todos os endpoints.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P2-T01 | Configurar JWT em `appsettings.json`: Secret, Issuer, Audience, ExpiryMinutes | Backend Engineer | P0-T03 | Config carregada sem erro | 30min | P0 |
| P2-T02 | Criar `Empresa.Api/DTOs/LoginRequest.cs` e `LoginResponse.cs` | Backend Engineer | - | DTOs sem referência a entidades de banco | 30min | P0 |
| P2-T03 | Criar `Empresa.Api/Services/IAuthService.cs` e `AuthService.cs` — login + geração de JWT com claims (ID_USUARIO, NOME, LOGIN, ID_PERFIL) | Backend Engineer | P1-T06 | Token gerado com claims corretos | 3h | P0 |
| P2-T04 | Criar `Empresa.Api/Endpoints/AuthEndpoints.cs` — POST /api/v1/auth/login, POST /api/v1/auth/refresh, POST /api/v1/auth/alterar-senha | Backend Engineer | P2-T02..03 | POST /api/v1/auth/login retorna JWT | 3h | P0 |
| P2-T05 | Configurar middleware JWT em `Program.cs` e proteger todos os endpoints (exceto /auth/login e /health) | Backend Engineer | P2-T04 | Endpoint protegido retorna 401 sem token | 1h | P0 |
| P2-T06 | Implementar hash de senha (BCrypt) — validar no login, nunca comparar texto plano | Backend Engineer | P2-T03 | Login com senha hasheada funciona corretamente | 1h | P0 |
| P2-T07 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P2-T06 | Arquivos de controle atualizados | 15min | P0 |

---

### FASE 3 — MÓDULO ACESSO/USUÁRIOS (P0 — BLOQUEANTE) (Agente: Backend Engineer + Database Engineer)

**Critério de saída:** CRUD completo de Usuários, Perfis e Menu dinâmico funcionando.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P3-T01 | Criar `IUsuarioRepository.cs` / `UsuarioRepository.cs` — CRUD, exclusão lógica (ATIVO='N') | Database Engineer | P1-T01..04 | GET /api/v1/usuarios retorna lista | 3h | P0 |
| P3-T02 | Criar `IPaginaRepository.cs` / `PaginaRepository.cs` — GetMenu (com hierarquia UNION), GetRecentes (TOP 10), RegistraAcesso (UPSERT) | Database Engineer | P1-T01..04 | GET /api/v1/paginas/menu retorna árvore correta | 4h | P0 |
| P3-T03 | Criar `IPerfilRepository.cs` / `PerfilRepository.cs` — CRUD perfis e vinculação com páginas | Database Engineer | P1-T01..04 | GET /api/v1/perfis retorna perfis | 2h | P0 |
| P3-T04 | Criar `IEstabelecimentoRepository.cs` / `EstabelecimentoRepository.cs` — GetNode, GetTree, Registra, Exclui | Database Engineer | P1-T01..04 | GET /api/v1/estabelecimentos retorna árvore | 2h | P0 |
| P3-T05 | Criar DTOs: UsuarioRequest, UsuarioResponse, PaginaResponse, PerfilResponse, EstabelecimentoResponse | Backend Engineer | - | DTOs sem entidades de banco expostas | 2h | P0 |
| P3-T06 | Criar Services: IUsuarioService/UsuarioService, IPaginaService/PaginaService, IPerfilService/PerfilService | Backend Engineer | P3-T01..05 | Lógica de negócio isolada dos repositórios | 2h | P0 |
| P3-T07 | Criar Endpoints: UsuarioEndpoints, PaginaEndpoints, PerfilEndpoints, EstabelecimentoEndpoints | Backend Engineer | P3-T06 | Todos os 13 endpoints do módulo Acesso (seção 13.4 do doc_legado) implementados | 4h | P0 |
| P3-T08 | Registrar relatórios em `REGISTRO_RELATORIOS` — chamada na geração de relatórios | Backend Engineer | P3-T07 | INSERT ocorre ao gerar qualquer relatório | 1h | P0 |
| P3-T09 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P3-T08 | Arquivos de controle atualizados | 15min | P0 |

---

### FASE 4 — MÓDULO COMPRAS (P1) (Agente: Database Engineer + Backend Engineer)

**Critério de saída:** Todos os endpoints de compras implementados e testados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P4-T01 | Criar entidades de Compras em `Models/`: ResumoAnualCompras (4 cursores), ComiteComprasNF, ProgressaoPrecoNF, PrevisaoCompra, CfopTransferencia, CentrosCusto | Database Engineer | P1-T01 | Classes tipadas para todos os retornos Oracle | 3h | P1 |
| P4-T02 | Criar `IComprasRepository.cs` / `ComprasRepository.cs` — mapear `DaoCompras` completo (seção 5.4 do doc_legado) | Database Engineer | P4-T01 | Todos os 30+ métodos do DaoCompras mapeados com Dapper | 6h | P1 |
| P4-T03 | Mapear package `packageCompraNf.sp_realizado` com 4 REF CURSORs usando `OracleDynamicParameters` | Database Engineer | P4-T02 | 4 cursores retornados corretamente (resultado, totais, grafico, grafico2) | 3h | P1 |
| P4-T04 | Criar `IComprasService.cs` / `ComprasService.cs` — pós-processamento e orquestração | Backend Engineer | P4-T02 | Lógica de pós-processamento isolada | 2h | P1 |
| P4-T05 | Criar `ComprasEndpoints.cs` — todos os 11 endpoints da seção 13.4 do doc_legado para Compras | Backend Engineer | P4-T04 | Endpoints GET/POST /api/v1/compras/* implementados | 4h | P1 |
| P4-T06 | Implementar CRUD de CFOP (Transferência + Exceção + Log de alterações) | Database Engineer + Backend Engineer | P4-T05 | CRUD CFOP funcional com log de auditoria | 3h | P1 |
| P4-T07 | Implementar Centro de Custo (3 procedures: agrup_conta_ccusto + detalhe + det_prod) | Database Engineer + Backend Engineer | P4-T05 | 3 endpoints de centro de custo funcionando | 2h | P1 |
| P4-T08 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P4-T07 | Arquivos de controle atualizados | 15min | P1 |

---

### FASE 5 — MÓDULO FINANCEIRO (P1) (Agente: Database Engineer + Backend Engineer)

**Critério de saída:** Posição Financeira (3 versões), Fluxo de Caixa e DRE (3 versões) implementados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P5-T01 | Criar entidades de Financeiro: PosicaoFinanceira, FluxoCaixa (master+detalhe), Dre, ProjecaoFinanceira, AjusteFinanceiro | Database Engineer | P1-T01 | Classes tipadas completas | 4h | P1 |
| P5-T02 | Criar `IFinanceiroRepository.cs` / `FinanceiroRepository.cs` — mapear `DaoPainel` completo para módulos financeiros | Database Engineer | P5-T01 | Todos os ~40 métodos de DaoPainel financeiro mapeados | 8h | P1 |
| P5-T03 | Implementar as 3 versões de `sp_posicao` (legado, new, sreal) como métodos separados ou com enum de versão | Database Engineer | P5-T02 | 3 endpoints retornam dados corretos de cada versão | 3h | P1 |
| P5-T04 | Implementar Posição Semanal com pós-processamento (remoção de colunas de semanas vazias) na camada de serviço | Backend Engineer | P5-T02..03 | Colunas sem dados removidas corretamente | 2h | P1 |
| P5-T05 | Implementar Fluxo de Caixa Analítico com pós-processamento (remoção de PORT, PORTADOR, CLIENTE, NOME_CLIENTE) | Backend Engineer | P5-T02 | Colunas corretamente removidas | 2h | P1 |
| P5-T06 | Implementar DRE com 3 packages distintas + drill-down (conta → prev → documento com 2 cursores) | Database Engineer + Backend Engineer | P5-T02 | 3 versões de DRE + drill-down completo | 6h | P1 |
| P5-T07 | Implementar Projeção Financeira e Planejamento (packageProjecaoFinanceira + packagePlanejamentoDre) | Database Engineer + Backend Engineer | P5-T02 | Endpoints de projeção e planejamento funcionando | 3h | P1 |
| P5-T08 | Implementar Ajustes Financeiros (AlteraRegistro, AlteraRegistroMovimento, AlteraStatus, GetAjusteLancamentos) | Database Engineer + Backend Engineer | P5-T02 | CRUD de ajustes funcional | 2h | P1 |
| P5-T09 | Implementar Portador (GetPortadores, GetSaldoPortadorDia, SetSaldoInicial, AtualizaSaldoPeriodo) | Database Engineer + Backend Engineer | P5-T02 | Endpoints de portador funcionando | 2h | P1 |
| P5-T10 | Criar `IFinanceiroService.cs` / `FinanceiroService.cs` — orquestra pós-processamento e lógicas de negócio | Backend Engineer | P5-T02..09 | Lógica separada dos repositórios | 3h | P1 |
| P5-T11 | Criar `FinanceiroEndpoints.cs` — todos os 18 endpoints financeiros da seção 13.4 do doc_legado | Backend Engineer | P5-T10 | Todos os endpoints /api/v1/financeiro/* implementados | 4h | P1 |
| P5-T12 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P5-T11 | Arquivos de controle atualizados | 15min | P1 |

---

### FASE 6 — MÓDULO PRAZO MÉDIO (P2) (Agente: Database Engineer + Backend Engineer)

**Critério de saída:** 3 níveis de drill-down de prazo médio para recebimento e pagamento funcionando.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P6-T01 | Criar entidades PrazoMedio: PrazoMedioMensal, PrazoMedioPessoa, PrazoMedioDocumentos | Database Engineer | P1-T01 | Classes tipadas para 3 níveis de drill-down | 2h | P2 |
| P6-T02 | Criar `IPrazoMedioRepository.cs` / `PrazoMedioRepository.cs` — 6 funções table-valued (`FN_PRAZO_MEDIO_*`) de 3 packages diferentes | Database Engineer | P6-T01 | Todos os ~24 métodos mapeados com Dapper | 4h | P2 |
| P6-T03 | Implementar os 7 tipos de operação PZM Pagamento (G, I, O, U, L, M, S) como enum ou constante | Database Engineer | P6-T02 | Operações corretamente filtradas | 1h | P2 |
| P6-T04 | Criar `PrazoMedioEndpoints.cs` — todos os 8 endpoints da seção 13.4 do doc_legado para Prazo Médio | Backend Engineer | P6-T02..03 | 8 endpoints de prazo médio implementados | 3h | P2 |
| P6-T05 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P6-T04 | Arquivos de controle atualizados | 15min | P2 |

---

### FASE 7 — MÓDULO VENDAS (P2) (Agente: Database Engineer + Backend Engineer)

**Critério de saída:** Análise de vendas, ranking de clientes e plano de vendas implementados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P7-T01 | Criar entidades de Vendas: AnaliseVendas, RankingClientes, PlanoVendasResultado, ComercialMI | Database Engineer | P1-T01 | Classes tipadas para módulo vendas | 2h | P2 |
| P7-T02 | Criar `IVendasRepository.cs` / `VendasRepository.cs` — mapear `DaoVendas` completo (seção 5.5) | Database Engineer | P7-T01 | Todos os 6 métodos de DaoVendas mapeados | 4h | P2 |
| P7-T03 | Implementar pós-processamento de Comercial MI (remoção de colunas diferença, colunas decimais zeradas, última linha de total) | Backend Engineer | P7-T02 | Pós-processamento na camada de serviço, não no repositório | 2h | P2 |
| P7-T04 | Criar `VendasEndpoints.cs` — todos os 6 endpoints de vendas da seção 13.4 | Backend Engineer | P7-T02..03 | Endpoints GET /api/v1/vendas/* implementados | 3h | P2 |
| P7-T05 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P7-T04 | Arquivos de controle atualizados | 15min | P2 |

---

### FASE 8 — MÓDULOS SECUNDÁRIOS (P2/P3) (Agente: Database Engineer + Backend Engineer)

**Critério de saída:** Agrícola, Calendário, Agrupamentos DRE e DBA implementados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P8-T01 | Criar `IAgricolaRepository.cs` / `AgricolaRepository.cs` — ComprasFrutas (3 procedures) | Database Engineer | P1-T01 | Endpoints /api/v1/agricola/* funcionando | 3h | P2 |
| P8-T02 | Criar `ICalendarioRepository.cs` / `CalendarioRepository.cs` — GetDatas, Insere, Update + lógica de cores | Database Engineer | P1-T01 | CRUD de calendário com cores (vermelho/azul/amarelo) | 3h | P2 |
| P8-T03 | Criar `IAgrupamentoRepository.cs` / `AgrupamentoRepository.cs` — CRUD AgrupamentoDre | Database Engineer | P1-T01 | CRUD de agrupamentos funcionando | 2h | P3 |
| P8-T04 | Criar `IDbaRepository.cs` / `DbaRepository.cs` — GetSessoes, MatarSessao, GetLocks | Database Engineer | P1-T01 | Endpoints /api/v1/dba/* (admin only) funcionando | 2h | P3 |
| P8-T05 | Criar endpoints para todos os módulos P8: AgricolaEndpoints, CalendarioEndpoints, AgrupamentoEndpoints, DbaEndpoints, AjusteEndpoints, PortadorEndpoints | Backend Engineer | P8-T01..04 | Todos os 16 endpoints restantes da seção 13.4 implementados | 4h | P2 |
| P8-T06 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P8-T05 | Arquivos de controle atualizados | 15min | P2 |

---

### FASE 9 — WORKER SERVICE (P1) (Agente: DevOps Engineer + Backend Engineer)

**Critério de saída:** Worker Service rodando tarefas agendadas equivalentes ao Windows Service legado.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P9-T01 | Analisar `Treis.Service/srvPrincipal.cs` do legado para mapear todas as tarefas agendadas | Backend Engineer | - | Lista completa de tarefas e intervalos do Windows Service legado | 1h | P1 |
| P9-T02 | Implementar `Empresa.Worker/Worker.cs` herdando `BackgroundService` com tarefas reais | Backend Engineer | P9-T01 | Worker executa sem exceção | 2h | P1 |
| P9-T03 | Configurar agendamento com `PeriodicTimer` para cada tarefa identificada | Backend Engineer | P9-T02 | Tarefas executadas nos intervalos corretos | 2h | P1 |
| P9-T04 | Adicionar health check ao Worker Service | DevOps Engineer | P9-T02 | GET /health retorna status do Worker | 1h | P1 |
| P9-T05 | Configurar injeção de dependência no Worker (acesso a DbSession + repositórios) | Backend Engineer | P9-T02 | Worker tem acesso ao banco via DI | 1h | P1 |
| P9-T06 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P9-T05 | Arquivos de controle atualizados | 15min | P1 |

---

### FASE 10 — QUALIDADE E TESTES (P1) (Agente: QA Engineer)

**Critério de saída:** Cobertura de testes ≥ 60%, testes de integração para todos os módulos críticos.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P10-T01 | Criar projeto `Empresa.Tests` (xUnit) | QA Engineer | P0-T04 | `dotnet test` roda sem erros | 1h | P1 |
| P10-T02 | Configurar mocking de `IDbConnection` para testes de repositórios | QA Engineer | P10-T01 | Repositórios testados sem banco real | 2h | P1 |
| P10-T03 | Testes unitários para todos os Services (AuthService, UsuarioService, FinanceiroService, ComprasService) | QA Engineer | P10-T02 | Cobertura dos cenários felizes e infelizes | 6h | P1 |
| P10-T04 | Testes de integração — validar 1 cenário feliz por módulo com Oracle real (ou Docker Oracle) | QA Engineer | P10-T02 | Integração Oracle validada | 4h | P1 |
| P10-T05 | Testes de segurança: endpoint sem JWT retorna 401, com JWT inválido retorna 401, com permissão errada retorna 403 | QA Engineer | Fase 2 | Autorização testada em todos os endpoints | 3h | P1 |
| P10-T06 | Testes de performance: endpoints respondem em < 2s para cenários normais | QA Engineer | Todas as fases | Baseline de performance registrada | 2h | P1 |
| P10-T07 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P10-T06 | Cobertura ≥ 60% registrada | 15min | P1 |

---

### FASE 11 — DOCUMENTAÇÃO E SWAGGER (P1) (Agente: Backend Engineer + Architect)

**Critério de saída:** Todos os endpoints documentados no Swagger, README atualizado.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P11-T01 | Configurar Swagger com título, versão, descrição e autenticação JWT | Backend Engineer | P0-T03 | Swagger UI acessível e funcional | 1h | P1 |
| P11-T02 | Adicionar `.WithSummary()` + `.WithDescription()` + `.Produces()` em cada endpoint | Backend Engineer | Todas as fases | Zero endpoint sem descrição | 4h | P1 |
| P11-T03 | Documentar parâmetros de query, path e body com exemplos | Backend Engineer | P11-T02 | Swagger mostra exemplos válidos | 3h | P1 |
| P11-T04 | Atualizar `docs/README.md` com instruções de setup, execução e deploy | DevOps Engineer | Todas as fases | Desenvolvedor consegue rodar o projeto seguindo o README | 2h | P1 |
| P11-T05 | Revisar contratos em `CLAUDE.md` — atualizar se houve mudanças arquiteturais | Architect | Todas as fases | CLAUDE.md reflete o estado atual real | 1h | P1 |
| P11-T06 | Atualizar `TASKS.md` e `PROGRESS.md` finais | Architect | P11-T05 | Todos os itens marcados como concluídos | 30min | P1 |

---

### FASE 12 — DEVOPS E INFRAESTRUTURA (P2) (Agente: DevOps Engineer)

**Critério de saída:** Pipeline CI/CD, Dockerfile e deploy configurados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço | Prioridade |
|----|--------|--------|--------------|-------------------|---------|:---:|
| P12-T01 | Criar `Dockerfile` para `Empresa.Api` (multi-stage build) | DevOps Engineer | Fase 10 | `docker build` e `docker run` sem erros | 2h | P2 |
| P12-T02 | Criar `docker-compose.yml` para desenvolvimento local (Api + Oracle) | DevOps Engineer | P12-T01 | `docker-compose up` sobe o ambiente | 2h | P2 |
| P12-T03 | Configurar pipeline CI (GitHub Actions ou Azure DevOps) — build + test em cada PR | DevOps Engineer | Fase 10 | Pipeline executa automaticamente | 3h | P2 |
| P12-T04 | Configurar pipeline CD — deploy em ambiente de homologação | DevOps Engineer | P12-T03 | Deploy automático em merge para main | 3h | P2 |
| P12-T05 | Configurar variáveis de ambiente por ambiente (dev, hom, prod) | DevOps Engineer | P12-T03 | Sem segredos no repositório | 2h | P2 |
| P12-T06 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P12-T05 | Arquivos de controle atualizados | 15min | P2 |

---

### 5. Contratos Técnicos e Padrões

Os contratos arquiteturais e padrões de desenvolvimento estão definidos nos seguintes documentos de governança:

| Documento | Propósito | Localização |
|-----------|-----------|-------------|
| `CLAUDE.md` | Contratos arquiteturais, regras de ouro, agentes, checklists | Raiz do projeto |
| `docs/architecture.md` | Clean Architecture, fluxo de requisição, ADRs | `docs/` |
| `docs/data-layer.md` | Padrões Dapper + Oracle, DbSession, Repository | `docs/` |
| `docs/api-patterns.md` | Minimal APIs, DTOs, IResult, versionamento | `docs/` |
| `docs/coding-standards.md` | Estilo de código, async/await, DI, tratamento de erros | `docs/` |
| `docs/error-handling.md` | Middleware de exceção, padrões de resposta de erro | `docs/` |
| `docs/security.md` | JWT, BCrypt, rate limiting, proteção contra SQL injection | `docs/` |
| `docs/testing.md` | Estratégia de testes, xUnit, mocking | `docs/` |
| `skills/dapper-orm.md` | Guia detalhado de Dapper | `skills/` |
| `skills/oracle-best-practices.md` | Boas práticas Oracle | `skills/` |
| `skills/clean-architecture-dotnet.md` | Clean Architecture em .NET | `skills/` |

**Regras de ouro (extraídas do CLAUDE.md):**

1. Clean Architecture com 4 camadas estritas
2. Dapper + Oracle para toda comunicação com banco
3. DI nativa do .NET (sem containers de terceiros)
4. Nomenclatura: namespaces `Empresa.Camada.Subdominio`, classes em inglês, comentários em português
5. Async/Await obrigatório em toda operação de I/O
6. Models vs DTOs separados — nunca expor entidades na API
7. Endpoints retornam `IResult` padronizado
8. Senhas com hash BCrypt ou PBKDF2

**Regras de migração do legado (extraídas do doc_legado.md seção 13.5):**

1. **PRESERVAÇÃO TOTAL DO BANCO ORACLE** — nenhuma alteração em tabelas, views, procedures
2. Substituir `System.Data.OracleClient` → `Oracle.ManagedDataAccess.Core`
3. Migrar DataSet/DataTable → entidades tipadas C#
4. Substituir FormsAuthentication + Session → JWT Bearer Token
5. Conexão compartilhada (`ref OracleConnection`) → Unit of Work ou transação explícita
6. Pós-processamento → camada de serviço

---

### 6. Critérios de Aceitação do Projeto

O projeto de modernização será considerado concluído quando TODOS os critérios abaixo forem atendidos:

#### 6.1. Infraestrutura
- [ ] `dotnet build` passa sem erros e sem warnings em todos os projetos
- [ ] `dotnet test` passa com cobertura ≥ 60%
- [ ] `Class1.cs` removidos de todos os projetos
- [ ] Health checks `/api/health` e `/api/health/database` respondem 200

#### 6.2. Autenticação e Segurança
- [ ] POST `/api/v1/auth/login` retorna JWT com claims corretos
- [ ] Senhas armazenadas com BCrypt (nunca texto plano)
- [ ] Endpoints sem token retornam 401
- [ ] Rate limiting configurado nos endpoints de auth
- [ ] Parâmetros Dapper em TODAS as queries (zero SQL concatenado)

#### 6.3. Funcionalidades (Mapeamento Completo do Legado)
- [ ] Todos os 43+ endpoints REST da seção 13.4 do doc_legado implementados
- [ ] Todas as 20+ packages Oracle referenciadas como constantes em `OracleProcedures.cs`
- [ ] Todos os 6 DAOs do legado mapeados para repositories
- [ ] Todas as 30+ tabelas Oracle mapeadas para modelos/entidades

#### 6.4. Regras de Negócio Críticas (Checklist da seção 14.2 do doc_legado)
- [ ] Autenticação com validação ATIVO='S' e ATUALIZA_SENHA
- [ ] Menu hierárquico com subquery UNION (páginas filhas + pais)
- [ ] TOP 10 páginas mais acessadas
- [ ] Controle de acesso multi-empresa com EXISTS nas queries
- [ ] 3 níveis de drill-down no Prazo Médio (recebimento e pagamento)
- [ ] 3 versões de Posição Financeira (legado, new, sreal)
- [ ] 3 versões de DRE (padrão, homologado, out)
- [ ] 7 tipos de operação no PZM Pagamento (G, I, O, U, L, M, S)
- [ ] Calendário financeiro com lógica de cores (vermelho/azul/amarelo)
- [ ] UPSERT manual em acesso a páginas e ajustes financeiros
- [ ] Pós-processamento: remoção de colunas/semanas vazias, colunas zeradas

#### 6.5. Arquitetura
- [ ] Clean Architecture respeitada: Api → Data → Util (sem referências circulares)
- [ ] Minimal APIs consistentemente em todos os endpoints
- [ ] DTOs Request/Response separados das entidades de banco
- [ ] Middleware global de exception handling
- [ ] Middleware de request logging
- [ ] Swagger/OpenAPI com todos os endpoints documentados

#### 6.6. Worker Service
- [ ] Worker executa tarefas agendadas equivalentes ao Windows Service legado
- [ ] Worker tem health check funcional
- [ ] DI configurada no Worker (DbSession + repositórios)

#### 6.7. DevOps
- [ ] Dockerfile multi-stage para a API
- [ ] Pipeline CI executando build + test
- [ ] Variáveis de ambiente por ambiente (sem segredos no repositório)

---

### 7. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|:---:|:---:|-----------|
| Complexidade das 3 versões de DRE e Posição Financeira com comportamentos distintos | Alta | Alto | Criar testes de integração específicos para cada versão; documentar diferenças com exemplos de entrada/saída |
| REF CURSORs com múltiplos cursores de saída podem ter comportamento diferente no ManagedDataAccess | Média | Alto | Testar `QueryMultiple` do Dapper com `OracleDynamicParameters` na Fase 1 antes de expandir |
| SQL Injection no legado — queries concatenadas precisam ser todas reescritas com parâmetros | Alta | Crítico | Code review obrigatório em TODO repositório; usar apenas `DynamicParameters` |
| Worker Service: código do `srvPrincipal.cs` pode não estar acessível ou documentado | Alta | Médio | Solicitar acesso ao repositório legado antes da Fase 9; documentar cada job encontrado |
| Multi-empresa com EXISTS nas queries — lógica de segurança pode ser complexa de reproduzir | Média | Alto | Extrair lógica de filtro multi-empresa para método helper reutilizável |
| Funções table-valued Oracle (`FN_*`) podem não ser suportadas diretamente pelo Dapper | Média | Médio | Testar `QueryAsync<T>` com `SELECT * FROM TABLE(PKG.FN_*(...))` na Fase 6 |
| Pós-processamento de dados (remoção dinâmica de colunas) pode ter bugs sutis | Média | Médio | Testes unitários extensivos com DataTables de entrada variadas |
| Prazo: ~199h (5 semanas) pode ser insuficiente se houver bloqueios | Média | Alto | Priorizar fases P0 e P1 primeiro; P2 e P3 podem ser entregues em iterações posteriores |

---

### 8. Perguntas em Aberto

Estas questões precisam de decisão para prosseguir com o desenvolvimento. Foram identificadas durante a análise e registro inicial (também documentadas em `PROGRESS.md`):

1. **A connection string Oracle será injetada por variável de ambiente ou appsettings?**
   - Recomendação: `appsettings.json` para desenvolvimento, User Secrets para credentials, variáveis de ambiente em produção

2. **O Worker terá scheduler (Quartz/Hangfire) ou será simples loop com PeriodicTimer?**
   - Recomendação: `PeriodicTimer` para simplicidade (ADR-004), migrar para Quartz se houver necessidade de cron expressions

3. **Será usado JWT com refresh token ou apenas access token?**
   - Recomendação: Access token (curta duração, 15-60min) + Refresh token (longa duração, 7-30 dias)

4. **Haverá migrações versionadas ou o schema Oracle já está pronto?**
   - Premissa: Schema Oracle NÃO será alterado (regra de ouro do doc_legado). Migrações não são necessárias.

5. **O repositório legado (`TreisTecnovin.sln`) está acessível para referência?**
   - Crítico para: análise do `srvPrincipal.cs` (Worker), validação de queries SQL, referência de componentes DevExpress

6. **Existe ambiente Oracle de desenvolvimento/homologação disponível?**
   - Crítico para: testes de integração, validação de mapeamento de REF CURSORs

7. **Qual a estratégia de versionamento da API?**
   - Recomendação: `/api/v1/` desde o início, preparado para `/api/v2/` quando houver breaking changes

8. **As entidades do legado que usam DataSet/DataTable têm documentação de esquema de colunas?**
   - O doc_legado.md cobre a maioria, mas algumas colunas podem precisar de validação contra o banco real

---

### Apêndice A — Mapeamento Completo de DAOs → Repositories

| DAO Legado | Repository .NET 9 | Entidades | Linhas no doc_legado |
|------------|-------------------|-----------|:---:|
| `DaoAcesso` | `AcessoRepository` | Usuario, Perfil, Pagina, Estabelecimento | Seção 5.3 |
| `DaoCompras` | `ComprasRepository` | ResumoAnualCompras, ComiteComprasNF, ProgressaoPrecoNF, PrevisaoCompra, CfopTransferencia, CentrosCusto | Seção 5.4 |
| `DaoVendas` | `VendasRepository` | AnaliseVendas, RankingClientes, PlanoVendasResultado, ComercialMI | Seção 5.5 |
| `DaoPainel` (Financeiro) | `FinanceiroRepository` | FluxoCaixa, PosicaoFinanceira, Dre, ProjecaoFinanceira, AjusteFinanceiro, PortadorSaldo | Seção 5.6 |
| `DaoPainel` (PZM) | `PrazoMedioRepository` | PrazoMedioMensal, PrazoMedioPessoa, PrazoMedioDocumentos | Seção 5.6 |
| `DaoPainel` (Agrícola) | `AgricolaRepository` | ComprasFrutas, SafraMeta | Seção 5.6 |
| `DaoPainel` (Calendário) | `CalendarioRepository` | CalendarioFinanceiro | Seção 5.6 |
| `DaoPainel` (Agrupamentos) | `AgrupamentoRepository` | AgrupamentoDRE | Seção 5.6 |
| `DaoSessao` | `DbaRepository` | SessaoOracle, TabelaLock, UsuarioLock | Seção 5.7 |

### Apêndice B — Mapeamento de Packages Oracle (20+ packages)

| Package | Módulo | Procedures |
|---------|--------|------------|
| `packageCompraNf` | Compras | `sp_realizado` (4 cursores), `sp_previsto_realizado_item`, `sp_progressao_preco`, `sp_agrup_conta_ccusto` (+detalhe+det_prod), `sp_relatorio_compra` (+2), `sp_busca_nf`, `sp_carga_prev_real_item`, etc. |
| `packageCompraNew` | Compras | `sp_item_compra_sem_comite`, `sp_consulta_itens`, `sp_previsto_realizado_item`, `sp_progressao_preco` |
| `packageFinanceiro` | Financeiro | `sp_fluxo_caixa` (2 cursores), `sp_resumo_geral_flxcxa`, `sp_fluxo_caixa_analitico`, `sp_fluxo_ccontabil_pagar`, `sp_fluxo_dre`, `sp_documento`, `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev` |
| `packageFinanceiroNew` | Financeiro | `sp_posicao`, `sp_posicao_dia_util`, `sp_posicao_prev_real`, `sp_posicao_detalhe`, `sp_posicao_prev_real_detalhe`, `sp_posicao_semanal`, `sp_resumo_posicao` (3 cursores), etc. |
| `packageFinanceiroNewPor` | Financeiro | `sp_posicao_portador`, `sp_posicao_portador_detalhe` |
| `packageFinanceiroHomolog` | DRE | `sp_fluxo_dre`, `sp_documento`, `sp_detalha_docto_mutuo`, `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev_mut` |
| `packageFinanceiroOut` | DRE | `sp_fluxo_dre`, `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev` |
| `packageFinanceira` | Financeiro (legado) | `sp_posicao`, `sp_posicao_detalhe` |
| `packageFinanceiroLcto` | Financeiro (legado) | `sp_posicao` |
| `PKG_POSICAO_FINANCEIRA_SREAL` | Financeiro | `sp_posicao`, `sp_posicao_detalhe` |
| `packagePlanejamentoDre` | Planejamento | `sp_projecao`, `sp_percentual_agrupamento`, `sp_salva_percentual_periodo` |
| `packageProjecaoFinanceira` | Projeção | `sp_projecao`, `sp_projecao_detalhe` |
| `packageCadastroSaldo` | Portador | `sp_lista_Portadores`, `sp_lista_saldo_inicial_periodo`, `sp_lista_saldo_portador_dia`, `sp_salva_saldo_inicial`, `sp_atualiza_saldo_periodo`, `sp_prepara_digitacao_saldo` |
| `packageVendas` | Vendas | `sp_painel_vendas_mi` |
| `packageVenda` | Vendas | `sp_comercial_mi`, `sp_comercial_vlr_mi` |
| `packageComercial` | Vendas | `prc_result_vendas` |
| `packageBi` | BI/Vendas | `sp_receita_empr_cliente_comp`, `sp_plano_vendas_resultado` |
| `packageBiPeriodo` | BI/Vendas | `sp_receita_empresa_doc_it_rel` |
| `packageCompras` | Agrícola | `sp_recebimento_frutas`, `sp_recebimento_frutas_det`, `sp_recebimento_frutas_det_nf` |
| `packageDba` | DBA | `sessoes_agrupadas`, `sessoes`, `ses_tab_bloqueios`, `sessao_tabela`, `matar_sessao`, `bloqueio_usuario` |
| `PKG_BI` | BI (funções) | `FN_RECEITA_EMPR_PRODUTO`, `FN_RECEITA_EMPRESA_DOC` |
| `PKG_FINANCEIRO` | PZM (funções) | `FN_PRAZO_MEDIO_MENSAL_RECEBE`, `FN_PRAZO_MEDIO_PESSOA_RECEBE`, `FN_PRAZO_MEDIO_RECEBIMENTO`, `FN_PRAZO_MEDIO_EMPRESA_RECEBE`, `FN_PRAZO_MEDIO_MENSAL_PAGA`, `FN_PRAZO_MEDIO_PESSOA_PAGA`, `FN_PRAZO_MEDIO_PAGAMENTO`, `FN_PRAZO_MEDIO_EMPRESA_PAGA` |
| `PKG_PRAZO_MEDIO` | PZM (funções) | `FN_RECEBIMENTO_MENSAL`, `FN_RECEBIMENTO_PESSOA`, `FN_RECEBIMENTO`, `FN_RECEBIMENTO_EMPRESA`, `FN_RECEBIMENTO_LINHA`, `FN_PAGAMENTO_MENSAL`, `FN_PAGAMENTO_PESSOA`, `FN_PAGAMENTO_LINHA`, `FN_PAGAMENTO`, `FN_PAGAMENTO_EMPRESA` |
| `PKG_PRAZO_MEDIO_PMZ` | PZM (funções nova) | `FN_RECEBIMENTO`, `FN_RECEBIMENTO_PESSOA`, `FN_RECEBIMENTO_DETALHADO`, `FN_PAGAMENTO`, `FN_PAGAMENTO_PESSOA`, `FN_PAGAMENTO_DETALHADO` |

---

*PRD gerado em 28/07/2026 com base na análise completa dos artefatos: `CLAUDE.md`, `TASKS.md`, `PROGRESS.md`, `docs/doc_legado.md` e inspeção estrutural do projeto GestaoNew.*