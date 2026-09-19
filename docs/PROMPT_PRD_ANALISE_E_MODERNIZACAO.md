# 🎯 PROMPT PRD — Análise Diagnóstica e Planejamento de Modernização TreisTecnovin

> **Projeto**: GestaoNew — Modernização TreisTecnovin (.NET 9 + Dapper + Oracle)
> **Contexto**: Prompt orquestrador para geração do PRD completo com análise de lacunas e roadmap de implementação
> **Usar em**: `D:\clientes\go19\tecnovin\teste\GestaoNew\`

---

## 📋 INSTRUÇÕES PARA O AGENTE ORQUESTRADOR

Você é um **Arquiteto de Software Sênior** executando uma missão crítica de diagnóstico e planejamento.
Seu trabalho é produzir um **PRD (Product Requirements Document)** completo para o projeto GestaoNew.

**ANTES DE QUALQUER COISA**, execute o protocolo obrigatório:

```
1. Leia `CLAUDE.md`    → contratos arquiteturais e regras de ouro
2. Leia `PROGRESS.md`  → status atual e tentativas anteriores com falha
3. Leia `TASKS.md`     → backlog atual e o que já foi concluído
4. Leia `docs/doc_legado.md` → documentação completa do sistema legado (1.651 linhas)
5. Inspecione a estrutura atual do projeto GestaoNew
```

---

## 🗂️ ETAPA 0 — PRÉ-ANÁLISE: LEITURA OBRIGATÓRIA DOS ARTEFATOS

### 0.1 Agente: **Architect** (`agents/architect.md`)

**Ação**: Ler e internalizar todos os documentos de governança antes de qualquer análise.

**Documentos a ler (nesta ordem)**:

| Ordem | Arquivo | Propósito |
|-------|---------|-----------|
| 1 | `CLAUDE.md` | Contratos arquiteturais, regras de ouro, agentes |
| 2 | `PROGRESS.md` | Estado atual, o que falhou, perguntas em aberto |
| 3 | `TASKS.md` | Backlog granular, critérios de saída por fase |
| 4 | `agents/README.md` | Personas dos agentes disponíveis |
| 5 | `docs/doc_legado.md` | Documentação completa do sistema legado |
| 6 | `docs/architecture.md` | Arquitetura alvo definida |
| 7 | `docs/data-layer.md` | Padrões Dapper + Oracle |
| 8 | `docs/api-patterns.md` | Padrões de API REST |
| 9 | `docs/coding-standards.md` | Padrões de código C# |

**Saída esperada**: Confirmação de leitura + lista de inconsistências já identificadas na leitura inicial.

---

## 🔍 ETAPA 1 — DIAGNÓSTICO DA ESTRUTURA ATUAL

### 1.1 Agente: **Architect** + **Backend Engineer**

**Missão**: Inspecionar cada arquivo e pasta do projeto GestaoNew e mapear o estado real vs o estado esperado.

**Execute a inspeção completa desta estrutura**:

```
GestaoNew/
├── Empresa.Api/
│   ├── Program.cs                 ← INSPECIONAR: está completo ou é o template padrão?
│   ├── appsettings.json           ← INSPECIONAR: tem connection string Oracle ou é placeholder?
│   ├── appsettings.Development.json
│   ├── Properties/
│   ├── Empresa.Api.csproj         ← INSPECIONAR: quais NuGet packages instalados?
│   └── Empresa.Api.http           ← INSPECIONAR: tem requisições de teste?
│
├── Empresa.Data/
│   ├── Class1.cs                  ← PROBLEMA: arquivo placeholder ainda existe?
│   ├── Models/                    ← INSPECIONAR: quais entidades existem?
│   ├── Empresa.Data.csproj        ← INSPECIONAR: packages e referências
│   └── (Repositories/, DbSession.cs?) ← VERIFICAR: existem ou estão faltando?
│
├── Empresa.Util/
│   ├── Class1.cs                  ← PROBLEMA: arquivo placeholder ainda existe?
│   └── Empresa.Util.csproj        ← INSPECIONAR: o que tem aqui?
│
├── Empresa.Worker/
│   └── Empresa.Worker.csproj      ← INSPECIONAR: Worker implementado ou só scaffold?
│
├── CLAUDE.md      ← Já lido na Etapa 0
├── PROGRESS.md    ← Já lido na Etapa 0
├── TASKS.md       ← Já lido na Etapa 0
├── agents/        ← INSPECIONAR: quais agentes existem?
├── commands/      ← INSPECIONAR: quais comandos reutilizáveis existem?
├── skills/        ← INSPECIONAR: quais skills técnicas existem?
└── docs/          ← Já lidos na Etapa 0
```

### 1.2 Checklist de Diagnóstico Estrutural

Para cada item abaixo, registre: ✅ Implementado | ⚠️ Parcialmente | ❌ Ausente | 🔴 Incorreto

**Infraestrutura Base**:
- [ ] `Program.cs` configurado com DI, Serilog, Swagger, CORS, JWT middleware
- [ ] Connection string Oracle em `appsettings.json` (não hardcoded)
- [ ] Health check endpoint `/api/health` configurado
- [ ] `Class1.cs` removidos de todos os projetos
- [ ] Referências entre projetos corretas (Empresa.Api → Empresa.Data → Empresa.Util)

**Camada de Dados (Empresa.Data)**:
- [ ] `DbSession.cs` — gerenciamento de conexão Oracle (scoped)
- [ ] `OracleConnectionFactory.cs` — factory para criar conexões
- [ ] `OracleProcedures.cs` — constantes de packages/procedures
- [ ] Pasta `Repositories/` com interfaces e implementações

**Repositórios Esperados** (conforme `doc_legado.md` seção 13.2):
- [ ] `IAcessoRepository.cs` / `AcessoRepository.cs` (equivale a `DaoAcesso`)
- [ ] `IComprasRepository.cs` / `ComprasRepository.cs` (equivale a `DaoCompras`)
- [ ] `IVendasRepository.cs` / `VendasRepository.cs` (equivale a `DaoVendas`)
- [ ] `IFinanceiroRepository.cs` / `FinanceiroRepository.cs` (equivale a `DaoPainel`)
- [ ] `IPrazoMedioRepository.cs` / `PrazoMedioRepository.cs` (equivale a `DaoPainel`)
- [ ] `IAgricolaRepository.cs` / `AgricolaRepository.cs` (equivale a `DaoPainel`)
- [ ] `ICalendarioRepository.cs` / `CalendarioRepository.cs` (equivale a `DaoPainel`)
- [ ] `IDbaRepository.cs` / `DbaRepository.cs` (equivale a `DaoSessao`)
- [ ] `IAgrupamentoRepository.cs` / `AgrupamentoRepository.cs` (equivale a `DaoPainel`)

**Modelos (Empresa.Data/Models)**:
- [ ] `Usuario.cs` — campos: ID_USUARIO, NOME, LOGIN, SENHA, ATIVO, ID_PERFIL, etc.
- [ ] `Perfil.cs` — campos: ID_PERFIL, NOME_PERFIL, etc.
- [ ] `Pagina.cs` — campos: ID_PAGINA, URL, TITULO_ABA, CHAVE_CONTROLE, ID_PAGINA_PAI, ORDEM, etc.
- [ ] `Estabelecimento.cs` — campos conforme `VW_ESTABELECIMENTO_NEW`
- [ ] `ComiteCompras.cs` e `ComiteComprasItem.cs`
- [ ] `ResumoAnualCompras.cs` — estrutura para 4 cursores (resultado, totais, grafico, grafico2)
- [ ] `FluxoCaixa.cs` — master + detalhe
- [ ] `PosicaoFinanceira.cs`
- [ ] `Dre.cs` — 3 versões (padrão, homologado, out)
- [ ] `PrazoMedio.cs` — recebimento e pagamento (3 níveis de drill-down)

**Camada de API (Empresa.Api)**:
- [ ] `Controllers/` ou `Endpoints/` com Minimal APIs
- [ ] `AuthController`/`AuthEndpoints` — POST /api/auth/login, refresh, alterar-senha
- [ ] `UsuarioController` — CRUD completo
- [ ] `PaginaController` — GET /api/paginas/menu, /recentes, POST /api/paginas/acesso
- [ ] `ComprasController` — mínimo: resumo-anual, previsto-realizado, progressao-preco
- [ ] `FinanceiroController` — mínimo: posicao, fluxo-caixa, dre
- [ ] `DTOs/` — Request e Response separados
- [ ] `Services/` — camada de serviços com regras de negócio
- [ ] `Middlewares/` — ExceptionHandling + RequestLogging
- [ ] Swagger/OpenAPI configurado e documentado

**Worker Service (Empresa.Worker)**:
- [ ] `BackgroundService` implementado (migração do Windows Service legado)
- [ ] Lógica de agendamento (PeriodicTimer ou Quartz.NET)
- [ ] Health check no Worker

**Qualidade**:
- [ ] Projeto de testes (`Empresa.Tests`) existe?
- [ ] `dotnet build` sem erros?
- [ ] Testes passando? (`dotnet test`)

---

## 📊 ETAPA 2 — ANÁLISE DE CONFORMIDADE COM O LEGADO

### 2.1 Agente: **Database Engineer** (`agents/database-engineer.md`)

**Missão**: Cruzar o código atual do GestaoNew com `docs/doc_legado.md` e verificar se todas as regras de negócio críticas estão mapeadas.

**Verifique os seguintes pontos críticos documentados em `doc_legado.md`**:

#### 2.1.1 Autenticação e Segurança (Seção 2.1 do doc_legado)
- [ ] Fluxo de login valida `ATIVO = 'S'` — implementado?
- [ ] Campo `ATUALIZA_SENHA` tratado no login — implementado?
- [ ] `SalvaAcessoUsuario()` — incrementa `QUANTIDADE_ACESSO` e atualiza `DATA_HORA_ULTIMO_ACESSO`?
- [ ] Menu hierárquico com UNION (filhas + pais) — `GetPaginas(IDPerfil)` implementado?
- [ ] TOP 10 páginas mais acessadas — implementado?
- [ ] UPSERT em `ACESSO_VISUALIZACAO_PAGINA` — implementado?
- [ ] Registro de relatórios em `REGISTRO_RELATORIOS` — implementado?

#### 2.1.2 Módulo Compras (Seção 2.2 do doc_legado)
- [ ] `packageCompraNf.sp_realizado` — 4 cursores mapeados? (resultado, totais, grafico, grafico2)
- [ ] `packageCompraNf.sp_previsto_realizado_item` — implementado?
- [ ] `packageCompraNf.sp_progressao_preco` — implementado?
- [ ] Gestão de CFOP (transferência + exceção + log) — implementado?
- [ ] `sp_agrup_conta_ccusto` + `_detalhe` + `_det_prod` — centro de custo implementado?

#### 2.1.3 Módulo Financeiro - Fluxo de Caixa (Seção 2.3)
- [ ] 3 versões de `sp_posicao` (legado, new, sreal) — todas implementadas?
- [ ] Lógica de remoção de colunas vazias (Posição Semanal) — implementada?
- [ ] Fluxo de caixa analítico — remoção de colunas PORT/PORTADOR/CLIENTE — implementado?
- [ ] `sp_resumo_geral_flxcxa` — totais fluxo de caixa — implementado?

#### 2.1.4 Módulo DRE (Seção 2.4)
- [ ] 3 versões de DRE: padrão (`packageFinanceiro`), homologado (`packageFinanceiroHomolog`), out (`packageFinanceiroOut`) — implementadas?
- [ ] Drill-down DRE: conta → prev → documento (2 cursores) — implementado?
- [ ] `sp_documento` com 8 parâmetros incluindo série NF — implementado?

#### 2.1.5 Prazo Médio (Seção 2.8 do doc_legado)
- [ ] 3 níveis de drill-down recebimento: mensal → pessoa → documentos — implementado?
- [ ] 3 níveis de drill-down pagamento: mensal → pessoa → documentos — implementado?
- [ ] 7 tipos de operação no PZM Pagamento (G, I, O, U, L, M, S) — implementado?
- [ ] Pivot de recebimento e pagamento — implementado?

#### 2.1.6 Regras de Negócio Gerais (Seções 11 e 12 do doc_legado)
- [ ] Multi-empresa com EXISTS nas queries — implementado?
- [ ] Calendário financeiro com lógica de cores — implementado?
- [ ] Pós-processamento de dados (remoção de colunas/linhas) na camada de serviço — implementado?
- [ ] Conexão compartilhada em fluxos multi-etapa — substituído por Unit of Work/Transaction?

---

## 🏗️ ETAPA 3 — ANÁLISE ARQUITETURAL E TECNOLÓGICA

### 3.1 Agente: **Architect** (`agents/architect.md`)

**Missão**: Avaliar se a arquitetura atual do GestaoNew está alinhada com as melhores práticas de .NET 9 e com os contratos do `CLAUDE.md`.

#### 3.1.1 Checklist de Modernidade .NET 9

**Clean Architecture**:
- [ ] Separação clara: Api → Data → Util (nenhuma camada acessa outra que não seja a imediatamente abaixo)
- [ ] Sem referências circulares entre projetos
- [ ] Domain/Business logic isolada (não dentro de Controllers)

**ASP.NET Core / Minimal APIs**:
- [ ] Usando Minimal APIs (`.MapGet`, `.MapPost`) OU Controllers (Controller MVC) — consistentemente
- [ ] Endpoints versionados (`/api/v1/...`) conforme contrato CLAUDE.md
- [ ] `IResult` retornado em todos os endpoints (`Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`)
- [ ] Global exception middleware configurado
- [ ] Request logging middleware configurado
- [ ] Swagger/OpenAPI com descrição em todos os endpoints

**Injeção de Dependência**:
- [ ] Apenas `Microsoft.Extensions.DependencyInjection` (sem Autofac, Ninject, etc.)
- [ ] Repositórios registrados como Scoped
- [ ] DbSession registrado como Scoped (uma conexão por request)
- [ ] Sem ServiceLocator anti-pattern

**Async/Await**:
- [ ] Todas as operações de I/O são `async Task` — sem `.Result` ou `.Wait()`
- [ ] `await using` para conexões Oracle
- [ ] CancellationToken propagado nos métodos de repositório

**Segurança**:
- [ ] JWT Bearer configurado no `Program.cs`
- [ ] `[Authorize]` ou `RequireAuthorization()` em todos os endpoints protegidos
- [ ] Senhas com hash (BCrypt ou PBKDF2) — nunca texto plano
- [ ] Parâmetros Dapper em TODAS as queries (zero SQL concatenado)
- [ ] Rate limiting nos endpoints de autenticação (`.AddRateLimiter()`)

**Dapper + Oracle**:
- [ ] `Oracle.ManagedDataAccess.Core` instalado (não o deprecated `System.Data.OracleClient`)
- [ ] REF CURSORs mapeados como `OracleDynamicParameters` com `OracleDbType.RefCursor`
- [ ] `OracleProcedures.cs` com constantes (sem magic strings)
- [ ] Mapeamento automático snake_case → PascalCase configurado ou manual
- [ ] Múltiplos cursores com `QueryMultiple` do Dapper

**Models vs DTOs**:
- [ ] `Empresa.Data/Models/` contém as entidades de banco
- [ ] Controllers/Endpoints recebem e retornam DTOs (`Empresa.Api/DTOs/`)
- [ ] Nunca a entidade de banco exposta diretamente na API

**Logging e Observabilidade**:
- [ ] Serilog configurado em `Program.cs`
- [ ] Logging estruturado (não string interpolation nos logs)
- [ ] Health checks: `/api/health` + `/api/health/database`
- [ ] Métricas de performance nas queries lentas

**NuGet Packages Esperados (Empresa.Api.csproj)**:
- [ ] `Oracle.ManagedDataAccess.Core` (versão ≥ 23.x)
- [ ] `Dapper` (versão ≥ 2.1)
- [ ] `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] `Serilog.AspNetCore`
- [ ] `Swashbuckle.AspNetCore` OU `Microsoft.AspNetCore.OpenApi`
- [ ] `BCrypt.Net-Next` (para hash de senha)

---

## 📝 ETAPA 4 — GERAÇÃO DO PRD COMPLETO

### 4.1 Agente: **Architect** (coordenador do PRD)

Com base nas 3 etapas anteriores, gere o arquivo `docs/PRD_MODERNIZACAO_GESTAO_NEW.md` dentro do projeto GestaoNew com a seguinte estrutura:

---

### ESTRUTURA DO PRD A GERAR

```markdown
# PRD — Modernização TreisTecnovin: GestaoNew
## Versão: 1.0 | Data: [DATA_ATUAL] | Status: [ATIVO/RASCUNHO]

### 1. Resumo Executivo
- O que foi encontrado (estado atual)
- Lacunas críticas identificadas
- Riscos de negócio
- Esforço estimado total

### 2. Score de Conformidade Atual
- Tabela com % de conformidade por categoria:
  | Categoria | Total Itens | Implementados | % |
  | Estrutura | X | Y | Z% |
  | Regras Negócio | X | Y | Z% |
  | Arquitetura .NET 9 | X | Y | Z% |
  | Segurança | X | Y | Z% |
  | Qualidade/Testes | X | Y | Z% |
  | **TOTAL** | X | Y | **Z%** |

### 3. Gap Analysis — O que está faltando
  Para cada lacuna: descrição, impacto (CRÍTICO/ALTO/MÉDIO/BAIXO), agente responsável

### 4. Roadmap de Implementação (Fases e Etapas)
  [Detalhado abaixo — ver Etapa 4.2]

### 5. Contratos Técnicos e Padrões
  [Referência ao CLAUDE.md e docs/]

### 6. Critérios de Aceitação do Projeto
  [Critérios objetivos para considerar a modernização concluída]

### 7. Riscos e Mitigações

### 8. Perguntas em Aberto
  [Itens que bloqueiam progresso e precisam de decisão]
```

---

## 📅 ETAPA 4.2 — ROADMAP DETALHADO DO PRD

O PRD deve conter um roadmap fragmentado em **fases** e **tarefas atômicas**, indicando para cada tarefa:
- ID único da tarefa (ex: `P1-T03`)
- Descrição clara e acionável
- Agente responsável
- Dependências (quais tarefas devem estar concluídas antes)
- Critério de saída (como saber que está pronto)
- Estimativa de esforço (em horas/dias)
- Prioridade: P0/P1/P2/P3

### FASE 0 — CORREÇÕES URGENTES (Agente: Backend Engineer + Architect)

**Critério de saída**: Build limpo, sem placeholders, DI configurado corretamente.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P0-T01 | Remover `Class1.cs` de `Empresa.Data` e `Empresa.Util` | Backend Engineer | - | `dotnet build` sem warnings de classe não utilizada | 15min |
| P0-T02 | Reescrever `Program.cs` completo: DI, Serilog, Swagger, CORS, JWT | Backend Engineer | - | `dotnet build` + GET /swagger carrega | 2h |
| P0-T03 | Configurar `appsettings.json` com connection string Oracle real (via User Secrets no dev) | DevOps Engineer | - | App sobe sem erro de configuração | 30min |
| P0-T04 | Instalar NuGet packages obrigatórios em todos os projetos | Backend Engineer | - | `dotnet restore` sem erros | 30min |
| P0-T05 | Validar que `dotnet build` e `dotnet test` passam sem erros | QA Engineer | P0-T01..04 | Zero erros e zero warnings | 15min |
| P0-T06 | Atualizar `TASKS.md` e `PROGRESS.md` com status desta fase | Architect | P0-T05 | Arquivos de controle atualizados | 15min |

---

### FASE 1 — INFRAESTRUTURA DE DADOS (Agente: Database Engineer)

**Critério de saída**: Conexão Oracle validada, repositório de Acesso funcionando com Dapper.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P1-T01 | Criar `Empresa.Data/DbSession.cs` — gerenciamento de conexão Oracle (scoped, com `await using`) | Database Engineer | P0-T04 | Conexão Oracle abre e fecha sem memory leak | 1h |
| P1-T02 | Criar `Empresa.Data/Oracle/OracleConnectionFactory.cs` — cria `IDbConnection` a partir de config | Database Engineer | P1-T01 | Retorna conexão válida | 30min |
| P1-T03 | Criar `Empresa.Data/Oracle/OracleProcedures.cs` — constantes de todas as packages/procedures do legado | Database Engineer | - | Todas as 20+ packages do doc_legado mapeadas como constantes | 2h |
| P1-T04 | Criar entidades em `Empresa.Data/Models/`: Usuario, Perfil, Pagina, Estabelecimento | Database Engineer | - | Classes com todos os campos do Oracle mapeados | 2h |
| P1-T05 | Criar `IAcessoRepository.cs` e `AcessoRepository.cs` — GetUsuario (login+senha), GetPaginas (com UNION), SalvaAcessoUsuario, GetNode/GetTree | Database Engineer | P1-T01..04 | Método GetUsuario retorna usuário via Oracle | 4h |
| P1-T06 | Escrever testes unitários para `AcessoRepository` (mock de IDbConnection) | QA Engineer | P1-T05 | Testes passando com cobertura dos casos: login válido, inválido, inativo | 2h |
| P1-T07 | Criar health check endpoint `/api/health/database` validando conexão Oracle | Backend Engineer | P1-T01 | GET /api/health/database retorna 200 com versão Oracle | 1h |
| P1-T08 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P1-T07 | Arquivos de controle atualizados | 15min |

---

### FASE 2 — AUTENTICAÇÃO JWT (P0 — BLOQUEANTE) (Agente: Backend Engineer)

**Critério de saída**: Login retornando JWT válido, middleware protegendo todos os endpoints.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P2-T01 | Configurar JWT em `appsettings.json`: Secret, Issuer, Audience, ExpiryMinutes | Backend Engineer | P0-T02 | Config carregada sem erro | 30min |
| P2-T02 | Criar `Empresa.Api/Services/IAuthService.cs` e `AuthService.cs` — login + geração de JWT com claims (ID_USUARIO, NOME, LOGIN, ID_PERFIL) | Backend Engineer | P1-T05 | Token gerado com claims corretos | 3h |
| P2-T03 | Criar `Empresa.Api/DTOs/LoginRequest.cs` e `LoginResponse.cs` | Backend Engineer | - | DTOs sem referência a entidades de banco | 30min |
| P2-T04 | Criar `Empresa.Api/Endpoints/AuthEndpoints.cs` — POST /api/v1/auth/login, POST /api/v1/auth/refresh, POST /api/v1/auth/alterar-senha | Backend Engineer | P2-T02..03 | POST /api/v1/auth/login retorna JWT | 3h |
| P2-T05 | Configurar middleware JWT em `Program.cs` e proteger todos os endpoints (exceto /auth/login e /health) | Backend Engineer | P2-T04 | Endpoint protegido retorna 401 sem token | 1h |
| P2-T06 | Implementar hash de senha (BCrypt) — validar no login, nunca comparar texto plano | Backend Engineer | P2-T02 | Login com senha hasheada funciona corretamente | 1h |
| P2-T07 | Escrever testes: geração de token, validação de claims, login com senha errada | QA Engineer | P2-T04..06 | Testes passando | 2h |
| P2-T08 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P2-T07 | Arquivos de controle atualizados | 15min |

---

### FASE 3 — MÓDULO ACESSO/USUÁRIOS (P0 — BLOQUEANTE) (Agente: Backend Engineer + Database Engineer)

**Critério de saída**: CRUD completo de Usuários, Perfis e Menu dinâmico funcionando.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P3-T01 | Criar `IUsuarioRepository.cs` / `UsuarioRepository.cs` — CRUD, exclusão lógica (ATIVO='N') | Database Engineer | P1-T01..04 | GET /api/v1/usuarios retorna lista | 3h |
| P3-T02 | Criar `IPaginaRepository.cs` / `PaginaRepository.cs` — GetMenu (com hierarquia UNION), GetRecentes (TOP 10), RegistraAcesso (UPSERT) | Database Engineer | P1-T01..04 | GET /api/v1/paginas/menu retorna árvore correta | 4h |
| P3-T03 | Criar `IPerfilRepository.cs` / `PerfilRepository.cs` — CRUD perfis e vinculação com páginas | Database Engineer | P1-T01..04 | GET /api/v1/perfis retorna perfis | 2h |
| P3-T04 | Criar `IEstabelecimentoRepository.cs` / `EstabelecimentoRepository.cs` — GetNode, GetTree, Registra, Exclui | Database Engineer | P1-T01..04 | GET /api/v1/estabelecimentos retorna árvore | 2h |
| P3-T05 | Criar DTOs: UsuarioRequest, UsuarioResponse, PaginaResponse, PerfilResponse, EstabelecimentoResponse | Backend Engineer | - | DTOs sem entidades de banco expostas | 2h |
| P3-T06 | Criar Services: IUsuarioService/UsuarioService, IPaginaService/PaginaService | Backend Engineer | P3-T01..05 | Lógica de negócio isolada dos repositórios | 2h |
| P3-T07 | Criar Endpoints: UsuarioEndpoints, PaginaEndpoints, PerfilEndpoints, EstabelecimentoEndpoints | Backend Engineer | P3-T06 | Todos os endpoints do mapa (seção 13.4 do doc_legado) implementados | 4h |
| P3-T08 | Registrar relatórios em `REGISTRO_RELATORIOS` — chamada na geração de relatórios | Backend Engineer | P3-T07 | INSERT ocorre ao gerar qualquer relatório | 1h |
| P3-T09 | Testes de integração para todos os endpoints de Acesso | QA Engineer | P3-T07..08 | Testes passando (login → menu → acesso a página) | 3h |
| P3-T10 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P3-T09 | Arquivos de controle atualizados | 15min |

---

### FASE 4 — MÓDULO COMPRAS (P1) (Agente: Database Engineer + Backend Engineer)

**Critério de saída**: Todos os endpoints de compras implementados e testados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P4-T01 | Criar entidades de Compras em `Models/`: ResumoAnualCompras (4 cursores), ComiteComprasNF, ProgressaoPrecoNF, PrevisaoCompra, CfopTransferencia, CentrosCusto | Database Engineer | P1-T01 | Classes tipadas para todos os retornos Oracle | 3h |
| P4-T02 | Criar `IComprasRepository.cs` / `ComprasRepository.cs` — mapear `DaoCompras` completo (seção 5 do doc_legado) | Database Engineer | P4-T01 | Todos os métodos do DaoCompras mapeados com Dapper | 6h |
| P4-T03 | Mapear package `packageCompraNf.sp_realizado` com 4 REF CURSORs usando `OracleDynamicParameters` | Database Engineer | P4-T02 | 4 cursores retornados corretamente | 3h |
| P4-T04 | Criar `IComprasService.cs` / `ComprasService.cs` — pós-processamento: remoção de colunas zeradas | Backend Engineer | P4-T02 | Lógica de pós-processamento isolada | 2h |
| P4-T05 | Criar `ComprasEndpoints.cs` (ou Controller) — todos os endpoints da seção 13.4 do doc_legado para Compras | Backend Engineer | P4-T04 | Endpoints GET /api/v1/compras/* implementados | 4h |
| P4-T06 | Implementar CRUD de CFOP (Transferência + Exceção + Log de alterações) | Database Engineer + Backend Engineer | P4-T05 | CRUD CFOP funcional com log de auditoria | 3h |
| P4-T07 | Implementar Centro de Custo (3 procedures: agrup_conta_ccusto + detalhe + det_prod) | Database Engineer + Backend Engineer | P4-T05 | 3 endpoints de centro de custo funcionando | 2h |
| P4-T08 | Testes de integração para módulo de compras | QA Engineer | P4-T05..07 | Testes passando (resumo anual, previsto/realizado, progressão de preço) | 3h |
| P4-T09 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P4-T08 | Arquivos de controle atualizados | 15min |

---

### FASE 5 — MÓDULO FINANCEIRO (P1) (Agente: Database Engineer + Backend Engineer)

**Critério de saída**: Posição Financeira (3 versões), Fluxo de Caixa e DRE (3 versões) implementados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P5-T01 | Criar entidades de Financeiro: PosicaoFinanceira, FluxoCaixa (master+detalhe), Dre, ProjecaoFinanceira, AjusteFinanceiro | Database Engineer | P1-T01 | Classes tipadas completas | 4h |
| P5-T02 | Criar `IFinanceiroRepository.cs` / `FinanceiroRepository.cs` — mapear `DaoPainel` completo para módulos financeiros | Database Engineer | P5-T01 | Todos os métodos de DaoPainel financeiro mapeados | 8h |
| P5-T03 | Implementar as 3 versões de `sp_posicao` (legado, new, sreal) como métodos separados ou com enum de versão | Database Engineer | P5-T02 | 3 endpoints retornam dados corretos de cada versão | 3h |
| P5-T04 | Implementar Posição Semanal com pós-processamento (remoção de colunas de semanas vazias) na camada de serviço | Backend Engineer | P5-T02..03 | Colunas sem dados removidas corretamente | 2h |
| P5-T05 | Implementar Fluxo de Caixa Analítico com pós-processamento (remoção de PORT, PORTADOR, CLIENTE, NOME_CLIENTE) | Backend Engineer | P5-T02 | Colunas corretamente removidas | 2h |
| P5-T06 | Implementar DRE com 3 packages distintas + drill-down (conta → prev → documento com 2 cursores) | Database Engineer + Backend Engineer | P5-T02 | 3 versões de DRE + drill-down completo | 6h |
| P5-T07 | Implementar Projeção Financeira e Planejamento (packageProjecaoFinanceira + packagePlanejamentoDre) | Database Engineer + Backend Engineer | P5-T02 | Endpoints de projeção e planejamento funcionando | 3h |
| P5-T08 | Implementar Ajustes Financeiros (AlteraRegistro, AlteraRegistroMovimento, AlteraStatus, GetAjusteLancamentos) | Database Engineer + Backend Engineer | P5-T02 | CRUD de ajustes funcional | 2h |
| P5-T09 | Implementar Portador (GetPortadores, GetSaldoPortadorDia, SetSaldoInicial) | Database Engineer + Backend Engineer | P5-T02 | Endpoints de portador funcionando | 2h |
| P5-T10 | Criar `IFinanceiroService.cs` / `FinanceiroService.cs` — orquestra pós-processamento e lógicas de negócio | Backend Engineer | P5-T02..09 | Lógica separada dos repositórios | 3h |
| P5-T11 | Criar `FinanceiroEndpoints.cs` — todos os endpoints financeiros da seção 13.4 do doc_legado | Backend Engineer | P5-T10 | Todos os endpoints GET /api/v1/financeiro/* implementados | 4h |
| P5-T12 | Testes de integração para módulo financeiro | QA Engineer | P5-T11 | Testes passando (posição financeira, fluxo de caixa, DRE) | 4h |
| P5-T13 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P5-T12 | Arquivos de controle atualizados | 15min |

---

### FASE 6 — MÓDULO PRAZO MÉDIO (P2) (Agente: Database Engineer + Backend Engineer)

**Critério de saída**: 3 níveis de drill-down de prazo médio para recebimento e pagamento funcionando.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P6-T01 | Criar entidades PrazoMedio: PrazoMedioMensal, PrazoMedioPessoa, PrazoMedioDocumentos | Database Engineer | P1-T01 | Classes tipadas para 3 níveis de drill-down | 2h |
| P6-T02 | Criar `IPrazoMedioRepository.cs` / `PrazoMedioRepository.cs` — 6 funções table-valued (`FN_PRAZO_MEDIO_*`) | Database Engineer | P6-T01 | Todos os 6 métodos mapeados com Dapper | 4h |
| P6-T03 | Implementar os 7 tipos de operação PZM Pagamento (G, I, O, U, L, M, S) como enum ou constante | Database Engineer | P6-T02 | Operações corretamente filtradas | 1h |
| P6-T04 | Criar `PrazoMedioEndpoints.cs` — todos os endpoints da seção 13.4 do doc_legado para Prazo Médio | Backend Engineer | P6-T02..03 | 8 endpoints de prazo médio implementados | 3h |
| P6-T05 | Testes de integração para Prazo Médio | QA Engineer | P6-T04 | Testes passando (drill-down de 3 níveis) | 2h |
| P6-T06 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P6-T05 | Arquivos de controle atualizados | 15min |

---

### FASE 7 — MÓDULO VENDAS (P2) (Agente: Database Engineer + Backend Engineer)

**Critério de saída**: Análise de vendas, ranking de clientes e plano de vendas implementados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P7-T01 | Criar entidades de Vendas: AnaliseVendas, RankingClientes, PlanoVendasResultado, ComercialMI | Database Engineer | P1-T01 | Classes tipadas para módulo vendas | 2h |
| P7-T02 | Criar `IVendasRepository.cs` / `VendasRepository.cs` — mapear `DaoVendas` completo | Database Engineer | P7-T01 | Todos os métodos de DaoVendas mapeados | 4h |
| P7-T03 | Implementar pós-processamento de Comercial MI (remoção de colunas diferença, colunas decimais zeradas, última linha de total) | Backend Engineer | P7-T02 | Pós-processamento na camada de serviço, não no repositório | 2h |
| P7-T04 | Criar `VendasEndpoints.cs` — todos os endpoints de vendas da seção 13.4 | Backend Engineer | P7-T02..03 | Endpoints GET /api/v1/vendas/* implementados | 3h |
| P7-T05 | Testes de integração para módulo de vendas | QA Engineer | P7-T04 | Testes passando | 2h |
| P7-T06 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P7-T05 | Arquivos de controle atualizados | 15min |

---

### FASE 8 — MÓDULOS SECUNDÁRIOS (P2/P3) (Agente: Database Engineer + Backend Engineer)

**Critério de saída**: Agrícola, Calendário, Agrupamentos DRE e DBA implementados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P8-T01 | Criar `IAgricolaRepository.cs` / `AgricolaRepository.cs` — ComprasFrutas (3 procedures) | Database Engineer | P1-T01 | Endpoints /api/v1/agricola/* funcionando | 3h |
| P8-T02 | Criar `ICalendarioRepository.cs` / `CalendarioRepository.cs` — GetDatas, Insere, Update + lógica de cores | Database Engineer | P1-T01 | CRUD de calendário com cores (vermelho/azul/amarelo) | 3h |
| P8-T03 | Criar `IAgrupamentoRepository.cs` / `AgrupamentoRepository.cs` — CRUD AgrupamentoDre | Database Engineer | P1-T01 | CRUD de agrupamentos funcionando | 2h |
| P8-T04 | Criar `IDbaRepository.cs` / `DbaRepository.cs` — GetSessoes, MatarSessao, GetLocks | Database Engineer | P1-T01 | Endpoints /api/v1/dba/* (admin only) funcionando | 2h |
| P8-T05 | Criar endpoints para todos os módulos P8 (Agrícola, Calendário, Agrupamento, DBA) | Backend Engineer | P8-T01..04 | Todos os endpoints da seção 13.4 restantes implementados | 4h |
| P8-T06 | Testes de integração para módulos secundários | QA Engineer | P8-T05 | Testes passando | 2h |
| P8-T07 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P8-T06 | Arquivos de controle atualizados | 15min |

---

### FASE 9 — WORKER SERVICE (P1) (Agente: DevOps Engineer + Backend Engineer)

**Critério de saída**: Worker Service rodando tarefas agendadas equivalentes ao Windows Service legado.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P9-T01 | Analisar `Treis.Service/srvPrincipal.cs` do legado para mapear todas as tarefas agendadas | Backend Engineer | - | Lista completa de tarefas e intervalos do Windows Service legado | 1h |
| P9-T02 | Implementar `Empresa.Worker/Worker.cs` herdando `BackgroundService` | Backend Engineer | P9-T01 | Worker executa sem exceção | 2h |
| P9-T03 | Configurar agendamento com `PeriodicTimer` para cada tarefa identificada | Backend Engineer | P9-T02 | Tarefas executadas nos intervalos corretos | 2h |
| P9-T04 | Adicionar health check ao Worker Service | DevOps Engineer | P9-T02 | GET /health retorna status do Worker | 1h |
| P9-T05 | Configurar injeção de dependência no Worker (acesso a repositórios) | Backend Engineer | P9-T02 | Worker tem acesso ao banco via DI | 1h |
| P9-T06 | Testes do Worker Service (mock do timer, validar que jobs executam) | QA Engineer | P9-T03..05 | Testes passando | 2h |
| P9-T07 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P9-T06 | Arquivos de controle atualizados | 15min |

---

### FASE 10 — QUALIDADE E TESTES (P1) (Agente: QA Engineer)

**Critério de saída**: Cobertura de testes ≥ 60%, testes de integração para todos os módulos críticos.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P10-T01 | Criar projeto `Empresa.Tests` (xUnit) se não existir | QA Engineer | P0-T04 | `dotnet test` roda sem erros | 1h |
| P10-T02 | Configurar mocking de `IDbConnection` para testes de repositórios | QA Engineer | P10-T01 | Repositórios testados sem banco real | 2h |
| P10-T03 | Testes unitários para todos os Services (UsuarioService, AuthService, FinanceiroService, ComprasService) | QA Engineer | P10-T02 | Cobertura dos cenários felizes e infelizes | 6h |
| P10-T04 | Testes de integração — validar 1 cenário feliz por módulo com Oracle real (ou Docker Oracle) | QA Engineer | P10-T02 | Integração Oracle validada | 4h |
| P10-T05 | Testes de segurança: endpoint sem JWT retorna 401, com JWT inválido retorna 401, com permissão errada retorna 403 | QA Engineer | Fase 2 | Autorização testada em todos os endpoints | 3h |
| P10-T06 | Testes de performance: endpoints respondem em < 2s para cenários normais | QA Engineer | Todas as fases | Baseline de performance registrada | 2h |
| P10-T07 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P10-T06 | Cobertura ≥ 60% registrada | 15min |

---

### FASE 11 — DOCUMENTAÇÃO E SWAGGER (P1) (Agente: Backend Engineer + Architect)

**Critério de saída**: Todos os endpoints documentados no Swagger, README atualizado.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P11-T01 | Configurar Swagger com título, versão, descrição e autenticação JWT | Backend Engineer | P0-T02 | Swagger UI acessível e funcional | 1h |
| P11-T02 | Adicionar `[SwaggerOperation]` ou `.WithSummary()` + `.WithDescription()` em cada endpoint | Backend Engineer | Todas as fases | Zero endpoint sem descrição | 4h |
| P11-T03 | Documentar parâmetros de query, path e body com exemplos | Backend Engineer | P11-T02 | Swagger mostra exemplos válidos | 3h |
| P11-T04 | Atualizar `docs/README.md` com instruções de setup, execução e deploy | DevOps Engineer | Todas as fases | Desenvolvedor consegue rodar o projeto seguindo o README | 2h |
| P11-T05 | Revisar contratos em `CLAUDE.md` — atualizar se houve mudanças arquiteturais | Architect | Todas as fases | CLAUDE.md reflete o estado atual real | 1h |
| P11-T06 | Atualizar `TASKS.md` e `PROGRESS.md` finais | Architect | P11-T05 | Todos os itens marcados como concluídos | 30min |

---

### FASE 12 — DEVOPS E INFRAESTRUTURA (P2) (Agente: DevOps Engineer)

**Critério de saída**: Pipeline CI/CD, Dockerfile e deploy configurados.

| ID | Tarefa | Agente | Dependências | Critério de Saída | Esforço |
|----|--------|--------|--------------|-------------------|---------|
| P12-T01 | Criar `Dockerfile` para `Empresa.Api` (multi-stage build) | DevOps Engineer | Fase 10 | `docker build` e `docker run` sem erros | 2h |
| P12-T02 | Criar `docker-compose.yml` para desenvolvimento local (Api + Oracle) | DevOps Engineer | P12-T01 | `docker-compose up` sobe o ambiente | 2h |
| P12-T03 | Configurar pipeline CI (GitHub Actions ou Azure DevOps) — build + test em cada PR | DevOps Engineer | Fase 10 | Pipeline executa automaticamente | 3h |
| P12-T04 | Configurar pipeline CD — deploy em ambiente de homologação | DevOps Engineer | P12-T03 | Deploy automático em merge para main | 3h |
| P12-T05 | Configurar variáveis de ambiente por ambiente (dev, hom, prod) | DevOps Engineer | P12-T03 | Sem segredos no repositório | 2h |
| P12-T06 | Atualizar `TASKS.md` e `PROGRESS.md` | Architect | P12-T05 | Arquivos de controle atualizados | 15min |

---

## 🚀 ETAPA 5 — ATUALIZAÇÃO DOS ARTEFATOS DE CONTROLE

### 5.1 Após gerar o PRD completo, execute:

**Agente: Architect**

1. **Atualizar `TASKS.md`**: Substituir o conteúdo atual pelo roadmap detalhado do PRD, com todas as fases e tarefas no formato de checkboxes. Manter a estrutura existente e adicionar as novas fases.

2. **Atualizar `PROGRESS.md`**: Registrar:
   - Data da análise de diagnóstico
   - Score de conformidade encontrado
   - Lacunas críticas identificadas
   - Próxima tarefa recomendada (primeira tarefa da Fase 0)
   - Perguntas em aberto identificadas durante a análise

3. **Salvar o PRD**: Criar `docs/PRD_MODERNIZACAO_GESTAO_NEW.md` com todo o conteúdo gerado nesta análise.

---

## ⚠️ REGRAS DE OURO PARA EXECUÇÃO DESTE PROMPT

1. **NUNCA pule a leitura dos artefatos** (CLAUDE.md, PROGRESS.md, TASKS.md, doc_legado.md) antes de analisar
2. **NUNCA altere o banco Oracle** — apenas a camada de aplicação é modernizada
3. **SEMPRE identifique o agente correto** para cada tarefa (architect, backend, database, qa, devops)
4. **SEMPRE atualize** TASKS.md e PROGRESS.md ao final de cada fase
5. **Se algo não estiver claro**, registre como "Pergunta em Aberto" no PROGRESS.md em vez de assumir
6. **O PRD deve ser gerado em português** seguindo o padrão do projeto
7. **Cada tarefa do PRD deve ter critério de saída objetivo** — algo testável ou verificável
8. **O Architect é o coordenador** — todas as decisões de quebra de contrato passam por ele
9. **Confira a seção 13.4 do doc_legado.md** para garantir que 100% dos endpoints REST estão mapeados no PRD
10. **Confira a seção 14 do doc_legado.md** (Checklist de Verificação para IA) — todos os itens devem constar no PRD

---

## 📊 FORMATO DE SAÍDA ESPERADO

Ao final da execução deste prompt, devem existir:

| Arquivo | Conteúdo |
|---------|----------|
| `docs/PRD_MODERNIZACAO_GESTAO_NEW.md` | PRD completo com diagnóstico, gap analysis, score de conformidade e roadmap |
| `TASKS.md` (atualizado) | Backlog completo com todas as fases e tarefas do PRD |
| `PROGRESS.md` (atualizado) | Score de conformidade, lacunas críticas, próxima tarefa, perguntas em aberto |

**Score de conformidade mínimo aceitável para iniciar o desenvolvimento**: 80% em Estrutura Arquitetural
**Score atual esperado (com base na análise inicial)**: ~ 15% (apenas scaffold criado)

---

*Este prompt foi gerado automaticamente pela análise da estrutura do projeto GestaoNew em 28/07/2026.*
*Baseia-se nos artefatos: `CLAUDE.md`, `TASKS.md`, `PROGRESS.md`, `docs/doc_legado.md` do projeto GestaoNew.*
