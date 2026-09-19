# PRD — Frontend: Módulo de Usuários e Perfis (GestãoUsuarios)

| Campo | Valor |
|-------|-------|
| **Título** | Módulo de Usuários e Perfis de Acesso |
| **Projeto** | GestãoUsuarios |
| **Versão do PRD** | 3.0.0 |
| **Data** | 2026-07-30 |
| **Status** | Em andamento |
| **Fase atual** | 2 — Cadastro de Usuários (em andamento) |
| **Multi-Agent?** | Sim — Orquestrado via `agents/orchestrator.md` |
| **Agentes Ativos** | `orchestrator`, `architect`, `frontend-engineer`, `backend-engineer`, `database-engineer`, `qa-engineer`, `devops-engineer` |

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
│ FASE 1: DESIGN & ARQUITETURA                                │
│ ┌─────────────┐                                              │
│ │ Architect   │                                              │
│ │ Agent       │                                              │
│ │ - Estrutura │                                              │
│ │ - Tipos     │                                              │
│ │ - Padrões   │                                              │
│ └─────────────┘                                              │
└─────────────────────┬───────────────────────────────────────┘
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 2: IMPLEMENTAÇÃO                                       │
│ ┌─────────────┐  ┌──────────────┐  ┌──────────────┐        │
│ │ Backend     │  │ Frontend     │  │ Database     │        │
│ │ Engineer    │  │ Engineer     │  │ Engineer     │        │
│ │ Agent       │  │ Agent       │  │ Agent        │        │
│ │             │  │             │  │              │        │
│ │ - Services  │  │ - Components│  │ - Procedures │        │
│ │ - Endpoints │  │ - Stores    │  │ - Queries    │        │
│ │ - DTOs      │  │ - Services  │  │              │        │
│ └──────┬──────┘  └──────┬───────┘  └──────┬───────┘        │
│        └────────────────┼─────────────────┘                │
└─────────────────────────┼──────────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 3: QUALIDADE & TESTES                                  │
│ ┌──────────────────────────────┐                            │
│ │       QA Engineer Agent      │                            │
│ │  - Testes unitários          │                            │
│ │  - Testes de integração      │                            │
│ │  - Cobertura ≥ 80%           │                            │
│ │  - Bug reports               │                            │
│ └──────────────┬───────────────┘                            │
└────────────────┼────────────────────────────────────────────┘
                 ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 4: REFINAMENTO & REVIEW                                │
│ ┌─────────────┐  ┌──────────────┐                           │
│ │ Architect   │  │ Frontend     │                           │
│ │ Agent       │  │ Engineer     │                           │
│ │             │  │ Agent        │                           │
│ │ - Code      │  │ - Ajustes    │                           │
│ │   Review    │  │   visuais    │                           │
│ │ - Docs      │  │ - Loading    │                           │
│ │             │  │ - Erros      │                           │
│ └──────┬──────┘  └──────┬───────┘                           │
│        └────────┬───────┘                                   │
└─────────────────┼──────────────────────────────────────────┘
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 5: ENTREGA                                             │
│ ┌──────────────────────────────┐                            │
│ │    DevOps Engineer Agent     │                            │
│ │  - Build de produção         │                            │
│ │  - Validação final           │                            │
│ │  - Deploy                    │                            │
│ └──────────────────────────────┘                            │
└─────────────────────────────────────────────────────────────┘
```

### 0.2 Catálogo de Agentes

| Agente | Arquivo | Função Principal |
|--------|---------|-----------------|
| **Orchestrator** | `agents/orchestrator.md` | Coordenação, distribuição de tarefas e sequenciamento |
| **Architect** | `agents/architect.md` | Design de arquitetura, padrões, revisão de PRD |
| **Frontend Engineer** | `agents/frontend-engineer.md` | Componentes Vue 3, composables, stores, integração |
| **Backend Engineer** | `agents/backend-engineer.md` | Implementação de APIs, services, endpoints |
| **Database Engineer** | `agents/database-engineer.md` | Modelagem de dados, queries, procedures Oracle |
| **QA Engineer** | `agents/qa-engineer.md` | Testes unitários, integração, cobertura |
| **DevOps Engineer** | `agents/devops-engineer.md` | CI/CD, Docker, deploy, infraestrutura |

### 0.3 Fluxo de Orquestração

```
1. Orchestrator lê PRD → extrai RFs
2. Orchestrator cria/atualiza TASKS.md com tasks atômicas
3. Orchestrator delega tasks por fase:
   a. Fase 1 → architect
   b. Fase 2 → backend-engineer, frontend-engineer, database-engineer
   c. Fase 3 → qa-engineer
   d. Fase 4 → frontend-engineer + architect
   e. Fase 5 → devops-engineer
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

Página de gestão de usuários e perfis de acesso. O sistema exibe uma listagem pesquisável de usuários (com filtros) e, ao selecionar um registro, abre o formulário de cadastro completo onde o operador pode editar dados pessoais, definir o perfil de acesso e vincular os estabelecimentos (lojas) que o usuário poderá acessar.

### 1.2 Objetivos SMART

- **Específico**: Fornecer uma interface única para cadastrar/editar usuários, atribuir perfis, gerenciar permissões por página e controlar o vínculo com estabelecimentos.
- **Mensurável**:
  - Tempo de cadastro/edição de um usuário < 2 minutos.
  - Cobertura de testes (unitários + integração) ≥ 80%.
  - Zero erros de validação escapando para produção (campos obrigatórios, email inválido, senha fraca).
- **Atingível**: APIs backend já fornecem todos os endpoints necessários; o trabalho concentra-se na camada de frontend (Vue 3 + TypeScript) e na integração.
- **Relevante**: O módulo substitui o legado ASP.NET WebForms (GestaoUsuarios), centralizando a administração de acessos.
- **Temporal**: Entrega da Fase 2 (Cadastro de Usuários — frontend) prevista para o ciclo atual de desenvolvimento.

### 1.3 Escopo

#### Dentro do escopo
- Tela de listagem de usuários com pesquisa e filtros
- Tela de cadastro/edição de usuário com formulário completo
- Seleção de perfil via dropdown
- Tree de páginas com checkboxes para permissões customizadas do perfil
- Tree de estabelecimentos com checkboxes para vínculo do usuário
- Validação de campos obrigatórios, formato de email e complexidade de senha
- Integração com os endpoints REST já existentes
- Testes unitários para componentes, composables e stores

#### Fora do escopo
- Criação/edição de perfis em tela separada (módulo de perfis puro)
- Criação/edição de páginas (gestão de menus)
- Criação/edição de estabelecimentos (cadastro de lojas)
- Migração de dados do legado
- Deploy em produção

## 2. Arquitetura & Diretrizes Técnicas

### 2.1 Stack Tecnológica

| Camada | Tecnologia |
|--------|-----------|
| Framework | Vue 3 (Composition API + `<script setup>`) |
| Linguagem | TypeScript (strict) |
| Build | Vite |
| UI Framework | PrimeVue 4 |
| Roteador | Vue Router 4 |
| Gerenciamento de estado | Pinia |
| Requisições HTTP | Axios |
| Testes | Vitest + Vue Test Utils |

### 2.2 Estrutura de Diretórios (Módulo)

```
src/
├── modules/
│   └── usuarios/
│       ├── components/
│       │   ├── UsuarioLista.vue
│       │   ├── UsuarioForm.vue
│       │   ├── PerfilSelect.vue
│       │   ├── PaginaTree.vue
│       │   └── EstabelecimentoTree.vue
│       ├── composables/
│       │   ├── useUsuarios.ts
│       │   ├── usePerfis.ts
│       │   ├── usePaginasTree.ts
│       │   └── useEstabelecimentoTree.ts
│       ├── stores/
│       │   └── usuarioStore.ts
│       ├── services/
│       │   ├── usuarioService.ts
│       │   ├── perfilService.ts
│       │   ├── paginaService.ts
│       │   └── estabelecimentoService.ts
│       ├── types/
│       │   └── index.ts
│       ├── views/
│       │   └── UsuarioPage.vue
│       └── router.ts
```

### 2.3 Componentes e Responsabilidades

| Componente | Responsabilidade |
|-----------|-----------------|
| `UsuarioPage.vue` | View principal — layout master-detail (lista + formulário) |
| `UsuarioLista.vue` | Tabela de usuários com pesquisa, filtros e paginação |
| `UsuarioForm.vue` | Formulário de cadastro/edição com todas as abas |
| `PerfilSelect.vue` | Dropdown de seleção de perfil |
| `PaginaTree.vue` | Tree de páginas com checkboxes para permissões |
| `EstabelecimentoTree.vue` | Tree de estabelecimentos com checkboxes para vínculo |

### 2.4 Design System (PrimeVue)

| Elemento | Componente PrimeVue |
|----------|-------------------|
| DataTable (listagem) | `DataTable` + `Column` + filtros |
| Formulário | `FloatLabel` + `InputText` / `Password` / `InputMask` + `ToggleSwitch` |
| Dropdowns | `Select` (com `optionLabel`) |
| Trees | `Tree` com `selectionMode="checkbox"` |
| Abas (form) | `Tabs` + `TabPanel` |
| Botões | `Button` (severity: success/danger/info) |
| Diálogos | `Dialog` + `ConfirmDialog` |
| Feedback | `Toast` + `Message` (inline validation) |
| Loading | `ProgressSpinner` / `Skeleton` |

## 3. Requisitos Funcionais Detalhados

### RF01 — Listagem de Usuários com Pesquisa e Filtros

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer` (UI) + `backend-engineer` (suporte à integração)

- **RF01.1** — A tabela deve exibir as colunas: ID, Nome, Email, Ativo, Perfil, Ações.
- **RF01.2** — Deve haver um campo de pesquisa textual (nome ou email) com debounce de **300ms**.
- **RF01.3** — Filtro por situação: **Todos**, **Ativos**, **Inativos**.
- **RF01.4** — Filtro por perfil: dropdown com ID do perfil (carregar lista de perfis).
- **RF01.5** — Paginação server-side: parâmetros `page`, `pageSize`, `orderBy`.
- **RF01.6** — Ordenação por colunas clicáveis (Nome, Email, Perfil, Ativo).
- **RF01.7** — A tabela deve ser responsiva (horizontal scroll em telas pequenas).
- **RF01.8** — Coluna "Ações" com botões: **Editar** (ícone lápis) e **Excluir** (ícone lixeira, com confirmação).

**Endpoint**: `GET /api/usuarios?search=&situacao=&perfilId=&page=&pageSize=&orderBy=`

### RF02 — Cadastro e Edição de Usuário (Formulário)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer` (UI) + `backend-engineer` (suporte à integração)

O formulário deve ser estruturado em **3 abas** (`Tabs`):

#### Aba 1: Dados Pessoais

- **RF02.1** — Campo `nome` (string, obrigatório, max 100 chars).
- **RF02.2** — Campo `email` (string, obrigatório, validação de formato email).
- **RF02.3** — Campo `senha` (string, obrigatório ao criar, opcional ao editar). Validação de complexidade:
  - Mínimo 8 caracteres
  - Pelo menos 1 letra maiúscula
  - Pelo menos 1 número
  - Pelo menos 1 caractere especial
- **RF02.4** — Campo `ativo` (toggle switch, default true).
- **RF02.5** — Ao editar, pré-preencher todos os campos com os dados existentes.
- **RF02.6** — Botão "Salvar" que submete o formulário com validação completa.
- **RF02.7** — Botão "Cancelar" que retorna à listagem.

#### Aba 2: Perfil e Permissões

- **RF02.8** — Dropdown `PerfilSelect` para selecionar o perfil do usuário.
  - `GET /api/perfis` para carregar opções.
  - Exibir: `id` e `descricao`.
- **RF02.9** — Tree `PaginaTree` com checkboxes exibindo as permissões do perfil.
  - `GET /api/paginas/perfil/{perfilId}` para carregar as permissões do perfil selecionado.
  - `GET /api/paginas/tree` para carregar a árvore completa de páginas.
  - Exibir `nome` de cada página.
  - Checkboxes somente leitura (as permissões pertencem ao perfil, não ao usuário diretamente).

#### Aba 3: Vínculo com Estabelecimentos

- **RF02.10** — Tree `EstabelecimentoTree` com checkboxes para selecionar os estabelecimentos que o usuário pode acessar.
  - `GET /api/estabelecimentos/tree` para carregar a árvore completa.
  - Ao editar, pré-selecionar os estabelecimentos já vinculados (`GET /api/usuarios/{id}/estabelecimentos`).
  - Checkboxes editáveis.
  - Nós pais não selecionáveis automaticamente (seleção independente por nó).

**Endpoint principal**: `POST /api/usuarios` (criar) | `PUT /api/usuarios/{id}` (editar)
**Body**:
```json
{
  "nome": "string",
  "email": "string",
  "senha": "string",
  "ativo": true,
  "perfilId": 0,
  "estabelecimentos": [1, 2, 3]
}
```

### RF03 — Seleção de Perfil

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer`

- **RF03.1** — Componente `PerfilSelect.vue`: dropdown que carrega perfis via `GET /api/perfis`.
- **RF03.2** — Exibir `id` e `descricao` no dropdown.
- **RF03.3** — Loading state enquanto carrega.
- **RF03.4** — Emitir evento `update:modelValue` com o ID do perfil selecionado.
- **RF03.5** — Reutilizável em outros módulos.

### RF04 — Tree de Páginas (Permissões)

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer`

- **RF04.1** — Componente `PaginaTree.vue`: exibe a árvore de páginas com checkboxes.
- **RF04.2** — `GET /api/paginas/tree` para carregar a estrutura completa.
- **RF04.3** — Ao selecionar um perfil (`@watch perfilId`), carregar automaticamente as permissões do perfil via `GET /api/paginas/perfil/{perfilId}`.
- **RF04.4** — Marcar os checkboxes correspondentes às páginas permitidas ao perfil.
- **RF04.5** — Checkboxes em modo somente leitura (disabled).
- **RF04.6** — Suportar hierarquia de múltiplos níveis.
- **RF04.7** — Expandir todos os nós por padrão.

### RF05 — Tree de Estabelecimentos

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer`

- **RF05.1** — Componente `EstabelecimentoTree.vue`: exibe a árvore de estabelecimentos com checkboxes.
- **RF05.2** — `GET /api/estabelecimentos/tree` para carregar a estrutura completa.
- **RF05.3** — Ao editar um usuário, carregar os estabelecimentos já vinculados via `GET /api/usuarios/{id}/estabelecimentos`.
- **RF05.4** — Checkboxes editáveis (seleção manual).
- **RF05.5** — Nós pais não devem ser automaticamente selecionados ao selecionar um nó filho.
- **RF05.6** — Suportar hierarquia de múltiplos níveis.
- **RF05.7** — Expandir todos os nós por padrão.

### RF06 — Validações

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer` (implementação) + `qa-engineer` (testes de validação)

- **RF06.1** — Validação no frontend **antes** do submit (não depender apenas do backend).
- **RF06.2** — Mensagens de erro em português (ex: "Nome é obrigatório.", "Email inválido.").
- **RF06.3** — Validação de senha com indicador visual de força (barra de progresso).
- **RF06.4** — Validações do backend devem ser exibidas como toast/mensagem de erro.
- **RF06.5** — Campos inválidos devem receber borda vermelha e mensagem abaixo.

### RF07 — Integração com Backend

**Prioridade**: P0 (Essencial)
**Orquestrador**: `orchestrator` → delega para `frontend-engineer` (services e interceptors) + `backend-engineer` (suporte)

- **RF07.1** — Services com Axios, organizados por domínio.
- **RF07.2** — Interceptor para tratamento global de erros (401, 403, 500).
- **RF07.3** — Interceptor para injeção automática do token JWT.
- **RF07.4** — Tipagem TypeScript para todas as requests/responses.
- **RF07.5** — Tratamento de loading states em todas as operações assíncronas.
- **RF07.6** — Tratamento de erros com mensagens amigáveis.

### RF08 — Testes

**Prioridade**: P1 (Importante)
**Orquestrador**: `orchestrator` → delega para `qa-engineer`

#### Testes Unitários
- **RF08.1** — Testar todos os composables (useUsuarios, usePerfis, usePaginasTree, useEstabelecimentoTree).
- **RF08.2** — Testar a store (usuarioStore) — ações, getters e mutations.
- **RF08.3** — Testar validações de formulário (regras, mensagens de erro).
- **RF08.4** — Testar serviços com mock do Axios.

#### Testes de Componente
- **RF08.5** — Testar renderização condicional (loading, empty, error states).
- **RF08.6** — Testar eventos de clique (selecionar usuário, abrir form, salvar, cancelar).
- **RF08.7** — Testar emissão de eventos pelos componentes filhos.

#### Testes de Integração
- **RF08.8** — Testar fluxo completo: listar → selecionar → editar → salvar.
- **RF08.9** — Testar fluxo: criar novo usuário → preencher → salvar → aparece na lista.
- **RF08.10** — Testar cenários de erro: API fora, timeout, 500.

## 4. Endpoints da API (Referência)

### 4.1 Usuários

| Método | Endpoint | Descrição |
|--------|---------|-----------|
| `GET` | `/api/usuarios?search=&situacao=&perfilId=&page=&pageSize=&orderBy=` | Listar usuários |
| `GET` | `/api/usuarios/{id}` | Obter usuário por ID |
| `POST` | `/api/usuarios` | Criar usuário |
| `PUT` | `/api/usuarios/{id}` | Atualizar usuário |
| `DELETE` | `/api/usuarios/{id}` | Excluir usuário |
| `GET` | `/api/usuarios/{id}/estabelecimentos` | Obter estabelecimentos vinculados |

### 4.2 Perfis

| Método | Endpoint | Descrição |
|--------|---------|-----------|
| `GET` | `/api/perfis` | Listar perfis |

### 4.3 Páginas

| Método | Endpoint | Descrição |
|--------|---------|-----------|
| `GET` | `/api/paginas/tree` | Obter árvore completa de páginas |
| `GET` | `/api/paginas/perfil/{perfilId}` | Obter páginas permitidas para um perfil |

### 4.4 Estabelecimentos

| Método | Endpoint | Descrição |
|--------|---------|-----------|
| `GET` | `/api/estabelecimentos/tree` | Obter árvore completa de estabelecimentos |

## 5. Métricas de Aceite (Definition of Done)

- [ ] Todas as funcionalidades RF01 a RF08 implementadas e testadas.
- [ ] Cobertura de testes ≥ 80% (unitários).
- [ ] Zero erros de console (exceções não tratadas).
- [ ] Build de produção (Vite) sem erros.
- [ ] Tipagem TypeScript em todos os componentes, composables e services.
- [ ] Responsivo (testado em 1920px, 1366px, 768px).
- [ ] Aprovado em code review.
- [ ] Documentação de componente atualizada (Storybook ou similar — opcional nesta fase).

## 6. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|-------------|---------|-----------|
| API de estabelecimentos não retornar tree conforme esperado | Média | Alto | Validar estrutura da resposta antes de integrar; mockar dados para desenvolvimento |
| Performance da tree com muitos registros | Baixa | Médio | Lazy loading nos nós da tree; paginação dos dados se necessário |
| Complexidade de senha conflitar com regras legadas | Média | Baixo | Alinhar regras de validação com o backend; documentar regras |
| PrimeVue Tree não suportar seleção independente de nós pais/filhos | Baixa | Médio | Implementar lógica customizada nos eventos `@nodeSelect` / `@nodeUnselect` |

## 7. Plano de Execução (Orquestrado)

| Fase | Descrição | Agentes | Status |
|------|-----------|---------|--------|
| Fase 1 | Estrutura base (tipos, services, store, router) | `orchestrator` → `architect` | ✅ Concluída |
| Fase 2 | Componentes: listagem, formulário, selects, trees, validações, integração backend | `orchestrator` → `frontend-engineer` + `backend-engineer` | ✅ Concluída |
| Fase 3 | Testes unitários, de componente e de integração | `orchestrator` → `qa-engineer` | 🔄 Em andamento |
| Fase 4 | Refinamento visual, responsividade, tratamento de erros | `orchestrator` → `frontend-engineer` + `architect` | ⏳ Pendente |
| Fase 5 | Code review, documentação, build e deploy | `orchestrator` → `architect` + `devops-engineer` | ⏳ Pendente |

### 7.1 Sequenciamento de Delegação por Fase

```
FASE 1 ──── architect ────► FASE 2
                                │
                    ┌───────────┼───────────┐
                    │           │           │
              frontend    backend      database
              engineer    engineer     engineer
                    │           │           │
                    └───────────┼───────────┘
                                ▼
                            FASE 3
                                │
                          qa-engineer
                                │
                                ▼
                            FASE 4
                                │
                    ┌───────────┼───────────┐
                    │                       │
              frontend-engineer       architect
                    │                       │
                    └───────────┬───────────┘
                                ▼
                            FASE 5
                                │
                    ┌───────────┼───────────┐
                    │                       │
              architect              devops-engineer
```

## 8. Aprovações

| Papel | Nome | Data | Assinatura |
|-------|------|------|-----------|
| PO | | | |
| Tech Lead | | | |
| QA | | | |