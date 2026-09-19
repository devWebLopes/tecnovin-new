# 📖 GUIA_MENU.md — Guia Completo do Funcionamento dos Menus

> **Público-alvo**: programadores (backend, frontend ou fullstack) que precisam entender, manter ou evoluir o sistema de menus do GestãoNew.
> **Escopo**: visão de ponta a ponta — tabelas Oracle → API .NET 9 → React — incluindo objetos, classes, SQLs, cache, autorização e fluxos.
> **Documentos relacionados**: `docs/menus.md` (engenharia reversa do legado — fonte das regras RN/D), `docs/PRD_MENU_LATERAL.md` (requisitos RF), `CLAUDE.md` (contratos arquiteturais).

---

## 1. Visão Geral — O que é o "Menu"

O menu é a **shell de navegação** da aplicação. No sistema legado (Web Forms + DevExpress) ele ficava num painel à direita de `Principal.aspx`; na stack atual ele é a **sidebar esquerda** do React. Funcionalmente, ele entrega 3 blocos:

| Bloco | Conteúdo | Fonte de dados |
|-------|----------|----------------|
| **Árvore de navegação** | Páginas hierárquicas (grupo → página → subpágina, profundidade ilimitada) filtradas pelo **perfil** do usuário | `ACESSO_CADASTRO_PAGINA` + `ACESSO_PERFIL_PAGINA` |
| **Painel "Mais Acessados"** | Top 10 páginas mais abertas pelo **usuário** (telemetria de uso) | `ACESSO_VISUALIZACAO_PAGINA` |
| **Barra do usuário** | Login/nome, link condicional do B.I., alterar senha, sair | `ACESSO_CADASTRO_USUARIO` + `USUARIO_EMPRESA` |

### Conceitos-chave (decore estes 4)

1. **`CHAVE_CONTROLE`** — string única que identifica logicamente cada página (ex.: `usuarios`, `comprasComite`). É a **"moeda" de autorização**: liga menu ↔ rota SPA ↔ permissão ↔ telemetria.
2. **Permissão é por PERFIL, nunca por usuário** — o usuário tem exatamente 1 perfil (`ACESSO_CADASTRO_USUARIO.ID_PERFIL`); o vínculo N:N perfil↔página vive em `ACESSO_PERFIL_PAGINA`.
3. **Pai implícito (RN-03)** — um grupo/pai aparece no menu se **qualquer** filho for permitido ao perfil, mesmo que o pai não tenha vínculo próprio.
4. **Defesa em profundidade (RN-12)** — o menu filtra o que é exibido, mas a permissão é **revalidada** no backend (policy `PaginaAcesso`) e no frontend (`MenuGuard`) a cada acesso. Nunca confie só no menu.

---

## 2. Stack e Estrutura da Solução

### 2.1 Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| Backend | **.NET 9**, ASP.NET Core **Minimal APIs**, **Dapper 2.1.79**, **Oracle** (ODP.NET `Oracle.ManagedDataAccess.Core`), JWT Bearer, BCrypt, Serilog, `IMemoryCache` |
| Frontend | **React 18 + TypeScript + Vite**, **Ant Design 5** (componente `Menu`), **Zustand** (estado), **React Router DOM 6**, **Axios**, dayjs |
| Testes | xUnit + Moq (backend) · Vitest (frontend) |

### 2.2 Projetos da solution (`Empresa.sln`)

```
Empresa.Api      → apresentação: Endpoints, Services, DTOs, Authorization, Middleware
Empresa.Data     → infra/dados: Models, Repositories (Dapper), DbSession
Empresa.Util     → utilitários cross-cutting
Empresa.Worker   → processos em background
Empresa.Tests    → testes xUnit
Empresa.Web      → SPA React (Vite)
```

Regra de dependência (Clean Architecture): uma camada só referencia a imediatamente abaixo. Endpoints recebem **DTOs**, nunca entidades de banco.

---

## 3. Modelo de Dados — Tabelas Oracle Envolvidas

> ⚠️ **Regra de Ouro**: estas tabelas são herdadas do legado e **não podem ser alteradas estruturalmente**.

### 3.1 `ACESSO_CADASTRO_PAGINA` — catálogo de páginas (os itens do menu)

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| `ID_PAGINA` | NUMBER (PK) | Identificador da página |
| `URL` | VARCHAR2 | URL legada `.aspx` (ex.: `interna/CadastroPerfil.aspx`) — hoje usada só como fallback de mapeamento de rota |
| `TITULO_ABA` | VARCHAR2 | Título da aba (herança do sistema de abas legado) |
| `CHAVE_CONTROLE` | VARCHAR2 | **Chave lógica única** — moeda de autorização |
| `TITULO_MENU` | VARCHAR2 | Texto exibido no nó do menu |
| `ID_PAGINA_PAI` | NUMBER (FK self, nullable) | `NULL` = nó raiz; auto-relacionamento (hierarquia) |
| `ORDEM` | NUMBER | Ordenação crescente **dentro de cada nível** |
| `TOOLTIP` | VARCHAR2 | Tooltip do nó |
| `ATIVO` | CHAR(1) | Só `'S'` entra no menu (exclusão é lógica: `ATIVO='N'`) |

### 3.2 `ACESSO_PERFIL_PAGINA` — permissões (N:N)

| Coluna | Descrição |
|--------|-----------|
| `ID_PERFIL` (FK) | Perfil |
| `ID_PAGINA` (FK) | Página permitida |

### 3.3 `ACESSO_CADASTRO_USUARIO` — usuários

Relevantes ao menu: `ID_USUARIO` (PK), `NOME`, `LOGIN`, `SENHA` (hash BCrypt na nova stack), `ID_PERFIL` (FK — **1 perfil por usuário**), `ATIVO`, `QUANTIDADE_ACESSO`, `ATUALIZA_SENHA`.

### 3.4 `ACESSO_VISUALIZACAO_PAGINA` — telemetria ("Mais Acessados")

| Coluna | Descrição |
|--------|-----------|
| `ID_VISUALIZACAO_PAGINA` (PK) | Identificador |
| `ID_USUARIO` (FK) | Usuário |
| `ID_PAGINA` (FK) | Página |
| `DATA_HORA` | Primeiro/último acesso (`SYSDATE`) |
| `QUANTIDADE` | Contador incrementado a cada abertura |

### 3.5 `USUARIO_EMPRESA` — vínculo B.I.

Se existir ao menos 1 registro para o usuário → o botão "Acesso ao B.I." aparece no header (RN-09).

### 3.6 Diagrama lógico

```
ACESSO_CADASTRO_USUARIO ──(ID_PERFIL)──► [PERFIL]
        │ 1                                  │ 1
        ▼ N                                  ▼ N
ACESSO_VISUALIZACAO_PAGINA          ACESSO_PERFIL_PAGINA
        │ N                                  │ N
        └────────► ACESSO_CADASTRO_PAGINA ◄──┘
                        │ ▲
                        └─┘  self-reference: ID_PAGINA_PAI → ID_PAGINA

ACESSO_CADASTRO_USUARIO 1──N USUARIO_EMPRESA   (habilita link B.I.)
```

---

## 4. Backend — Objetos e Classes

### 4.1 Mapa dos artefatos

| Artefato | Arquivo | Papel |
|----------|---------|-------|
| Entidade `Pagina` | `Empresa.Data/Models/Pagina.cs` | Espelha `ACESSO_CADASTRO_PAGINA` |
| Entidade `PaginaAcesso` | `Empresa.Data/Models/PaginaAcesso.cs` | `Pagina` + `Quantidade` (mais acessados) |
| Entidade `MenuUsuarioInfo` | `Empresa.Data/Models/MenuUsuarioInfo.cs` | Login, nome e contagem de vínculos B.I. |
| `DbSession` | `Empresa.Data/DbSession.cs` | Conexão Oracle **Scoped** (1 por requisição) |
| `IAcessoRepository` / `AcessoRepository` | `Empresa.Data/Repositories/AcessoRepository.cs` | Todos os SQLs do menu (Dapper) |
| `IPaginaRepository` / `PaginaRepository` | `Empresa.Data/Repositories/PaginaRepository.cs` | CRUD de páginas (admin) |
| `IMenuService` / `MenuService` | `Empresa.Api/Services/MenuService.cs` | **Cérebro do menu**: árvore + cache + telemetria |
| `IPaginaService` / `PaginaService` | `Empresa.Api/Services/PaginaService.cs` | CRUD de páginas + invalidação de cache |
| `PerfilService` | `Empresa.Api/Services/PerfilService.cs` | Permissões perfil×página + invalidação de cache |
| DTOs Response | `Empresa.Api/DTOs/Response/Menu*.cs` | `MenuResponse`, `MenuItemResponse`, `MaisAcessadoItemResponse`, `MenuUsuarioResponse` |
| DTO Request | `Empresa.Api/DTOs/Request/RegistroAcessoMenuRequest.cs` | Body da telemetria (`chaveControle`) |
| `MenuEndpoints` | `Empresa.Api/Endpoints/MenuEndpoints.cs` | `GET /api/v1/menu`, `POST /api/v1/menu/acessos` |
| `PaginaEndpoints` | `Empresa.Api/Endpoints/PaginaEndpoints.cs` | CRUD `/api/v1/paginas` + guard `/autorizacao` |
| `PaginaAutorizacaoHandler` | `Empresa.Api/Authorization/PaginaAutorizacaoHandler.cs` | Policy `PaginaAcesso` (RN-12) |
| DI / pipeline | `Empresa.Api/Program.cs` | Registro de serviços, JWT, policy, CORS |

### 4.2 Entidades (`Empresa.Data.Models`)

```csharp
// Pagina.cs — espelha ACESSO_CADASTRO_PAGINA
public class Pagina
{
    public int IDPagina { get; set; }
    public string Url { get; set; } = string.Empty;
    public string TituloAba { get; set; } = string.Empty;
    public string ChaveControle { get; set; } = string.Empty;
    public string TituloMenu { get; set; } = string.Empty;
    public int? IDPaginaPai { get; set; }      // null = raiz
    public int Ordem { get; set; }
    public string ToolTip { get; set; } = string.Empty;
    public string Ativo { get; set; } = "S";
}

// PaginaAcesso.cs — herda Pagina, acrescenta o contador da telemetria
public class PaginaAcesso : Pagina { public int Quantidade { get; set; } }

// MenuUsuarioInfo.cs — barra do usuário
public class MenuUsuarioInfo
{
    public int IDUsuario { get; set; }
    public string Nome { get; set; }
    public string Login { get; set; }
    public int Vinculos { get; set; }   // COUNT em USUARIO_EMPRESA → link B.I.
}
```

> O mapeamento coluna→propriedade (`ID_PAGINA` → `IDPagina`) funciona porque o `Program.cs` ativa `DefaultTypeMap.MatchNamesWithUnderscores = true` (Dapper).

### 4.3 DTOs (`Empresa.Api.DTOs`)

```csharp
// Response/MenuResponse.cs — payload do GET /api/v1/menu
public class MenuResponse
{
    public List<MenuItemResponse> Menu { get; set; }           // árvore
    public List<MaisAcessadoItemResponse> MaisAcessados { get; set; }
    public MenuUsuarioResponse Usuario { get; set; }
}

// Response/MenuItemResponse.cs — nó recursivo da árvore
public class MenuItemResponse
{
    public int IdPagina { get; set; }
    public string ChaveControle { get; set; }
    public string TituloMenu { get; set; }
    public string TituloAba { get; set; }
    public string Url { get; set; }
    public string ToolTip { get; set; }
    public int Ordem { get; set; }
    public List<MenuItemResponse> Filhos { get; set; }   // recursão
}

// Response/MaisAcessadoItemResponse.cs — idem + Quantidade
// Response/MenuUsuarioResponse.cs — { Login, Nome, ExibeLinkBi }
// Request/RegistroAcessoMenuRequest.cs — { ChaveControle } ([Required])
```

Exemplo real do JSON de `GET /api/v1/menu`:

```json
{
  "menu": [
    {
      "idPagina": 10, "chaveControle": "configuracoes", "tituloMenu": "Configurações",
      "tituloAba": "", "url": "", "toolTip": "", "ordem": 9,
      "filhos": [
        { "idPagina": 42, "chaveControle": "usuarios", "tituloMenu": "Usuários",
          "tituloAba": "Usuários", "url": "interna/CadastroUsuario.aspx",
          "toolTip": "", "ordem": 1, "filhos": [] }
      ]
    }
  ],
  "maisAcessados": [
    { "idPagina": 42, "chaveControle": "usuarios", "tituloMenu": "Usuários",
      "tituloAba": "Usuários", "url": "interna/CadastroUsuario.aspx", "quantidade": 87 }
  ],
  "usuario": { "login": "joao", "nome": "João Silva", "exibeLinkBi": true }
}
```

### 4.4 Repositório — os SQLs do menu (`AcessoRepository`)

Todos usam **bind variables** (`:Parametro`) — correção do SQL injection do legado (D-01). Conexão via `DbSession` (Scoped, `IDbConnection` lazy).

#### a) Árvore do menu — `GetPaginasMenuAsync(idPerfil)`

```sql
SELECT DISTINCT p.ID_PAGINA, p.URL, p.TITULO_ABA, p.CHAVE_CONTROLE,
       p.TITULO_MENU, p.ID_PAGINA_PAI, p.ORDEM, p.TOOLTIP, p.ATIVO
  FROM ACESSO_CADASTRO_PAGINA p
 WHERE p.ATIVO = 'S'
   AND p.ID_PAGINA IN (
         SELECT q.ID_PAGINA
           FROM ACESSO_CADASTRO_PAGINA q
          START WITH q.ID_PAGINA IN (
                     SELECT pp.ID_PAGINA
                       FROM ACESSO_PERFIL_PAGINA pp
                      WHERE pp.ID_PERFIL = :IdPerfil)
          CONNECT BY PRIOR q.ID_PAGINA_PAI = q.ID_PAGINA)
 ORDER BY p.ORDEM
```

**Como ler**: `START WITH` = páginas com vínculo direto no perfil (RN-01); `CONNECT BY PRIOR q.ID_PAGINA_PAI = q.ID_PAGINA` **sobe** a hierarquia trazendo **todos os ancestrais** (pais implícitos, RN-03) com profundidade ilimitada (corrige D-07 do legado, que subia só 1 nível); `ATIVO='S'` vale para folhas **e** ancestrais (corrige D-06). Retorna **lista plana** — a montagem da árvore é feita em C#.

#### b) Mais acessados — `GetPaginasMaisAcessadasAsync(idUsuario, idPerfil)`

```sql
SELECT p.*, avp.QUANTIDADE
  FROM ACESSO_VISUALIZACAO_PAGINA avp
 INNER JOIN ACESSO_CADASTRO_PAGINA p  ON avp.ID_PAGINA = p.ID_PAGINA
 INNER JOIN ACESSO_PERFIL_PAGINA  pp ON pp.ID_PAGINA = p.ID_PAGINA
 WHERE avp.ID_USUARIO = :IdUsuario
   AND pp.ID_PERFIL   = :IdPerfil      -- corrige D-05 (página revogada não aparece)
   AND p.ATIVO = 'S'
 ORDER BY avp.QUANTIDADE DESC          -- corrige D-04 (ordenação explícita)
 FETCH FIRST 10 ROWS ONLY              -- RN-07: top 10
```

#### c) Barra do usuário — `GetMenuUsuarioAsync(idUsuario)`

```sql
SELECT U.ID_USUARIO, U.NOME, U.LOGIN,
       (SELECT COUNT(1) FROM USUARIO_EMPRESA BI
         WHERE BI.ID_USUARIO = U.ID_USUARIO) AS VINCULOS
  FROM ACESSO_CADASTRO_USUARIO U
 WHERE U.ID_USUARIO = :IdUsuario
-- VINCULOS > 0 → ExibeLinkBi = true (RN-09)
```

#### d) Telemetria (upsert) — `RegistraAcessoPaginaAsync(idUsuario, idPagina)`

```sql
MERGE INTO ACESSO_VISUALIZACAO_PAGINA avp
USING (SELECT :IdUsuario AS ID_USUARIO, :IdPagina AS ID_PAGINA FROM DUAL) src
   ON (avp.ID_USUARIO = src.ID_USUARIO AND avp.ID_PAGINA = src.ID_PAGINA)
 WHEN MATCHED THEN
   UPDATE SET QUANTIDADE = QUANTIDADE + 1, DATA_HORA = SYSDATE
 WHEN NOT MATCHED THEN
   INSERT (ID_USUARIO, ID_PAGINA, QUANTIDADE, DATA_HORA)
   VALUES (src.ID_USUARIO, src.ID_PAGINA, 1, SYSDATE)
-- RN-08: 1ª abertura → INSERT qtd=1; demais → qtd+1
```

#### e) Guard de autorização — `GetPaginaByChaveAsync(chaveControle, idPerfil)`

Consulta o vínculo `CHAVE_CONTROLE` × `ID_PERFIL` (com `ATIVO='S'` e `ROWNUM=1`). É usada em **2 lugares**: pela policy `PaginaAcesso` e pelo `MenuService.RegistrarAcessoAsync` (RN-12: telemetria de página sem permissão é recusada silenciosamente).

### 4.5 Serviço — `MenuService` (o cérebro)

`Empresa.Api/Services/MenuService.cs` — registrado como **Scoped** junto com `IMemoryCache` (singleton) em `Program.cs`.

#### `GetMenuAsync(idUsuario, idPerfil)` — monta o shell

```csharp
var tree          = await GetOrCreateTreeAsync(idPerfil);                       // CACHE (10 min)
var maisAcessados = await _repository.GetPaginasMaisAcessadasAsync(idUsuario, idPerfil); // sempre fresco
var usuario       = await _repository.GetMenuUsuarioAsync(idUsuario);           // sempre fresco
```

A árvore é cacheada **por perfil** (dado compartilhado e estável); "mais acessados" e "usuário" são por usuário e vão ao banco a cada chamada.

#### Cache — chave com geração

```
Chave da árvore:   menu:tree:{geracao}:{idPerfil}   TTL absoluto: 10 min
Chave da geração:  menu:tree:generation             sliding: 1 hora
```

- `Invalidate(idPerfil)` → remove a chave daquele perfil (usado ao **salvar permissões do perfil** — `PerfilService.SalvarPermissoesAsync`).
- `Invalidate()` (sem parâmetro) → **incrementa a geração**, tornando todas as chaves `menu:tree:{gen}:{perfil}` obsoletas de uma vez (usado ao **criar/editar/excluir páginas** — `PaginaService`). Esse truque contorna a limitação do `IMemoryCache`, que não enumera chaves.
- Falha de cache **nunca** quebra o fluxo (try/catch com log — RF07.4).
- Falha de **banco** ao montar a árvore **propaga 500** (RF01.5/D-03) — nunca retorna menu vazio silencioso.

#### Algoritmo `BuildTree` (fiel ao legado, menus.md §10.3)

```
entrada: lista plana de Pagina (do SQL CONNECT BY)
raízes  = itens com IDPaginaPai NULL **ou cujo pai não está na lista**
          (ex.: ancestral inativo filtrado) — ordenadas por Ordem
para cada raiz: cria nó → PreencherFilhos(recursivo) → filhos ordenados por Ordem
saída: List<MenuItemResponse> aninhada
```

#### `RegistrarAcessoAsync(chaveControle, idUsuario, idPerfil)` — telemetria segura

1. Resolve a chave via `GetPaginaByChaveAsync(chave, idPerfil)` — **valida permissão ANTES** (RN-12);
2. Sem permissão → log warning e **ignora silenciosamente** (retorna sucesso; D-15);
3. Com permissão → `MERGE` (upsert) no repositório;
4. Qualquer exceção → log e segue (RF03.5: **telemetria nunca interrompe navegação**).

### 4.6 Endpoints (`MenuEndpoints` + `PaginaEndpoints`)

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/api/v1/menu` | Bearer | Shell completo (árvore + mais acessados + usuário). Extrai `ID_USUARIO`/`ID_PERFIL` das claims JWT |
| `POST` | `/api/v1/menu/acessos` | Bearer | Telemetria. Body `{ "chaveControle": "..." }` → 204 sempre (400 sem chave, 401 sem token) |
| `GET` | `/api/v1/paginas/menu-hierarquico` | Bearer | Árvore do perfil (uso administrativo/alternativo) |
| `GET` | `/api/v1/paginas/{chaveControle}/autorizacao` | Bearer + **policy `PaginaAcesso`** | Guard de rota: 200 `{ autorizado: true }` ou **403** |
| `GET/POST/PUT/DELETE` | `/api/v1/paginas[...]` | Bearer | CRUD de páginas — disparam `IMenuService.Invalidate()` |

Extração das claims (padrão repetido nos endpoints):

```csharp
var claimPerfil  = context.User.FindFirst("ID_PERFIL")?.Value;
var claimUsuario = context.User.FindFirst("ID_USUARIO")?.Value;
```

As claims são emitidas no login pelo `AuthService` (`ID_USUARIO`, `ID_PERFIL`, `ClaimTypes.Name`, `ClaimTypes.Email`, `ClaimTypes.Role`).

### 4.7 Autorização em profundidade — policy `PaginaAcesso`

`Program.cs` registra a policy; `PaginaAutorizacaoHandler` executa:

1. Lê `ID_PERFIL` do JWT → inválido/ausente = **fail**;
2. Extrai a chave de `RouteValues["chaveControle"]` **ou** `Query["chaveControle"]` → ausente = **fail**;
3. `GetPaginaByChaveAsync(chave, idPerfil)` → nulo = **403** + log warning; encontrou = **Succeed**.

É a tradução do `GetPaginaByChave` que as ~60 páginas internas do legado chamavam no `Page_Load`. **O backend é sempre a autoridade final.**

### 4.8 Quem invalida o cache do menu

| Ação | Onde | Invalidação |
|------|------|-------------|
| Criar/editar/excluir página | `PaginaService.Create/Update/DeleteAsync` | `Invalidate()` — **global** (geração++) |
| Salvar permissões do perfil | `PerfilService.SalvarPermissoesAsync` | `Invalidate(idPerfil)` — **cirúrgica** |
| TTL natural | `IMemoryCache` | 10 min (absoluto) |

---

## 5. Frontend — Objetos e Classes

### 5.1 Mapa dos artefatos (`Empresa.Web/src`)

| Artefato | Arquivo | Papel |
|----------|---------|-------|
| Tipos | `types/api.ts` | `MenuItem`, `MaisAcessadoItem`, `MenuUsuario`, `MenuResponse` (espelham os DTOs) |
| HTTP client | `lib/api.ts` | Axios: injeta Bearer, refresh automático em 401 |
| Mapa de rotas | `lib/routeMap.ts` | `ROUTE_MAP`: `CHAVE_CONTROLE` → rota SPA |
| Service | `services/menuService.ts` | `getMenu()` e `registrarAcesso()` |
| Store | `store/menuStore.ts` | Zustand: estado do menu, cache com TTL, telemetria deduplicada |
| Auth store | `store/authStore.ts` | Token/usuário persistidos (`localStorage: auth-storage`) |
| Componente | `components/layout/SideMenu.tsx` | Árvore Ant Design `Menu` (dinâmica + fallback estático) |
| Componente | `components/layout/MaisAcessadosPanel.tsx` | Painel "Mais Acessados" |
| Componente | `components/layout/AppLayout.tsx` | Sider + Header (link B.I., dropdown usuário) |
| Guard | `components/auth/MenuGuard.tsx` | Bloqueia rota fora do menu → `/sem-permissao` |
| Guard | `components/auth/PrivateRoute.tsx` | Bloqueia não autenticado → `/login` |
| Rotas | `App.tsx` | Composição `PrivateRoute` → `AppLayout` → `MenuGuard` |

### 5.2 Fluxo de dados no frontend

```
AppLayout (renderiza Sider)
   └─ SideMenu ──useEffect──► menuStore.carregarMenu()
                                  │  respeita TTL 10 min (espelha backend)
                                  ▼
                          menuService.getMenu() ──► GET /api/v1/menu (Axios + Bearer)
                                  ▼
                    state: { menu, maisAcessados, usuario, ultimaCarga }
                                  ▼
   SideMenu: buildMenuItems(menu) → itens AntD (grupo=com filhos, folha=navega)
   MaisAcessadosPanel: lista plana com "N acessos"
   AppLayout: usuario.exibeLinkBi → botão "Acesso ao B.I." (abre VITE_BI_URL)
```

### 5.3 `routeMap.ts` — a ponte menu ↔ rotas

As `URL`s do banco são `.aspx` legadas; a SPA usa React Router. A tradução é por `CHAVE_CONTROLE`:

```typescript
export const ROUTE_MAP: Record<string, string> = {
  dashboard: '/',
  usuarios: '/usuarios',
  perfis: '/perfis',
  cadastroUsuarioBi: '/configuracoes/acesso-bi',
  comprasComite: '/compras/comite',
  // ... todo item de menu DEVE ser registrado aqui (obrigatório no code review)
}

resolverRota(chaveControle, url):
  1. ROUTE_MAP[chaveControle]  → rota
  2. fallback: kebab-case da URL ("interna/CadastroPerfil.aspx" → "/cadastro-perfil")
  3. último recurso: "/"
```

### 5.4 `menuStore.ts` — estado e cache (Zustand)

```typescript
interface MenuState {
  menu: MenuItem[]
  maisAcessados: MaisAcessadoItem[]
  usuario: MenuUsuario | null
  carregando: boolean
  erro: string | null
  ultimaCarga: number | null
  carregarMenu: (force?: boolean) => Promise<void>  // TTL 10 min; force ignora
  registrarAcesso: (chaveControle: string) => void  // fire-and-forget
  invalidar: () => void
}
```

- **Cache cliente**: `carregarMenu()` não refaz a chamada se `Date.now() - ultimaCarga < 10 min` (alinhado ao TTL do backend, RF07.2/D-08).
- **Telemetria deduplicada** (`chavesRegistradas: Set<string>`): registra cada chave **1 vez por sessão** — reproduz a semântica do legado "só conta ao abrir aba nova" (RN-07/D-11).
- **Falha silenciosa**: erro de carga grava `erro` e o `SideMenu` usa o **fallback estático** (RF08.3/D-09); erro de telemetria só loga no console.
- `invalidar()` limpa tudo (usado no logout).

### 5.5 `SideMenu.tsx` — a árvore Ant Design

- Converte `MenuItem[]` em itens do `<Menu theme="dark" mode="inline">`:
  - **com filhos** → submenu (só expande/colapsa — RN-06);
  - **folha** → item clicável: `registrarAcesso(chave)` + `navigate(rota)`;
  - deduplica por rota (`seenKeys`); ícone resolvido por palavras-chave (`resolveIcon`: "compra"→carrinho, "financ"→cifrão, "usuario/perfil"→time, etc.).
- `openKeys` = união das chaves abertas pelo usuário + as derivadas da URL atual (menu abre no caminho da página).
- **Skeleton** enquanto carrega pela 1ª vez; **menu estático de fallback** (`FALLBACK_MENU`) se a API falhar, com aviso "Menu offline — usando fallback estático".

### 5.6 `MenuGuard.tsx` — defesa em profundidade no frontend

```typescript
// Coleta as rotas-folha da árvore; se a rota atual não está no conjunto → /sem-permissao
if (menu.length === 0 || ultimaCarga === null) return children   // menu não carregou → deixa o backend barrar
if (!leafRoutes.has(rotaAtual)) return <Navigate to="/sem-permissao" />
```

Aplicado no `App.tsx` aos módulos (`/compras/*`, `/financeiro/*`, `/configuracoes/acesso-bi`, etc.). É uma **camada de UX** — a autoridade real é o backend (RN-12). Repare que `/usuarios` e `/perfis` **não** usam `MenuGuard` hoje (decisão registrada no código: "backend é a autoridade").

### 5.7 `AppLayout.tsx` — o chrome da aplicação

- `Sider` 240px / 64px colapsada; estado persistido em `localStorage('menu-collapsed')` (RF06.7);
- `MaisAcessadosPanel` some quando colapsado (RF02.6);
- Header: botão **"Acesso ao B.I."** condicional a `usuario.exibeLinkBi` (RN-09), abre `import.meta.env.VITE_BI_URL` em nova aba (se não configurada, loga warning);
- Dropdown do usuário: login, "Alterar Senha" (`/alterar-senha`), "Sair" (`logout()` + `invalidar()` do menu + `/login`).

---

## 6. Fluxos de Ponta a Ponta

### 6.1 Login → menu na tela

```
1. POST /api/v1/auth/login → AuthService valida (BCrypt) → emite JWT
      claims: ID_USUARIO, ID_PERFIL, Name, Email, Role
2. authStore persiste (localStorage "auth-storage") → redirect "/"
3. AppLayout renderiza → SideMenu.useEffect → menuStore.carregarMenu()
4. GET /api/v1/menu (Bearer)
   └─ MenuEndpoints extrai claims → MenuService.GetMenuAsync(idUsuario, idPerfil)
        ├─ cache? menu:tree:{gen}:{perfil} → hit: retorna / miss:
        │     AcessoRepository.GetPaginasMenuAsync (CONNECT BY) → BuildTree → cache 10 min
        ├─ GetPaginasMaisAcessadasAsync (top 10, sempre fresco)
        └─ GetMenuUsuarioAsync (login, nome, vínculos B.I.)
5. SideMenu renderiza árvore | MaisAcessadosPanel renderiza top 10
   AppLayout mostra botão B.I. se exibeLinkBi
```

### 6.2 Clique num item do menu (navegação + telemetria)

```
SideMenu.handleSelect(key)
  ├─ é folha? (leafKeys) — grupo só expande/colapsa (RN-06)
  ├─ menuStore.registrarAcesso(chaveControle)
  │     ├─ já registrada nesta sessão? → não faz nada (dedupe, RN-07)
  │     └─ POST /api/v1/menu/acessos { chaveControle }  (fire-and-forget)
  │           └─ MenuService: valida permissão (RN-12) → MERGE upsert (RN-08)
  │              (qualquer falha → só log; navegação nunca é afetada)
  └─ navigate(rota) → React Router renderiza a página
        └─ MenuGuard confere se a rota está nas folhas do menu
              └─ não está → /sem-permissao
```

### 6.3 Admin altera permissões → menu atualiza

```
PerfilPage salva permissões
  → PUT /api/v1/perfis/{id}/paginas
  → PerfilService.SalvarPermissoesAsync
       ├─ repositório: INSERT/DELETE em ACESSO_PERFIL_PAGINA
       └─ _menuService.Invalidate(idPerfil)     // remove cache do perfil
Próximo GET /api/v1/menu de qualquer usuário desse perfil → cache miss → árvore nova
Frontend: após TTL de 10 min ou carregarMenu(force: true)
```

### 6.4 Acesso direto por URL (sem passar pelo menu)

```
Usuário digita /compras/comite sem permissão
  1. MenuGuard: rota não está nas folhas → redirect /sem-permissao (UX)
  2. Se a página chamar a API: endpoint protegido por
     RequireAuthorization("PaginaAcesso") → PaginaAutorizacaoHandler
     → GetPaginaByChaveAsync(chave, idPerfil) → null → 403 (autoridade real)
```

---

## 7. Regras de Negócio — Onde Estão no Código

| RN | Regra | Implementação |
|----|-------|---------------|
| RN-01 | Item só aparece com vínculo perfil×página | `GetPaginasMenuAsync` (`START WITH ... ACESSO_PERFIL_PAGINA`) |
| RN-02 | Só `ATIVO='S'` entra | `WHERE p.ATIVO='S'` (folhas **e** ancestrais — corrige D-06) |
| RN-03 | Pai implícito aparece se algum filho for permitido | `CONNECT BY PRIOR` sobe todos os ancestrais |
| RN-04 | Hierarquia self-reference, profundidade ilimitada | `ID_PAGINA_PAI` + `BuildTree` recursivo |
| RN-05 | Ordenação por `ORDEM` em cada nível | `ORDER BY` no SQL + `OrderBy(p => p.Ordem)` por nível no `BuildTree` |
| RN-06 | Grupo expande; folha navega | `SideMenu` (submenu vs item) |
| RN-07 | Mais Acessados = top 10 por `QUANTIDADE` | `FETCH FIRST 10 ROWS ONLY` + dedupe de telemetria por sessão no `menuStore` |
| RN-08 | Telemetria é upsert (1ª vez qtd=1, depois +1) | `MERGE` em `RegistraAcessoPaginaAsync` |
| RN-09 | Link B.I. só com vínculo `USUARIO_EMPRESA` | `VINCULOS > 0 → ExibeLinkBi` → botão no `AppLayout` |
| RN-12 | Autorização dupla (menu + guard) | `MenuGuard` (frontend) + policy `PaginaAcesso`/`PaginaAutorizacaoHandler` (backend) |
| RN-14 | Logout limpa sessão | `authStore.logout()` + `menuStore.invalidar()` |

Cache/erros (decisões D-*): menu remontado por request no legado → **cache 10 min por perfil** (D-08); menu vazio silencioso → **500 propagado** (D-03); telemetria falha → **nunca quebra navegação** (RF03.5); chave sem permissão na telemetria → **ignorada silenciosamente** (D-15).

---

## 8. Testes Existentes (referência de comportamento)

| Arquivo | O que cobre |
|---------|-------------|
| `Empresa.Tests/Services/MenuServiceTests.cs` | Montagem da árvore (3 níveis), raízes com pai ausente, cache, telemetria com/sem permissão |
| `Empresa.Tests/Services/PerfilServiceTests.cs` / `PerfilPermissoesTests.cs` | Permissões e invalidação de cache (`IMenuService` mockado) |
| `Empresa.Tests/Authorization/PaginaAutorizacaoHandlerTests.cs` | Policy `PaginaAcesso`: 200/403 |
| `Empresa.Tests/Repositories/AcessoRepositoryTests.cs` | SQLs via `FakeDbConnection` |
| `Empresa.Web/tests/menuStore.test.ts` | TTL do cache, force reload, erro → estado vazio, dedupe de telemetria, `invalidar()` |

Rodar: `dotnet test` (backend) · `cd Empresa.Web && npm run test` (frontend).

---

## 9. Guia Prático — Como Adicionar um Novo Item de Menu

Checklist completo (a ordem importa):

1. **Banco** — insira a página em `ACESSO_CADASTRO_PAGINA` definindo: `CHAVE_CONTROLE` única (camelCase, ex.: `relatorioVendas`), `TITULO_MENU`, `TITULO_ABA`, `URL` (pode manter a convenção legada), `ID_PAGINA_PAI` (grupo; `NULL` = raiz), `ORDEM` dentro do nível, `ATIVO='S'`.
2. **Permissão** — vincule aos perfis em `ACESSO_PERFIL_PAGINA` (ou use a tela **Perfis** → árvore de páginas, que faz isso e já invalida o cache).
3. **Rota SPA** — registre a chave em `Empresa.Web/src/lib/routeMap.ts` → `ROUTE_MAP['relatorioVendas'] = '/vendas/relatorio'` (**obrigatório no code review**).
4. **Rota no router** — adicione o `<Route>` em `App.tsx`, envolvendo em `<MenuGuard>` para defesa em profundidade.
5. **Endpoint protegido** (se a página consumir API própria) — proteja com `.RequireAuthorization("PaginaAcesso")` e exponha a `chaveControle` como route value ou query string.
6. **Ícone** (opcional) — inclua palavra-chave no `resolveIcon` do `SideMenu.tsx`.
7. **Cache** — se inseriu direto no banco (sem passar pela API), o cache expira sozinho em 10 min; via API (CRUD de páginas) a invalidação é automática.
8. **Testes** — atualize/adicione casos em `MenuServiceTests.cs` e `menuStore.test.ts` quando o comportamento mudar.

---

## 10. Glossário Rápido

| Termo | Significado |
|-------|-------------|
| `CHAVE_CONTROLE` | Identificador lógico único da página — moeda de autorização |
| Pai implícito | Grupo exibido porque algum filho é permitido (RN-03) |
| Telemetria | Contador usuário×página que alimenta o "Mais Acessados" |
| Geração (cache) | Contador em `menu:tree:generation` que invalida globalmente o cache ao ser incrementado |
| Fallback estático | Menu hardcoded exibido pelo `SideMenu` quando a API falha |
| `MenuGuard` / `PaginaAcesso` | Guards de rota no frontend / policy de autorização no backend (RN-12) |
| RN-xx / D-xx / RFxx | Regras de negócio / defeitos-decisões / requisitos funcionais — rastreáveis em `docs/menus.md` e `docs/PRD_MENU_LATERAL.md` |

---

*Documento gerado por leitura do código-fonte em 02/08/2026. Em caso de divergência, o código é a fonte de verdade; regras de negócio rastreadas em `docs/menus.md` (legado) e `docs/PRD_MENU_LATERAL.md`.*
