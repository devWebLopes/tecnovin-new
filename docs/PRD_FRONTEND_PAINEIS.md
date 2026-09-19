# PRD — Frontend dos Painéis GestaoNew
## Versão: 1.0 | Data: 29/07/2026 | Status: RASCUNHO

---

### 1. Resumo Executivo

#### Objetivo
Criar um frontend web moderno para o sistema GestaoNew, iniciando pela tela de login e evoluindo progressivamente para todos os painéis de dashboard do sistema. O frontend consumirá a API REST já implementada no backend (`Empresa.Api`).

#### Stack Tecnológica Proposta

| Camada | Tecnologia | Justificativa |
|--------|-----------|---------------|
| **Framework** | React 18+ com TypeScript | Ecossistema maduro, tipagem forte, ampla adoção |
| **Build Tool** | Vite 6 | Build rápido, HMR instantâneo, otimizado para React |
| **UI Component Library** | Material UI (MUI) v6 + ou Shadcn/ui | Componentes prontos para dashboards, temas customizáveis, acessibilidade |
| **Gráficos** | Recharts + ApexCharts | Gráficos responsivos, suporte a drill-down, séries temporais |
| **Estado Global** | React Query (TanStack Query) + Zustand | Cache de API, refetch automático, estado global leve |
| **Roteamento** | React Router v7 | Rotas aninhadas, lazy loading, guardas de autenticação |
| **Formulários** | React Hook Form + Zod | Validação tipada, performática, schemas reutilizáveis |
| **Requisições HTTP** | Axios + interceptors | Token JWT automático, refresh token, tratamento de erros |
| **Data Grid** | MUI Data Grid ou AG Grid | Tabelas com filtro, ordenação, exportação, virtual scrolling |
| **Notificações** | Sonner + React Hot Toast | Toast notifications leves e customizáveis |
| **Data Context** | React Query com WebSocket | Dados frescos em tempo real para painéis críticos |

#### Escopo Geral

O frontend será construído em **sprints incrementais**, começando pela estrutura base + tela de login e evoluindo módulo a módulo:

| Sprint | Módulo | Prioridade |
|--------|--------|:----------:|
| **Sprint 1** | Login + Layout Base + Sidebar + Guards | P0 |
| **Sprint 2** | Dashboard Home + Menu Dinâmico | P0 |
| **Sprint 3** | Módulo Usuários/Perfis/Páginas (CRUD) | P0 |
| **Sprint 4** | Módulo Compras (Painéis) | P1 |
| **Sprint 5** | Módulo Financeiro (Posição, Fluxo, DRE) | P1 |
| **Sprint 6** | Módulo Prazo Médio (Drill-down) | P1 |
| **Sprint 7** | Módulo Vendas (Análise, Ranking) | P2 |
| **Sprint 8** | Módulos Secundários (Agrícola, Calendário, DBA) | P2 |
| **Sprint 9** | Relatórios + Exportação + Quality Pass | P2 |

---

### 2. Arquitetura Frontend

#### 2.1. Estrutura de Diretórios

```
GestaoNew.Frontend/
├── public/
│   ├── favicon.svg
│   ├── logo.svg
│   └── manifest.json
├── src/
│   ├── main.tsx                          # Entry point
│   ├── App.tsx                           # Root component (providers)
│   ├── vite-env.d.ts
│   │
│   ├── api/                              # Camada de API
│   │   ├── client.ts                     # Axios instance + interceptors
│   │   ├── auth.api.ts                   # Endpoints de autenticação
│   │   ├── usuario.api.ts               # Endpoints de usuários
│   │   ├── pagina.api.ts                # Endpoints de páginas/menu
│   │   ├── perfil.api.ts                # Endpoints de perfis
│   │   ├── estabelecimento.api.ts       # Endpoints de estabelecimentos
│   │   ├── compras.api.ts               # Endpoints de compras
│   │   ├── financeiro.api.ts            # Endpoints de financeiro
│   │   ├── prazo-medio.api.ts            # Endpoints de prazo médio
│   │   ├── vendas.api.ts                # Endpoints de vendas
│   │   ├── secundarios.api.ts           # Endpoints secundários
│   │   └── types/                       # Tipos compartilhados da API
│   │       ├── auth.types.ts
│   │       ├── usuario.types.ts
│   │       ├── pagina.types.ts
│   │       ├── perfil.types.ts
│   │       ├── estabelecimento.types.ts
│   │       ├── compras.types.ts
│   │       ├── financeiro.types.ts
│   │       ├── prazo-medio.types.ts
│   │       ├── vendas.types.ts
│   │       └── api.types.ts             # Tipos genéricos (Pagination, Response, etc.)
│   │
│   ├── hooks/                            # Custom hooks
│   │   ├── useAuth.ts                    # Hook de autenticação
│   │   ├── useMenu.ts                    # Hook do menu dinâmico
│   │   ├── usePagination.ts             # Hook de paginação
│   │   └── useDebounce.ts               # Hook de debounce para inputs
│   │
│   ├── contexts/                         # Contextos React
│   │   └── AuthContext.tsx               # Contexto de autenticação (JWT + claims)
│   │
│   ├── components/                       # Componentes compartilhados
│   │   ├── ui/                           # Componentes base (Button, Input, Card, etc.)
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── DataTable.tsx            # Tabela genérica com filtros
│   │   │   ├── StatusBadge.tsx          # Badge de status (ativo/inativo)
│   │   │   ├── LoadingSpinner.tsx
│   │   │   └── EmptyState.tsx
│   │   │
│   │   ├── layout/                       # Componentes de layout
│   │   │   ├── DashboardLayout.tsx       # Layout principal (sidebar + header + content)
│   │   │   ├── Sidebar.tsx              # Sidebar com menu dinâmico
│   │   │   ├── Header.tsx              # Topbar com usuário, notificações
│   │   │   ├── Breadcrumb.tsx           # Breadcrumb dinâmico
│   │   │   └── Footer.tsx
│   │   │
│   │   ├── charts/                       # Componentes de gráficos
│   │   │   ├── BarChart.tsx
│   │   │   ├── LineChart.tsx
│   │   │   ├── PieChart.tsx
│   │   │   ├── AreaChart.tsx
│   │   │   └── DrillDownChart.tsx       # Gráfico com suporte a drill-down
│   │   │
│   │   ├── feedback/                     # Feedback components
│   │   │   ├── ConfirmDialog.tsx        # Diálogo de confirmação
│   │   │   ├── Toast.tsx               # Notificações toast
│   │   │   └── ErrorBoundary.tsx        # Boundary de erro
│   │   │
│   │   └── guards/                       # Guards de rota
│   │       ├── AuthGuard.tsx            # Redireciona para login se não autenticado
│   │       ├── RoleGuard.tsx            # Restringe acesso por perfil
│   │       └── PageAccessGuard.tsx      # Restringe acesso com base na árvore de páginas
│   │
│   ├── features/                         # Módulos/Features (cada um com suas páginas)
│   │   ├── auth/                         # Módulo de autenticação
│   │   │   ├── LoginPage.tsx            # Página de login
│   │   │   ├── LoginForm.tsx            # Formulário de login
│   │   │   └── AlterarSenhaPage.tsx     # Página de alteração de senha
│   │   │
│   │   ├── dashboard/                    # Dashboard Home
│   │   │   ├── DashboardHomePage.tsx    # Dashboard principal pós-login
│   │   │   ├── WelcomeCard.tsx
│   │   │   ├── RecentPages.tsx          # TOP 10 páginas mais acessadas
│   │   │   └── QuickActions.tsx
│   │   │
│   │   ├── usuarios/                     # Módulo de Usuários
│   │   │   ├── UsuarioListPage.tsx      # Lista de usuários
│   │   │   ├── UsuarioFormPage.tsx      # Cadastro/edição de usuário
│   │   │   └── UsuarioDetailPage.tsx    # Detalhes do usuário
│   │   │
│   │   ├── perfis/                       # Módulo de Perfis
│   │   │   ├── PerfilListPage.tsx
│   │   │   ├── PerfilFormPage.tsx
│   │   │   └── PerfilPermissoes.tsx     # Vinculação de páginas ao perfil
│   │   │
│   │   ├── paginas/                      # Módulo de Páginas (admin)
│   │   │   ├── PaginaListPage.tsx
│   │   │   ├── PaginaFormPage.tsx
│   │   │   └── PaginaTreeView.tsx       # Visualização em árvore do menu
│   │   │
│   │   ├── estabelecimentos/             # Módulo de Estabelecimentos
│   │   │   ├── EstabelecimentoListPage.tsx
│   │   │   ├── EstabelecimentoFormPage.tsx
│   │   │   └── EstabelecimentoTreeView.tsx
│   │   │
│   │   ├── compras/                      # Módulo de Compras
│   │   │   ├── ResumoAnualPage.tsx      # Painel com 4 cursores
│   │   │   ├── ComiteComprasPage.tsx
│   │   │   ├── ProgressaoPrecoPage.tsx
│   │   │   ├── CfopPage.tsx            # CRUD CFOP
│   │   │   └── CentroCustoPage.tsx      # Centro de custo
│   │   │
│   │   ├── financeiro/                   # Módulo Financeiro
│   │   │   ├── PosicaoFinanceiraPage.tsx # 3 versões (legado, new, sreal)
│   │   │   ├── FluxoCaixaPage.tsx       # Fluxo de caixa analítico
│   │   │   ├── DrePage.tsx             # DRE com 3 versões + drill-down
│   │   │   ├── ProjecaoPage.tsx
│   │   │   ├── AjustesPage.tsx
│   │   │   └── PortadorPage.tsx
│   │   │
│   │   ├── prazo-medio/                  # Módulo Prazo Médio
│   │   │   ├── PrazoMedioMensalPage.tsx
│   │   │   ├── PrazoMedioPessoaPage.tsx
│   │   │   └── PrazoMedioDocumentosPage.tsx # Drill-down (3 níveis)
│   │   │
│   │   ├── vendas/                       # Módulo Vendas
│   │   │   ├── AnaliseVendasPage.tsx
│   │   │   ├── RankingClientesPage.tsx
│   │   │   └── PlanoVendasPage.tsx
│   │   │
│   │   └── secundarios/                  # Módulos Secundários
│   │       ├── AgricolaPage.tsx
│   │       ├── CalendarioPage.tsx
│   │       ├── DbaPage.tsx             # Admin: sessões Oracle
│   │       └── AgrupamentoDrePage.tsx
│   │
│   ├── routes/                           # Configuração de rotas
│   │   ├── index.tsx                    # Definição de todas as rotas
│   │   └── menuConfig.ts               # Mapeamento rotas -> páginas do sistema
│   │
│   ├── theme/                            # Tema do sistema
│   │   ├── theme.ts                     # Tema MUI customizado (cores, tipografia, spacing)
│   │   ├── colors.ts                    # Paleta de cores do sistema
│   │   └── typography.ts               # Tipografia
│   │
│   ├── utils/                            # Utilitários
│   │   ├── format.ts                    # Formatadores (moeda, data, CNPJ, etc.)
│   │   ├── validators.ts               # Validadores (CNPJ, CPF, etc.)
│   │   ├── permissions.ts               # Utilitários de permissão
│   │   └── constants.ts                 # Constantes do sistema
│   │
│   └── styles/                           # Estilos globais
│       ├── globals.css
│       └── variables.css
│
├── index.html
├── vite.config.ts
├── tsconfig.json
├── tsconfig.node.json
├── package.json
├── .env                                  # Variáveis de ambiente
├── .env.development                      # Dev API URL
├── .env.production                       # Prod API URL
├── .eslintrc.cjs
├── .prettierrc
├── tailwind.config.js                    # Se usar Tailwind + MUI
└── README.md
```

#### 2.2. Fluxo de Autenticação

```
┌──────────┐     POST /api/v1/auth/login     ┌──────────────┐
│          │  ──────────────────────────────>  │              │
│  Login   │     { login, senha }              │  API Backend │
│  Page    │  <──────────────────────────────  │              │
│          │     { token, refreshToken,         │              │
└──────────┘       usuario: { nome, login,      └──────────────┘
                   idPerfil, idUsuario } }
    │
    │ Armazena token no localStorage
    │ (ou httpOnly cookie via refresh)
    ▼
┌──────────┐
│ Layout   │  Axios interceptor adiciona
│ Principal│  header: Authorization Bearer {token}
│          │
│          │  Se 401 → tenta refresh token
│          │  Se refresh falhar → redireciona login
└──────────┘
```

#### 2.3. Fluxo de Menu Dinâmico

```
                    ┌─────────────────────┐
                    │  GET                │
                    │  /api/v1/paginas/   │
                    │  menu               │
                    └─────────┬───────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │  Retorno: Árvore    │
                    │  hierárquica com    │
                    │  UNION (pais+filhas)│
                    └─────────┬───────────┘
                              │
                    ┌─────────▼───────────┐
                    │ Sidebar.tsx         │
                    │ Renderiza itens     │
                    │ c/ collapsible      │
                    └─────────────────────┘
```

---

### 3. Sprint 1: Login + Layout Base + Guards

#### 3.1. Tela de Login

**Descrição:** Tela moderna com design responsivo, animação suave e foco em usabilidade.

**Layout:**
- Split screen: lado esquerdo com formulário, lado direito com branding/ilustração
- Em telas menores (mobile), apenas o formulário aparece
- Fundo com gradiente ou pattern sutil

**Elementos:**
| Elemento | Descrição |
|----------|-----------|
| **Logo** | Logo da empresa no topo do formulário |
| **Campo Login** | Input com ícone de usuário, autocomplete off |
| **Campo Senha** | Input com ícone de cadeado, toggle visibility (olho) |
| **Checkbox "Lembrar-me"** | Opção para manter sessão (opcional) |
| **Botão "Entrar"** | Botão primário full-width com loading spinner |
| **Link "Esqueci minha senha"** | Abre modal/fluxo de recuperação |
| **Mensagem de erro** | Toast ou inline error para credenciais inválidas |

**Estados:**
| Estado | Comportamento |
|--------|---------------|
| **Inicial** | Formulário limpo, botão desabilitado |
| **Validando** | Validação em tempo real (campo obrigatório, tamanho mínimo) |
| **Loading** | Botão com spinner, campos desabilitados |
| **Erro** | Toast error ou mensagem inline: "Usuário ou senha inválidos" |
| **Sucesso** | Redireciona para dashboard, menu carregado |

**Mockup conceitual:**
```
┌──────────────────────────────────────────────────────┐
│  ┌──────────────────┐  ┌──────────────────────────┐ │
│  │                  │  │                          │ │
│  │     [LOGO]      │  │                          │ │
│  │                  │  │    [Ilustração/          │ │
│  │   Usuário        │  │     Branding]            │ │
│  │   ┌──────────┐   │  │                          │ │
│  │   │          │   │  │  "Gestão Empresarial     │ │
│  │   └──────────┘   │  │   Inteligente"           │ │
│  │                  │  │                          │ │
│  │   Senha          │  │                          │ │
│  │   ┌──────────┐   │  │                          │ │
│  │   │          │   │  │                          │ │
│  │   └──────────┘   │  │                          │ │
│  │                  │  │                          │ │
│  │  [✔] Lembrar-me  │  │                          │ │
│  │                  │  │                          │ │
│  │  ┌──────────────┐│  │                          │ │
│  │  │   ENTRAR     ││  │                          │ │
│  │  └──────────────┘│  │                          │ │
│  │                  │  │                          │ │
│  │  Esqueceu sua    │  │                          │ │
│  │  senha?          │  │                          │ │
│  └──────────────────┘  └──────────────────────────┘ │
└──────────────────────────────────────────────────────┘
```

#### 3.2. Layout Base (DashboardLayout)

**Descrição:** Layout principal que envelopa todas as páginas internas.

**Componentes:**
1. **Sidebar:**
   - Colapsável (ícone de hamburguer no header)
   - Menu dinâmico vindo da API
   - Hierarquia com collapsible items
   - Ícones por página
   - Destaque no item ativo
   - Footer da sidebar: versão do sistema

2. **Header:**
   - Logo/ícone do sistema (esquerda)
   - Breadcrumb dinâmico (centro)
   - Avatar do usuário + nome + dropdown (direita)
   - Dropdown: "Meus Dados", "Alterar Senha", "Sair"

3. **Content Area:**
   - Padding responsivo
   - Scroll suave
   - Transição de páginas

#### 3.3. Guards de Autenticação

1. **AuthGuard:** Wrapper de rotas que verifica se token JWT existe e é válido. Se inválido, redireciona para `/login`.

2. **RoleGuard:** Verifica se o perfil do usuário tem acesso à rota. Baseado no `idPerfil` do JWT.

3. **PageAccessGuard:** Verifica se a página está na árvore de menu do usuário. Garante que mesmo com URL manual, o acesso é bloqueado.

#### 3.4. Pós-Login: Tela Dashboard Home

**Descrição:** Primeira tela após login, visão geral do sistema.

**Cards:**
- **Boas-vindas:** Nome do usuário, empresa, última sessão
- **Páginas Recentes:** TOP 10 páginas mais acessadas (grid de ícones)
- **Atalhos Rápidos:** Ações comuns (últimos relatórios, favoritos)
- **Informações do Sistema:** Versão, data/hora, ambiente

---

### 4. Sprint 2: Dashboard Home + Menu Dinâmico

#### 4.1. Integração Menu Dinâmico

**Endpoint:** `GET /api/v1/paginas/menu`

**Integração:**
- Ao fazer login, buscar árvore de menu e armazenar em cache (React Query)
- Sidebar renderiza recursivamente com base na hierarquia
- Ao clicar em item, navegar via React Router
- Marcar página como acessada (UPSERT em `ACESSO_VISUALIZACAO_PAGINA`)

#### 4.2. Estado Global (AuthContext)

**Estrutura do Context:**
```typescript
interface AuthState {
  user: {
    idUsuario: number;
    nome: string;
    login: string;
    idPerfil: number;
    idEstabelecimento?: number;
  } | null;
  token: string | null;
  menu: PaginaTree[];  // Árvore de páginas
  isAuthenticated: boolean;
  isLoading: boolean;
}
```

**Ações:**
- `login(credentials)` → chama API, armazena token e dados
- `logout()` → limpa dados, redireciona para login
- `refreshToken()` → tenta renovar token expirado
- `loadMenu()` → carrega árvore de menu

---

### 5. Sprint 3: Módulo Usuários/Perfis/Páginas (CRUD)

#### 5.1. Padrão de Páginas CRUD

Todas as páginas CRUD seguirão o mesmo padrão:

```
┌─────────────────────────────────────────────┐
│  Header: Título + Breadcrumb                │
├─────────────────────────────────────────────┤
│  Barra de Ações:                            │
│  [Busca...] [Filtros] [Novo +] [Exportar]  │
├─────────────────────────────────────────────┤
│                                             │
│  DataTable com:                             │
│  - Colunas sorteáveis                       │
│  - Paginação server-side                    │
│  - Ações por linha (Editar, Excluir)        │
│  - Checkbox para seleção múltipla           │
│  - Status visual (ativo/inativo)            │
│                                             │
├─────────────────────────────────────────────┤
│  Footer: Total de registros                 │
└─────────────────────────────────────────────┘
```

**Componentes CRUD Reutilizáveis:**
| Componente | Função |
|------------|--------|
| `DataTable` | Tabela genérica com colunas configuráveis, paginação, filtros |
| `SearchInput` | Input de busca com debounce |
| `FormDrawer` | Drawer lateral para formulários (edição/criação) |
| `ConfirmDialog` | Modal de confirmação para exclusão |
| `StatusToggle` | Switch para ativar/desativar registro |

#### 5.2. Endpoints Consumidos

| Feature | Endpoints |
|---------|-----------|
| Usuários | `GET/POST /api/v1/usuarios`, `GET/PUT/DELETE /api/v1/usuarios/{id}` |
| Perfis | `GET/POST /api/v1/perfis`, `GET/PUT/DELETE /api/v1/perfis/{id}` |
| Páginas | `GET/POST /api/v1/paginas`, `GET/PUT/DELETE /api/v1/paginas/{id}`, `GET /api/v1/paginas/menu` |
| Estabelecimentos | `GET/POST /api/v1/estabelecimentos`, `GET/PUT/DELETE /api/v1/estabelecimentos/{id}`, `GET /api/v1/estabelecimentos/tree` |

---

### 6. Sprint 4-8: Painéis de Dashboard

#### 6.1. Padrão de Páginas de Painel

```
┌─────────────────────────────────────────────┐
│  Header: Título + Breadcrumb                │
├─────────────────────────────────────────────┤
│  Filtros Globais:                           │
│  [Data Início] [Data Fim] [Empresa ▼] [...]│
├─────────────────────────────────────────────┤
│                                             │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐       │
│  │ KPI 1   │ │ KPI 2   │ │ KPI 3   │       │
│  │ R$ 1.2M │ │ 15.3%   │ │ 42      │       │
│  └─────────┘ └─────────┘ └─────────┘       │
│                                             │
│  ┌─────────────────────────────────────┐   │
│  │ Gráfico Principal (Área/Barras)     │   │
│  │                                     │   │
│  └─────────────────────────────────────┘   │
│                                             │
│  ┌─────────────────┐ ┌─────────────────┐   │
│  │ Tabela Detalhe  │ │ Gráfico Sec.    │   │
│  │ (com drill-down)│ │ (Pizza/Donut)   │   │
│  └─────────────────┘ └─────────────────┘   │
│                                             │
├─────────────────────────────────────────────┤
│  Footer: Última atualização                 │
└─────────────────────────────────────────────┘
```

#### 6.2. Painéis Específicos

**Módulo Compras:**
| Tela | Gráficos | Funcionalidades Específicas |
|------|----------|----------------------------|
| Resumo Anual | 4 gráficos (resultado, totais, grafico, grafico2) | Filtro por ano, empresa |
| Comitê Compras Tabela | Detalhada com agrupamento | Drill-down por item |
| Progressão Preço | Line chart com evolução | Comparativo meses |
| CFOP | Formulário + tabela | CRUD com log de alterações |
| Centro Custo | Tree view + tabela | 3 níveis de detalhe |

**Módulo Financeiro:**
| Tela | Gráficos | Funcionalidades Específicas |
|------|----------|----------------------------|
| Posição Financeira | 3 abas (legado, new, sreal) | Tabelas com totais |
| Posição Semanal | Tabela dinâmica | Remoção automática de colunas vazias |
| Fluxo Caixa | Area chart + tabela analítica | Remoção de colunas PORT/PORTADOR |
| DRE | 3 versões (padrão, homologado, out) | Drill-down: conta → prev → documento |
| Projeção | Line chart com previsão | Cenários |

**Módulo Prazo Médio:**
| Tela | Funcionalidades Específicas |
|------|----------------------------|
| Prazo Médio Mensal | Tabela mensal + gráfico de tendência |
| Prazo Médio Pessoa | Tabela por pessoa/empresa |
| Prazo Médio Documentos | Drill-down 3 níveis (recebimento e pagamento) |

**Módulo Vendas:**
| Tela | Gráficos |
|------|----------|
| Análise Vendas | Bar chart + tabela |
| Ranking Clientes | Tabela ordenável + bar chart horizontal |
| Plano Vendas Meta | Gauge chart + comparativo |

---

### 7. Requisitos Não-Funcionais

#### 7.1. Performance

| Requisito | Critério |
|-----------|----------|
| **Tempo de carregamento inicial** | < 2s (com lazy loading de chunks) |
| **Tempo de renderização** | < 500ms após dados carregados |
| **Tamanho bundle inicial** | < 200KB (gzip) |
| **Lazy loading** | Por rota (cada módulo carrega sob demanda) |
| **Cache de API** | React Query com staleTime configurável |
| **Virtual scrolling** | Tabelas com > 100 linhas |

#### 7.2. Responsividade

| Breakpoint | Largura | Comportamento |
|------------|---------|---------------|
| **Desktop** | > 1024px | Layout completo com sidebar expandida |
| **Tablet** | 768-1024px | Sidebar colapsada, grid adaptável |
| **Mobile** | < 768px | Sidebar em drawer, cards empilhados |

#### 7.3. Acessibilidade (a11y)

- Contraste mínimo WCAG AA
- Navegação por teclado (Tab, Enter, Escape)
- ARIA labels em todos os componentes interativos
- Focus visível em todos os elementos
- Suporte a leitores de tela

#### 7.4. Segurança

- Token JWT armazenado em memória ou httpOnly cookie
- Refresh token armazenado em httpOnly cookie
- Interceptor Axios para renovação automática
- Validação de formulários no frontend + backend
- Sanitização de inputs
- Proteção contra XSS (React já protege por padrão)
- CSP (Content Security Policy) configurado

---

### 8. Tema Visual

#### 8.1. Paleta de Cores

```
Cores Primárias:
  Primary:    #1A73E8 (Azul Google)
  Secondary:  #34A853 (Verde)
  Accent:     #FBBC04 (Amarelo)

Cores Neutras:
  Background: #F5F7FA
  Surface:    #FFFFFF
  Text:       #1F2937
  Text Light: #6B7280

Cores de Status:
  Success:    #34A853
  Warning:    #FBBC04
  Error:      #EA4335
  Info:       #4285F4
```

#### 8.2. Tipografia

```
Fonte Principal: Inter (Google Fonts)
  - Títulos:   600/700 weight, 1.5-2rem
  - Corpo:     400 weight, 0.875-1rem
  - Labels:    500 weight, 0.75rem
  - Monospace: JetBrains Mono (para dados financeiros)
```

#### 8.3. Componentes de Dashboard

- **Cards KPI:** Borda sutil, sombra leve, ícone colorido, valor em destaque, variação percentual
- **Gráficos:** Tooltips interativos, legendas claras, animação de entrada
- **Tabelas:** Zebra striping, hover highlight, colunas fixas (pin), sticky header
- **Sidebar:** Ícones consistentes (Material Icons), hover com destaque, item ativo em negrito

---

### 9. Plano de Implementação (Multi-Agent)

#### 9.1. Agentes e Responsabilidades

| Agente | Responsabilidade no Frontend |
|--------|------------------------------|
| **Architect** | Definir estrutura, contratos API-Frontend, padrões de componentes |
| **Frontend Engineer** | Implementar componentes React, páginas, integração com API |
| **Backend Engineer** | Garantir que endpoints atendam às necessidades do frontend, ajustar DTOs |
| **QA Engineer** | Testar fluxos de usuário, responsividade, acessibilidade, performance |
| **DevOps Engineer** | Configurar build, deploy, Docker, CI/CD para o frontend |

#### 9.2. Tarefas Detalhadas por Sprint

**Sprint 1: Login + Layout Base (P0)**
| ID | Tarefa | Agente | Esforço |
|----|--------|--------|:-------:|
| F-P1-T01 | Inicializar projeto React + Vite + TypeScript | Frontend Engineer | 30min |
| F-P1-T02 | Configurar tema MUI, paleta de cores, tipografia | Frontend Engineer | 1h |
| F-P1-T03 | Criar componente de LoginForm com validação (React Hook Form + Zod) | Frontend Engineer | 3h |
| F-P1-T04 | Criar LoginPage com split screen e responsividade | Frontend Engineer | 2h |
| F-P1-T05 | Configurar Axios client com interceptors JWT | Frontend Engineer | 1h |
| F-P1-T06 | Criar AuthContext + useAuth hook | Frontend Engineer | 2h |
| F-P1-T07 | Criar AuthGuard e RoleGuard | Frontend Engineer | 1h |
| F-P1-T08 | Criar DashboardLayout (Sidebar + Header + Content) | Frontend Engineer | 4h |
| F-P1-T09 | Criar Sidebar dinâmica com menu vindo da API | Frontend Engineer | 3h |
| F-P1-T10 | Criar Header com breadcrumb + avatar/dropdown | Frontend Engineer | 2h |
| F-P1-T11 | Criar DashboardHomePage (cards boas-vindas, recentes, atalhos) | Frontend Engineer | 2h |
| F-P1-T12 | Integrar fluxo completo: login → menu → dashboard | Frontend Engineer | 2h |
| F-P1-T13 | Testar fluxo de autenticação (login, logout, token expirado) | QA Engineer | 2h |
| F-P1-T14 | Revisar componentes e validar contratos com API | Architect | 1h |

**Tempo total Sprint 1: ~26h**

**Sprint 2: CRUD Usuários/Perfis/Páginas (P0)**
| ID | Tarefa | Agente | Esforço |
|----|--------|--------|:-------:|
| F-P2-T01 | Criar componente DataTable genérico (colunas, paginação, filtros) | Frontend Engineer | 4h |
| F-P2-T02 | Criar componente FormDrawer para CRUD lateral | Frontend Engineer | 2h |
| F-P2-T03 | Criar ConfirmDialog, SearchInput, StatusToggle | Frontend Engineer | 2h |
| F-P2-T04 | Implementar UsuarioListPage + UsuarioFormPage | Frontend Engineer | 4h |
| F-P2-T05 | Implementar PerfilListPage + PerfilFormPage + PermissoesPage | Frontend Engineer | 4h |
| F-P2-T06 | Implementar PaginaListPage + PaginaFormPage + TreeView | Frontend Engineer | 3h |
| F-P2-T07 | Implementar EstabelecimentoListPage + EstabelecimentoTreeView | Frontend Engineer | 3h |
| F-P2-T08 | Configurar rotas React Router com lazy loading | Frontend Engineer | 1h |
| F-P2-T09 | Testar CRUD completo e permissões | QA Engineer | 3h |
| F-P2-T10 | Revisão arquitetural | Architect | 1h |

**Tempo total Sprint 2: ~27h**

**Sprint 3: Módulo Compras (P1)**
| ID | Tarefa | Agente | Esforço |
|----|--------|--------|:-------:|
| F-P3-T01 | Criar componentes de chart base (BarChart, LineChart, AreaChart) | Frontend Engineer | 3h |
| F-P3-T02 | Criar KpiCard genérico | Frontend Engineer | 1h |
| F-P3-T03 | Implementar ResumoAnualPage com 4 gráficos | Frontend Engineer | 4h |
| F-P3-T04 | Implementar ComiteComprasPage com DataTable | Frontend Engineer | 2h |
| F-P3-T05 | Implementar ProgressaoPrecoPage com LineChart | Frontend Engineer | 2h |
| F-P3-T06 | Implementar CfopPage (CRUD + log alterações) | Frontend Engineer | 3h |
| F-P3-T07 | Implementar CentroCustoPage (3 níveis) | Frontend Engineer | 3h |
| F-P3-T08 | Testar painéis de compras | QA Engineer | 3h |

**Tempo total Sprint 3: ~21h**

**Sprint 4: Módulo Financeiro (P1)**
| ID | Tarefa | Agente | Esforço |
|----|--------|--------|:-------:|
| F-P4-T01 | Implementar PosicaoFinanceiraPage com 3 abas | Frontend Engineer | 6h |
| F-P4-T02 | Implementar FluxoCaixaPage (AreaChart + tabela) | Frontend Engineer | 4h |
| F-P4-T03 | Implementar DrePage com 3 versões + drill-down | Frontend Engineer | 8h |
| F-P4-T04 | Implementar ProjecaoPage com gráfico de projeção | Frontend Engineer | 3h |
| F-P4-T05 | Implementar AjustesPage + PortadorPage | Frontend Engineer | 4h |
| F-P4-T06 | Testar painéis financeiros | QA Engineer | 4h |

**Tempo total Sprint 4: ~29h**

**Sprint 5: Prazo Médio + Vendas + Secundários (P2)**
| ID | Tarefa | Agente | Esforço |
|----|--------|--------|:-------:|
| F-P5-T01 | Implementar PrazoMedioMensal/Pessoa/DocumentosPage | Frontend Engineer | 6h |
| F-P5-T02 | Implementar drill-down 3 níveis PZM | Frontend Engineer | 4h |
| F-P5-T03 | Implementar AnaliseVendas/Ranking/PlanoVendasPage | Frontend Engineer | 6h |
| F-P5-T04 | Implementar Agricola/Calendario/Dba/AgrupamentoPage | Frontend Engineer | 6h |
| F-P5-T05 | Testar todos os módulos restantes | QA Engineer | 4h |

**Tempo total Sprint 5: ~26h**

#### 9.3. Cronograma Consolidado

| Sprint | Módulo | Esforço Estimado | Prioridade |
|--------|--------|:----------------:|:----------:|
| Sprint 1 | Login + Layout + Guards | ~26h | P0 |
| Sprint 2 | CRUD Usuários/Perfis/Páginas | ~27h | P0 |
| Sprint 3 | Módulo Compras | ~21h | P1 |
| Sprint 4 | Módulo Financeiro | ~29h | P1 |
| Sprint 5 | Prazo Médio + Vendas + Secundários | ~26h | P2 |
| **TOTAL** | | **~129h** | |

---

### 10. API Endpoints Mapeados (Contratos Frontend ⇄ Backend)

#### 10.1. Autenticação

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| POST | `/api/v1/auth/login` | Autenticar usuário | `{ login, senha }` |
| POST | `/api/v1/auth/refresh` | Renovar token | `{ refreshToken }` |
| POST | `/api/v1/auth/alterar-senha` | Alterar senha | `{ senhaAtual, novaSenha }` |

#### 10.2. Usuários

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/usuarios` | Listar usuários (paginado) |
| GET | `/api/v1/usuarios/{id}` | Obter usuário por ID |
| POST | `/api/v1/usuarios` | Criar usuário |
| PUT | `/api/v1/usuarios/{id}` | Atualizar usuário |
| DELETE | `/api/v1/usuarios/{id}` | Excluir (lógica) usuário |

#### 10.3. Perfis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/perfis` | Listar perfis |
| GET | `/api/v1/perfis/{id}` | Obter perfil por ID |
| POST | `/api/v1/perfis` | Criar perfil |
| PUT | `/api/v1/perfis/{id}` | Atualizar perfil |
| DELETE | `/api/v1/perfis/{id}` | Excluir perfil |
| GET | `/api/v1/perfis/{id}/paginas` | Listar páginas vinculadas ao perfil |
| POST | `/api/v1/perfis/{id}/paginas` | Vincular páginas ao perfil |

#### 10.4. Páginas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/paginas` | Listar páginas (admin) |
| GET | `/api/v1/paginas/menu` | Obter árvore de menu do usuário logado |
| GET | `/api/v1/paginas/{id}` | Obter página por ID |
| POST | `/api/v1/paginas` | Criar página |
| PUT | `/api/v1/paginas/{id}` | Atualizar página |
| DELETE | `/api/v1/paginas/{id}` | Excluir página |
| POST | `/api/v1/paginas/acessadas` | Registrar acesso à página |

#### 10.5. Estabelecimentos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/estabelecimentos` | Listar estabelecimentos |
| GET | `/api/v1/estabelecimentos/tree` | Obter árvore de estabelecimentos |
| GET | `/api/v1/estabelecimentos/{id}` | Obter estabelecimento por ID |
| POST | `/api/v1/estabelecimentos` | Criar estabelecimento |
| PUT | `/api/v1/estabelecimentos/{id}` | Atualizar estabelecimento |
| DELETE | `/api/v1/estabelecimentos/{id}` | Excluir estabelecimento |

#### 10.6. Compras

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/compras/resumo-anual` | Resumo anual (4 cursores) |
| GET | `/api/v1/compras/comite` | Comitê de compras |
| GET | `/api/v1/compras/progressao-preco` | Progressão de preço |
| GET | `/api/v1/compras/cfop` | Listar CFOPs |
| POST | `/api/v1/compras/cfop` | Criar CFOP |
| PUT | `/api/v1/compras/cfop/{id}` | Atualizar CFOP |
| DELETE | `/api/v1/compras/cfop/{id}` | Excluir CFOP |
| GET | `/api/v1/compras/centro-custo` | Centro de custo (agrupado) |
| GET | `/api/v1/compras/centro-custo/{id}/detalhe` | Detalhe centro de custo |
| GET | `/api/v1/compras/centro-custo/{id}/detalhe/{prodId}` | Detalhe produto |

#### 10.7. Financeiro

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/financeiro/posicao` | Posição financeira (query params: versao) |
| GET | `/api/v1/financeiro/posicao/semanal` | Posição semanal |
| GET | `/api/v1/financeiro/fluxo-caixa` | Fluxo de caixa |
| GET | `/api/v1/financeiro/dre` | DRE (query params: versao) |
| GET | `/api/v1/financeiro/dre/{conta}/detalhamento` | Drill-down DRE |
| GET | `/api/v1/financeiro/projecao` | Projeção financeira |
| GET | `/api/v1/financeiro/ajustes` | Ajustes financeiros |
| POST | `/api/v1/financeiro/ajustes` | Criar ajuste |
| GET | `/api/v1/financeiro/portador` | Portadores |
| GET | `/api/v1/financeiro/portador/saldo` | Saldo portador |

#### 10.8. Prazo Médio

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/prazo-medio/mensal` | Prazo médio mensal (receber/pagar) |
| GET | `/api/v1/prazo-medio/pessoa` | Prazo médio por pessoa |
| GET | `/api/v1/prazo-medio/documentos` | Drill-down documentos (3 níveis) |

#### 10.9. Vendas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/vendas/analise` | Análise de vendas |
| GET | `/api/v1/vendas/ranking` | Ranking de clientes |
| GET | `/api/v1/vendas/plano` | Plano de vendas resultado |

#### 10.10. Secundários

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/v1/agricola/frutas` | Compras de frutas |
| GET | `/api/v1/calendario` | Calendário financeiro |
| POST | `/api/v1/calendario` | Inserir evento calendário |
| GET | `/api/v1/dba/sessoes` | Sessões Oracle (admin) |
| POST | `/api/v1/dba/matar-sessao` | Matar sessão (admin) |
| GET | `/api/v1/agrupamentos` | Agrupamentos DRE |

---

### 11. Configuração de Ambiente

#### 11.1. Variáveis de Ambiente (.env)

```env
# .env.development
VITE_API_BASE_URL=http://localhost:5000/api/v1
VITE_APP_TITLE=GestaoNew - Desenvolvimento

# .env.production
VITE_API_BASE_URL=https://api.gestaonew.com.br/api/v1
VITE_APP_TITLE=GestaoNew
```

#### 11.2. Configuração Docker (opcional para dev)

```dockerfile
# Dockerfile (multi-stage)
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

### 12. Critérios de Aceitação

#### 12.1. Sprint 1 - Login + Layout

- [ ] Tela de login renderiza com split screen responsivo
- [ ] Validação de formulário funciona (campos obrigatórios, email válido)
- [ ] Loading state aparece durante requisição
- [ ] Erro de login exibe mensagem amigável
- [ ] Login bem-sucedido redireciona para dashboard
- [ ] Token JWT é armazenado e enviado em requisições
- [ ] Sidebar carrega menu dinâmico da API
- [ ] Header exibe nome do usuário e avatar
- [ ] Logout limpa sessão e redireciona para login
- [ ] AuthGuard redireciona para login se token inválido
- [ ] Dashboard Home exibe cards de boas-vindas e páginas recentes
- [ ] Responsivo: funciona em desktop, tablet e mobile
- [ ] `npm run build` sem erros

#### 12.2. Sprint 2 - CRUD

- [ ] DataTable exibe dados paginados
- [ ] Busca/filtro funcionam (com debounce)
- [ ] CRUD de usuários completo (criar, editar, excluir)
- [ ] CRUD de perfis completo com vinculação de páginas
- [ ] CRUD de páginas completo com visualização em árvore
- [ ] CRUD de estabelecimentos completo
- [ ] Confirmação antes de excluir
- [ ] Toast notifications em todas as operações
- [ ] Lazy loading de chunks por rota

#### 12.3. Sprints 3-5 - Painéis

- [ ] Todos os gráficos renderizam com dados reais
- [ ] Filtros de data/empresa funcionam em todos os painéis
- [ ] Drill-down funcional (DRE, Prazo Médio)
- [ ] Tabelas com sorting e paginação
- [ ] Exportação de dados (CSV/Excel) nos painéis principais
- [ ] Performance: < 2s para carregar qualquer painel
- [ ] Dados são atualizados (React Query refetch)

---

### 13. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|:---:|:---:|-----------|
| **Volume de dados grande** nos gráficos pode travar o navegador | Média | Alto | Virtual scrolling, paginação server-side, lazy loading de séries |
| **Drill-down complexo** (DRE 3 níveis, PZM 3 níveis) | Média | Médio | Componente DrillDownChart reutilizável, loading por nível |
| **Responsividade comprometida** em gráficos complexos | Média | Médio | Testar em 3 breakpoints, gráficos com resize handler |
| **Token expira durante uso** (JWT 15-60min) | Alta | Alto | Refresh token automático no Axios interceptor |
| **Menu dinâmico com muitos itens** | Baixa | Baixo | Virtual scrolling na sidebar, pesquisa no menu |
| **Integração com backend em paralelo** (endpoints podem mudar) | Alta | Médio | Tipos TypeScript gerados a partir dos DTOs da API, contrato API-first |

---

### 14. Perguntas em Aberto

1. **Framework UI:** MUI v6 vs Shadcn/ui vs Ant Design? Recomendação: MUI pela maturidade e componentes de DataGrid/Charts nativos.
2. **Gráficos:** Recharts (mais simples) vs ApexCharts (mais recursos) vs ECharts (mais performático)? Recomendação: Recharts para início, migrar para ApexCharts em gráficos complexos.
3. **Autenticação:** localStorage (simples) vs httpOnly cookie (mais seguro)? Recomendação: localStorage para MVP, migrar para cookie com refresh token depois.
4. **PWA:** Deve ser Progressive Web App para funcionar offline parcial? Recomendação: Não para o MVP, considerar depois.
5. **Testes:** Vitest + Testing Library (unidade) + Playwright (E2E)? Recomendação: Vitest para unitários, Playwright para E2E nos fluxos críticos.
6. **Internacionalização:** Suporte a português apenas ou preparar para i18n? Recomendação: Apenas português, mas estrutura preparada para i18n.

---

### Apêndice A — Bibliotecas e Versões (package.json)

```json
{
  "name": "gestaonew-frontend",
  "private": true,
  "version": "0.0.1",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "preview": "vite preview",
    "test": "vitest",
    "lint": "eslint ."
  },
  "dependencies": {
    "react": "^19.0.0",
    "react-dom": "^19.0.0",
    "react-router-dom": "^7.0.0",
    "@tanstack/react-query": "^5.0.0",
    "zustand": "^5.0.0",
    "axios": "^1.7.0",
    "@mui/material": "^6.0.0",
    "@mui/icons-material": "^6.0.0",
    "@emotion/react": "^11.13.0",
    "@emotion/styled": "^11.13.0",
    "recharts": "^2.14.0",
    "react-hook-form": "^7.54.0",
    "@hookform/resolvers": "^3.9.0",
    "zod": "^3.24.0",
    "sonner": "^1.7.0",
    "date-fns": "^4.1.0",
    "dayjs": "^1.11.0"
  },
  "devDependencies": {
    "@types/react": "^19.0.0",
    "@types/react-dom": "^19.0.0",
    "@vitejs/plugin-react": "^4.3.0",
    "vite": "^6.0.0",
    "typescript": "^5.7.0",
    "vitest": "^2.1.0",
    "@testing-library/react": "^16.1.0",
    "@testing-library/jest-dom": "^6.6.0",
    "eslint": "^9.0.0",
    "prettier": "^3.4.0",
    "tailwindcss": "^4.0.0"
  }
}
```

---

### Apêndice B — Tipos TypeScript Compartilhados

```typescript
// api.types.ts
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface PaginatedResponse<T> extends ApiResponse<T[]> {
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PaginationParams {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

// auth.types.ts
export interface LoginRequest {
  login: string;
  senha: string;
  lembrarMe?: boolean;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  usuario: {
    idUsuario: number;
    nome: string;
    login: string;
    email: string;
    idPerfil: number;
    nomePerfil: string;
    idEstabelecimento?: number;
    nomeEstabelecimento?: string;
    atualizaSenha: boolean;
  };
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface AlterarSenhaRequest {
  senhaAtual: string;
  novaSenha: string;
}

// pagina.types.ts
export interface PaginaTree {
  idPagina: number;
  nome: string;
  icone?: string;
  url?: string;
  ordem: number;
  children: PaginaTree[];
}

// compras.types.ts
export interface ResumoAnualCompras {
  resultado: CompraResultado[];
  totais: CompraTotais[];
  grafico: CompraGrafico[];
  grafico2: CompraGrafico2[];
}

// financeiro.types.ts (fortemente tipado para cada versão)
export type VersaoPosicao = 'legado' | 'new' | 'sreal';
export type VersaoDre = 'padrao' | 'homologado' | 'out';

export interface DreDrillDown {
  conta: DreConta[];
  previsto: DrePrevisto[];
  documento: DreDocumento[];
}
```

---

*PRD gerado em 29/07/2026 para o frontend dos painéis GestaoNew. Baseado na API implementada e nos padrões de arquitetura definidos no projeto.*