# 📘 menus.md — Especificação Técnica: Montagem do Menu Lateral Direito

> **Sistema**: TreisTecnovin — Gestão Empresarial (ERP/BI)
> **Stack Legada**: .NET Framework 4.8 · ASP.NET Web Forms · DevExpress v16.2 (`ASPxTreeView`) · Oracle via `System.Data.OracleClient` (SQL inline, sem procedures para o menu)
> **Propósito deste documento**: Especificação autocontida (engenharia reversa completa) para que **outra IA / outro time consiga reproduzir o comportamento funcional do menu em uma nova stack** (ex.: ASP.NET Core Web API + Blazor/React), preservando 100% das regras de negócio e o banco Oracle intacto (Regra de Ouro nº 1 do CLAUDE.md).
> **Data da engenharia reversa**: 01/08/2026 — verificado linha a linha contra o código-fonte.

---

## 1. Visão Geral

O "menu direito" **não fica na MasterPage**. Ele vive na página shell **`Principal.aspx`**, dentro de um `ASPxSplitter` DevExpress. O pane nomeado `MENU` (400px) é declarado **depois** do pane `CONTEUDO`, por isso renderiza **à direita** da tela.

O pane `MENU` contém:

1. **Barra do usuário** — login exibido, link condicional para o B.I., botão de troca de senha (popup) e botão sair.
2. **Painel "Menu"** (`ASPxRoundPanel1`) → `ASPxTreeView ID="treMenu"` — **árvore hierárquica de páginas filtrada pelo PERFIL do usuário logado**.
3. **Painel "Mais Acessados"** (`ASPxRoundPanel2`) → `ASPxTreeView ID="treMaisAcessados"` — **lista plana com o top 10 de páginas mais abertas pelo USUÁRIO**.

Clicar num nó **folha** abre a página em uma **aba com iframe** (sistema de abas caseiro `tab-view.js`), registra estatística de acesso via callback e **colapsa o menu/topo** (a página abre em tela cheia). Clicar num nó **grupo** apenas expande/colapsa.

### Cadeia de camadas (legado)

```
Principal.aspx (ASPxTreeView + JS)
        │
Principal.aspx.cs (CarregaPaginas / PreencheFilhos / callAtualizar_Callback)
        │
Treis.Data.DaoAcesso (SQL inline — System.Data.OracleClient)
        │
Oracle: ACESSO_CADASTRO_PAGINA · ACESSO_PERFIL_PAGINA · ACESSO_VISUALIZACAO_PAGINA
        · ACESSO_CADASTRO_USUARIO · USUARIO_EMPRESA
```

> ⚠️ `Treis.Service` **não participa** do menu (é um Windows Service de e-mails/avisos). Não há stored procedures/packages envolvidas na montagem do menu — apenas SQL inline.

---

## 2. Domínios Identificados

| Domínio | Responsabilidade | Entidades/Conceitos |
|---|---|---|
| **Acesso/Segurança** (P0) | Autenticação, perfis, permissões página×perfil | `Usuario`, `Perfil` (implícito via `ID_PERFIL`), `Pagina` |
| **Navegação** | Catálogo hierárquico de páginas do menu (self-reference) | `Pagina` (`ID_PAGINA_PAI`, `ORDEM`, `ATIVO`) |
| **Telemetria de uso** | Contador de acessos por usuário×página → "Mais Acessados" | `VisualizacaoPagina` |
| **Integração B.I.** | Vínculo usuário×empresa que habilita o link do B.I. no topo do menu | `UsuarioEmpresa` |

Conceitos-chave do domínio:

- **`CHAVE_CONTROLE`**: identificador lógico único da página (string). É a "moeda" de autorização — usada tanto no menu quanto no *guard* de cada página interna (`GetPaginaByChave`). Deve ser preservada na nova stack.
- **Usuário tem exatamente 1 perfil** (`ACESSO_CADASTRO_USUARIO.ID_PERFIL`). Permissão é por perfil, nunca por usuário.
- **Hierarquia de páginas**: auto-relacionamento `ID_PAGINA_PAI` → profundidade ilimitada (a montagem em C# é recursiva).

---

## 3. Artefatos Envolvidos (Legado)

| Artefato | Caminho | Papel |
|---|---|---|
| `Principal.aspx` | `Treis.Web\Principal.aspx` | Markup: splitter, panes `TOPO`/`CONTEUDO`/`MENU`, os 2 TreeViews, callback, popup de senha, JS `AbrePagina`/`AddAba` |
| `Principal.aspx.cs` | `Treis.Web\Principal.aspx.cs` | Code-behind: `Page_Load` (l.19), `CarregaPaginas` (l.50), `PreencheFilhos` (l.72), `callAtualizar_Callback` (l.45), `btnSair_Click` (l.82), `btnSalvarSenha_Click` (l.93) |
| `Default.aspx.cs` | `Treis.Web\Default.aspx.cs` | Login — popula `Session["User"]` / `Session["UserLogado"]`, `Session.Timeout = 60` |
| `DaoAcesso.cs` | `Treis.Data\DaoAcesso.cs` | Todos os SQLs do menu: `GetPaginas(idPerfil)` (l.91), `GetPaginas()` (l.163), `GetPaginaByChave` (l.203), `GetUsuario` (2 sobrecargas), `GetPaginasAcessadas` (l.445), `RegistraAcessoMenu` (l.392), `AlterarSenha` (l.502) |
| `DaoBase.cs` | `Treis.Data\DaoBase.cs` | Conexão (`GetConnectionString("Oracle")`), helpers genéricos |
| `Pagina.cs` | `Treis.Data\acesso\Pagina.cs` | Entidade de menu |
| `Usuario.cs` | `Treis.Data\acesso\Usuario.cs` | Entidade de sessão |
| `tab-view.js` | `Treis.Web\content\js\tab-view.js` | Sistema de abas (máx. 10 abas — `tabView_maxNumberOfTabs`, l.5; `createNewTab` l.257; `tabClick` l.88) |
| `rotina.js` | `Treis.Web\content\js\rotina.js` | `MostraCarregando` — overlay de loading das abas (l.61–99) |
| `estilosPrincipal.css` | `Treis.Web\content\css\estilosPrincipal.css` | `.fonteLogout`, `.boxes`, `.window` (overlay z-index 9000/9999) |
| `Web.config` | `Treis.Web\Web.config` | Connection string `Oracle` (l.44–48) |
| `Interna.Master` | `Treis.Web\interna\Interna.Master` | MasterPage das páginas internas — **NÃO contém menu** |

### Controles DevExpress usados (referência de UI)

| Controle | ID / Config | Equivalente sugerido na nova stack |
|---|---|---|
| `ASPxSplitter` | `splitter`, tema RedWine, panes colapsáveis | Layout flex/grid com painel lateral direito colapsável |
| `ASPxTreeView` | `treMenu` (hierárquico) e `treMaisAcessados` (plano), tema Aqua | TreeView Blazor/MUI TreeView/AG Grid tree, ou `<nav>` recursivo |
| `ASPxCallback` | `callAtualizar` — registra acesso | `POST /api/v1/menu/acessos` (fire-and-forget) |
| `ASPxPopupControl` | `pnlAlterarSenha`, tema Office2010Blue | Modal de troca de senha |
| Abas caseiras | `tab-view.js` + iframes | Tabs SPA (router) — iframe não é mais necessário |

---

## 4. Modelo de Dados — Tabelas Oracle Envolvidas

> ⚠️ Regra de Ouro nº 1: estas tabelas **não podem ser alteradas**. A nova stack consome exatamente este modelo.

### 4.1 `ACESSO_CADASTRO_PAGINA` — catálogo de páginas/itens de menu

| Coluna | Tipo (inferido) | Descrição |
|---|---|---|
| `ID_PAGINA` | NUMBER (PK) | Identificador da página |
| `URL` | VARCHAR2 | URL relativa da página legada (ex.: `interna/CadastroPerfil.aspx`) |
| `TITULO_ABA` | VARCHAR2 | Título exibido na aba quando a página abre |
| `CHAVE_CONTROLE` | VARCHAR2 (única, lógica) | Chave de autorização/identidade da página |
| `TITULO_MENU` | VARCHAR2 | Texto exibido no nó do menu |
| `ID_PAGINA_PAI` | NUMBER (FK → `ID_PAGINA`, nullable) | Auto-relacionamento; `NULL` = nó raiz |
| `ATIVO` | CHAR(1) `'S'/'N'` | Só `'S'` entra no menu |
| `ORDEM` | NUMBER | Ordenação crescente dentro do nível |
| `TOOLTIP` | VARCHAR2 | Tooltip do nó |

### 4.2 `ACESSO_PERFIL_PAGINA` — permissões (N:N perfil↔página)

| Coluna | Tipo | Descrição |
|---|---|---|
| `ID_PERFIL` | NUMBER (FK) | Perfil |
| `ID_PAGINA` | NUMBER (FK) | Página permitida ao perfil |

Manutenção feita pela tela `CadastroPerfil` via `SalvarPerfilPagina` (INSERT) e `DeletaVinculoPaginaPerfil` (DELETE).

### 4.3 `ACESSO_CADASTRO_USUARIO` — usuários

| Coluna | Tipo | Descrição |
|---|---|---|
| `ID_USUARIO` | NUMBER (PK) | Identificador |
| `NOME` | VARCHAR2 | Nome |
| `LOGIN` | VARCHAR2 | Login |
| `SENHA` | VARCHAR2 | ⚠️ **Texto plano no legado** — na nova stack: hash (bcrypt/PBKDF2) |
| `ID_PERFIL` | NUMBER (FK) | Perfil único do usuário |
| `ATIVO` | CHAR(1) | Só `'S'` autentica |
| `QUANTIDADE_ACESSO` | NUMBER | Incrementado a cada login |
| `ATUALIZA_SENHA` | CHAR(1) | `'S'` força troca de senha no login |
| `DATA_HORA_ULTIMO_ACESSO` | DATE | Atualizado no login (`sysdate`) |

### 4.4 `ACESSO_VISUALIZACAO_PAGINA` — telemetria ("Mais Acessados")

| Coluna | Tipo | Descrição |
|---|---|---|
| `ID_VISUALIZACAO_PAGINA` | NUMBER (PK) | Identificador |
| `ID_USUARIO` | NUMBER (FK) | Usuário |
| `ID_PAGINA` | NUMBER (FK) | Página acessada |
| `DATA_HORA` | DATE | Primeiro acesso (`sysdate` no INSERT) |
| `QUANTIDADE` | NUMBER | Contador incrementado a cada abertura |

### 4.5 `USUARIO_EMPRESA` — vínculo B.I.

| Coluna | Tipo | Descrição |
|---|---|---|
| `ID_USUARIO_EMPRESA` | NUMBER (PK) | Se preenchido para o usuário → link B.I. visível |
| `ID_USUARIO` | NUMBER (FK) | Usuário |
| `EMPRESA` | NUMBER/VARCHAR2 | Empresa vinculada |

### 4.6 Diagrama de relacionamento (lógico)

```
ACESSO_CADASTRO_USUARIO ──(ID_PERFIL)──► [PERFIL]
        │ 1                                  │ 1
        │                                    │
        ▼ N                                  ▼ N
ACESSO_VISUALIZACAO_PAGINA          ACESSO_PERFIL_PAGINA
        │ N                                  │ N
        └────────► ACESSO_CADASTRO_PAGINA ◄──┘
                        │ ▲
                        └─┘ (self-reference: ID_PAGINA_PAI)

ACESSO_CADASTRO_USUARIO 1──N USUARIO_EMPRESA  (habilita link B.I.)
```

---

## 5. Consultas SQL (contratos de dados a reproduzir)

> Na nova stack, reescrever com **Dapper + Oracle.ManagedDataAccess.Core + bind variables** (o legado concatena valores via `String.Format` — SQL injection).

### 5.1 Menu principal — `GetPaginas(IDPerfil)` (`DaoAcesso.cs:91–161`)

```sql
SELECT DISTINCT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE,
       ACP.TITULO_MENU, ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
FROM (
    -- páginas ativas permitidas ao perfil
    SELECT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE,
           ACP.TITULO_MENU, ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
      FROM acesso_cadastro_pagina ACP
     INNER JOIN ACESSO_PERFIL_PAGINA APP ON ACP.ID_PAGINA = APP.ID_PAGINA
     WHERE ACP.ATIVO = 'S'
       AND APP.ID_PERFIL = :idPerfil
) Temp
INNER JOIN acesso_cadastro_pagina ACP ON ACP.ID_PAGINA = Temp.ID_PAGINA_PAI  -- traz os PAIS
UNION
SELECT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE,
       ACP.TITULO_MENU, ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
  FROM acesso_cadastro_pagina ACP
 INNER JOIN ACESSO_PERFIL_PAGINA APP ON ACP.ID_PAGINA = APP.ID_PAGINA
 WHERE ACP.ATIVO = 'S'
   AND APP.ID_PERFIL = :idPerfil
```

**Semântica crítica**:
- O primeiro SELECT do UNION traz **os pais das páginas permitidas, mesmo que o pai não tenha vínculo próprio no perfil** — assim o grupo aparece no menu quando qualquer filho é permitido.
- O pai trazido pelo join **não é filtrado por `ATIVO='S'` nem por perfil** (comportamento legado — decidir se preserva ou corrige na migração).
- O SQL só garante a subida de **1 nível** de pai. Como na prática a hierarquia tem 2 níveis (grupo → página), funciona; hierarquias ≥3 níveis exigiriam `CONNECT BY PRIOR` / CTE recursiva na nova implementação.

### 5.2 Mais acessados — `GetPaginasAcessadas(IdUsuario)` (`DaoAcesso.cs:445–500`)

```sql
SELECT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE,
       ACP.TITULO_MENU, ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
  FROM acesso_cadastro_pagina ACP
 INNER JOIN ACESSO_VISUALIZACAO_PAGINA AVP ON AVP.ID_PAGINA = ACP.ID_PAGINA
 WHERE ACP.ATIVO = 'S'
   AND AVP.ID_USUARIO = :idUsuario
 ORDER BY AVP.QUANTIDADE DESC
-- Limite aplicado em C#: while (reader.Read() && contador < 11)  → TOP 10
-- Na nova stack: FETCH FIRST 10 ROWS ONLY
```

⚠️ **Não filtra por perfil** — página cuja permissão foi revogada continua aparecendo em "Mais Acessados" (furo do legado; recomenda-se corrigir na nova stack com JOIN em `ACESSO_PERFIL_PAGINA`).

### 5.3 Registro de acesso — `RegistraAcessoMenu(chaveControle, idUsuario)` (`DaoAcesso.cs:392–443`)

```sql
-- 1) Verifica existência
SELECT ID_VISUALIZACAO_PAGINA
  FROM ACESSO_VISUALIZACAO_PAGINA
 WHERE ID_PAGINA = (SELECT ID_PAGINA FROM acesso_cadastro_pagina WHERE CHAVE_CONTROLE = :chave)
   AND ID_USUARIO = :idUsuario;

-- 2a) Não existe → INSERT
INSERT INTO acesso_visualizacao_pagina (id_usuario, id_pagina, data_hora, quantidade)
VALUES (:idUsuario,
        (SELECT ID_PAGINA FROM acesso_cadastro_pagina WHERE CHAVE_CONTROLE = :chave),
        sysdate, 1);

-- 2b) Existe → UPDATE
UPDATE acesso_visualizacao_pagina
   SET quantidade = quantidade + 1
 WHERE id_visualizacao_pagina = :idVisualizacao;
```

> Na nova stack pode ser um único `MERGE` (upsert) — o efeito líquido deve ser idêntico.

### 5.4 Guard de autorização por página — `GetPaginaByChave(chave, idPerfil)` (`DaoAcesso.cs:203–258`)

```sql
SELECT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE,
       ACP.TITULO_MENU, ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
  FROM acesso_cadastro_pagina ACP
 INNER JOIN ACESSO_PERFIL_PAGINA APP ON APP.ID_PAGINA = ACP.ID_PAGINA
 WHERE ACP.CHAVE_CONTROLE = :chave
   AND APP.ID_PERFIL = :idPerfil
```

Usado por **~60 páginas internas** como defesa em profundidade: cada página define `nomeTelaAtual = "<chaveControle>"` e no `Page_Load`, se o retorno for nulo → redirect para `~/interna/SemPermissao.aspx?pag=...`. **Na nova stack isso vira política de autorização por endpoint/rota** (ex.: `[Authorize(Policy = "Pagina:<CHAVE_CONTROLE>")]` + `IAuthorizationHandler` que consulta o vínculo perfil×página).

### 5.5 Suporte (contexto do pane do menu)

```sql
-- Visibilidade do link B.I. (GetUsuario(id) — DaoAcesso.cs:28–35)
SELECT U.ID_USUARIO, U.NOME, U.LOGIN, BI.EMPRESA, BI.ID_USUARIO_EMPRESA
  FROM ACESSO_CADASTRO_USUARIO U
  LEFT JOIN USUARIO_EMPRESA BI ON BI.ID_USUARIO = U.ID_USUARIO
 WHERE U.ID_USUARIO = :idUsuario;
-- Regra: se ID_USUARIO_EMPRESA for NULL/vazio → link B.I. oculto

-- Login (GetUsuario(login, senha) — DaoAcesso.cs:297–334)
SELECT ID_USUARIO, NOME, LOGIN, ID_PERFIL, QUANTIDADE_ACESSO, ATUALIZA_SENHA
  FROM ACESSO_CADASTRO_USUARIO
 WHERE LOGIN = :login AND SENHA = :senha AND ATIVO = 'S';

-- Estatística de login (SalvaAcessoUsuario — DaoAcesso.cs:37–63)
UPDATE ACESSO_CADASTRO_USUARIO
   SET DATA_HORA_ULTIMO_ACESSO = sysdate,
       QUANTIDADE_ACESSO = QUANTIDADE_ACESSO + 1
 WHERE ID_USUARIO = :idUsuario;

-- Troca de senha via popup do menu (AlterarSenha — DaoAcesso.cs:502)
UPDATE ACESSO_CADASTRO_USUARIO
   SET SENHA = :novaSenha, ATUALIZA_SENHA = 'N'
 WHERE ID_USUARIO = :idUsuario;
```

---

## 6. Entidades / DTOs

### Legado — `Treis.Data.Acesso.Pagina` (`acesso\Pagina.cs:8–18`)

| Propriedade | Tipo | Origem (coluna) |
|---|---|---|
| `IDPagina` | `int` | `ID_PAGINA` |
| `Url` | `string` | `URL` |
| `TituloAba` | `string` | `TITULO_ABA` |
| `ChaveControle` | `string` | `CHAVE_CONTROLE` |
| `TituloMenu` | `string` | `TITULO_MENU` |
| `IDPaginaPai` | `int?` | `ID_PAGINA_PAI` |
| `Ordem` | `int` | `ORDEM` |
| `ToolTip` | `string` | `TOOLTIP` |

### Legado — `Treis.Data.acesso.Usuario` (`acesso\Usuario.cs:8–16`)

`IDUsuario:int, Nome:string, Login:string, IDPerfil:int, QuantidadeAcesso:int, Atualiza_Senha:string`

> Curiosidade/armadilha do legado: existem **dois namespaces** com casing diferente (`Treis.Data.Acesso` e `Treis.Data.acesso`). Unificar na nova stack.

### Sugestão de DTO para a nova API (árvore já montada no backend)

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
      "filhos": [ /* recursivo, ordenado por 'ordem' */ ]
    }
  ],
  "maisAcessados": [
    { "idPagina": 42, "chaveControle": "...", "tituloMenu": "...", "tituloAba": "...", "url": "...", "tooltip": "...", "quantidade": 87 }
  ],
  "usuario": { "login": "fulano", "exibeLinkBi": true }
}
```

> Na nova UI, `url` da tela legada deve ser mapeada para a **rota SPA** correspondente (manter `CHAVE_CONTROLE` como chave de mapeamento rota↔permissão).

---

## 7. Fluxo Completo (passo a passo do legado)

1. **Login** (`Default.aspx.cs`, `btnEnviar_Click`): `GetUsuario(login, senha)` → sucesso: `Session["UserLogado"]=true`, `Session["User"]=Usuario`, `Session.Timeout=60`; se `ATUALIZA_SENHA='S'` → redirect para troca de senha; grava último acesso (`SalvaAcessoUsuario`); redirect para `Principal.aspx`.
2. **`Principal.Page_Load`** (l.19–43): se `Session["UserLogado"]==false` → redirect login com `?url=` de retorno. Caso contrário: recupera `Usuario` da sessão; `GetUsuario(id)` decide visibilidade do `lnkBi` (`ID_USUARIO_EMPRESA` vazio → oculto); exibe login em `lblUsuarioLogado`; chama `CarregaPaginas()`. **Qualquer exceção → redirect silencioso ao login.** O menu é remontado a **cada Page_Load** (sem cache, sem check de PostBack).
3. **`CarregaPaginas`** (l.50–70):
   - `treMenu.Nodes.Clear()` → `GetPaginas(IDPerfil)` (SQL 5.1);
   - Em memória: filtra **raízes** (`IDPaginaPai == null`), ordena por `Ordem`, cria nó `TreeViewNode(text: TITULO_MENU, name: CHAVE_CONTROLE, imageUrl: null, navigateUrl: null, target: URL + "|" + TITULO_ABA)` — **o campo `target` é abusado como transportador `"url|tituloAba"` para o JavaScript**;
   - `PreencheFilhos` (l.72–81): recursão por `IDPaginaPai == idPai`, ordenando cada nível por `Ordem` — profundidade ilimitada;
   - `treMaisAcessados.Nodes.Clear()` → `GetPaginasAcessadas(IDUsuario)` (SQL 5.2) → nós planos (sem filhos).
4. **Render**: `ASPxTreeView` serializa; evento client-side `NodeClick → AbrePagina(s, e)`.
5. **Clique no nó** (JS em `Principal.aspx:23–50`):
   ```javascript
   function AbrePagina(s, e) {
       if (e.node.nodes.length == 0) {              // FOLHA → abre página
           var n = e.node.target.split("|");        // ["url", "tituloAba"]
           AddAba(n[0], e.node.index, n[1], e.node.name);  // name = CHAVE_CONTROLE
       } else {
           e.node.SetExpanded(!e.node.GetExpanded()); // GRUPO → expande/colapsa
       }
   }
   ```
   `AddAba(url, index, tituloAba, chave)`:
   - Se a aba (deduplicada **por título**) ainda não existe:
     1. `callAtualizar.PerformCallback(chave)` → servidor executa `RegistraAcessoMenu` (SQL 5.3);
     2. `OcultaMenu()` / `OcultaTopo()` → colapsam os panes `MENU` e `TOPO` do splitter (página em tela cheia);
     3. `createNewTab(...)` cria aba com `<iframe src="{url}">` + overlay de loading (máx. **10 abas**);
   - Se já existe: apenas `showTab(...)` + oculta menu/topo.
6. **Retorno ao menu** (`tab-view.js`, `tabClick` l.88–100): clicar na aba índice 0 ("Pagina Inicial") → `MostraMenu()`/`MostraTopo()`; qualquer outra aba → oculta. **O menu direito só é visível na aba inicial.**
7. **Defesa em profundidade**: a página aberta no iframe valida a própria permissão via `GetPaginaByChave(chaveControle, IDPerfil)` (SQL 5.4); nulo → `SemPermissao.aspx`.

---

## 8. Regras de Negócio (RN) — Contrato a Preservar

| # | Regra | Fonte |
|---|---|---|
| RN-01 | Item de menu aparece **somente** se existir vínculo em `ACESSO_PERFIL_PAGINA` para o `ID_PERFIL` do usuário logado. Permissão é **por perfil**, nunca por usuário. Usuário tem exatamente 1 perfil. | SQL 5.1 |
| RN-02 | Somente páginas com `ATIVO = 'S'` entram no menu. (Exceção legada: pais trazidos pelo join do UNION não passam por esse filtro.) | SQL 5.1 |
| RN-03 | **Pai implícito**: o nó pai/grupo aparece automaticamente se **qualquer** filho for permitido, mesmo sem vínculo próprio do pai no perfil (garantido 1 nível pelo SQL). | SQL 5.1 |
| RN-04 | Hierarquia via `ID_PAGINA_PAI` (self-reference); raiz = `ID_PAGINA_PAI IS NULL`; profundidade ilimitada (recursão em C#). | `PreencheFilhos` |
| RN-05 | Ordenação por `ORDEM` crescente **em cada nível** da árvore. | `CarregaPaginas`/`PreencheFilhos` |
| RN-06 | Nó **folha** abre a página; nó **com filhos** apenas expande/colapsa (nunca navega, mesmo que tenha URL). | JS `AbrePagina` |
| RN-07 | "Mais Acessados" = **top 10** páginas do usuário, ordenadas por `QUANTIDADE DESC`; contador incrementado a cada **abertura de aba nova** (não a cada visualização de aba já aberta). Lista plana, sem hierarquia. | SQL 5.2/5.3 + `AddAba` |
| RN-08 | Registro de acesso é **upsert**: primeira abertura → INSERT com `quantidade=1` e `data_hora=sysdate`; seguintes → `quantidade+1`. Identificado por (`ID_USUARIO`, `ID_PAGINA` via `CHAVE_CONTROLE`). | SQL 5.3 |
| RN-09 | Link B.I. no topo do menu visível **apenas** se o usuário possui vínculo em `USUARIO_EMPRESA` (`ID_USUARIO_EMPRESA` não nulo). | SQL 5.5 |
| RN-10 | Sessão obrigatória: sem `Session["UserLogado"]=true` → redirect ao login com `?url=` de retorno. Timeout 60 min. | `Page_Load` |
| RN-11 | `ATUALIZA_SENHA='S'` força troca de senha no login; ao salvar nova senha → `'N'`. Popup de troca de senha disponível no próprio pane do menu. | `Default.aspx.cs` / `AlterarSenha` |
| RN-12 | **Autorização dupla**: além do filtro do menu, cada página valida sua `CHAVE_CONTROLE` × perfil (`GetPaginaByChave`); falha → tela "Sem Permissão". Protege contra acesso direto por URL. | SQL 5.4 |
| RN-13 | UX: máx. **10 abas** simultâneas; abas deduplicadas por `TITULO_ABA`; menu/topo colapsam ao abrir página e só reaparecem na aba inicial. | `tab-view.js` |
| RN-14 | Sair: limpa sessão (`UserLogado=false`, `User=null`) e redireciona ao login. | `btnSair_Click` |

---

## 9. Defeitos Latentes do Legado (decidir: corrigir ou preservar na migração)

| # | Defeito | Local | Recomendação |
|---|---|---|---|
| D-01 | SQL injection: todos os SQLs usam `String.Format` com concatenação (inclusive login/senha) | `DaoAcesso.cs` (todo) | **Corrigir** — bind variables obrigatórios (Dapper) |
| D-02 | Senha em texto plano (armazenamento e comparação) | `DaoAcesso.cs:304,508` | **Corrigir** — hash + migração gradual |
| D-03 | Exceções engolidas retornando `null` → menu vazio silencioso | `GetPaginas` l.152–155 | **Corrigir** — logar (Serilog) e propagar erro 500 |
| D-04 | `GetPaginasAcessadas` não popula `Ordem` (fica 0); a ordem final depende da estabilidade do `OrderBy` do LINQ sobre a ordem do SQL | `DaoAcesso.cs:445+` | **Corrigir** — ordenar explicitamente por `QUANTIDADE DESC` |
| D-05 | "Mais Acessados" não filtra por perfil — página revogada continua listada (e clicável; só barra no guard da página) | SQL 5.2 | **Corrigir** — JOIN com `ACESSO_PERFIL_PAGINA` |
| D-06 | Pai trazido pelo UNION não checa `ATIVO='S'` — pai inativo pode aparecer como grupo | SQL 5.1 | **Corrigir** (validar impacto com dados reais) |
| D-07 | Subida de hierarquia limitada a 1 nível no SQL (avô de página permitida não aparece se não tiver vínculo próprio) | SQL 5.1 | Reimplementar com CTE recursiva/`CONNECT BY` se houver ≥3 níveis |
| D-08 | Menu remontado a cada `Page_Load` (2+ queries por request da shell) | `Page_Load` | Nova stack: cache por perfil com invalidação ao salvar perfil |
| D-09 | Catch genérico no `Page_Load` redireciona ao login mascarando qualquer erro | `Principal.aspx.cs:39–42` | **Corrigir** — distinguir "não autenticado" de "erro" |
| D-10 | Campo `target` do TreeViewNode abusado como transportador `"url|tituloAba"` — quebra se título contiver `|` | `CarregaPaginas` | Nova stack: DTO estruturado (JSON) |
| D-11 | Deduplicação de abas por **título** (não por chave) — duas páginas com mesmo `TITULO_ABA` colidem | `tab-view.js` | Nova stack: deduplicar por `CHAVE_CONTROLE` |
| D-12 | Driver depreciado `System.Data.OracleClient` | `DaoBase.cs` | **Corrigir** — `Oracle.ManagedDataAccess.Core` (Regra de Ouro nº 2) |

---

## 10. Blueprint de Reprodução na Nova Stack

### 10.1 Endpoints REST sugeridos (`/api/v1`)

| Método | Rota | Descrição | Fonte legada |
|---|---|---|---|
| `GET` | `/api/v1/menu` | Árvore do menu do usuário autenticado (perfil extraído do JWT). Retorna DTO da seção 6, já hierarquizado e ordenado. | `GetPaginas(idPerfil)` + `CarregaPaginas` |
| `GET` | `/api/v1/menu/mais-acessados` | Top 10 do usuário autenticado (corrigindo D-04/D-05). | `GetPaginasAcessadas` |
| `POST` | `/api/v1/menu/acessos` | Body: `{ "chaveControle": "..." }`. Upsert de telemetria (RN-08). Idempotente por incremento. | `RegistraAcessoMenu` / `callAtualizar` |
| `GET` | `/api/v1/paginas/{chaveControle}/autorizacao` | (Opcional) Checagem de permissão para guard de rota no frontend; no backend, a mesma regra vira `IAuthorizationHandler`. | `GetPaginaByChave` |

### 10.2 Mapeamento de responsabilidades

| Legado | Nova stack |
|---|---|
| `Session["User"]` / `Session["UserLogado"]` | JWT Bearer (claims: `sub`=ID_USUARIO, `perfil`=ID_PERFIL, `login`) — stateless |
| `DaoAcesso` (SQL inline, OracleClient) | `MenuRepository` / `AcessoRepository` (Dapper + ODP.NET Core, bind variables) |
| `CarregaPaginas`/`PreencheFilhos` (code-behind) | `MenuService` (camada de aplicação): monta árvore em memória a partir da lista plana (mesmo algoritmo: raízes = pai nulo; filhos recursivos; ordenar por `ORDEM` em cada nível) |
| `ASPxTreeView` + `AbrePagina` | Componente TreeView SPA; folha → navega para rota mapeada por `CHAVE_CONTROLE`; grupo → toggle |
| Abas com iframe (`tab-view.js`) | Router SPA (tabs opcionais); sem iframe |
| `GetPaginaByChave` em cada página | Política de autorização central: `[Authorize(Policy = "Pagina")]` + handler que valida `CHAVE_CONTROLE` da rota × perfil do token (Regra de Ouro nº 5) |
| Colapso do splitter | Estado de layout do frontend (menu lateral direito colapsável) |

### 10.3 Algoritmo de montagem da árvore (pseudocódigo, fiel ao legado)

```
entrada: lista plana de Pagina (resultado do SQL 5.1)
saída:  árvore

raizes = paginas.filtrar(p => p.idPaginaPai == null).ordenar(p => p.ordem)
para cada raiz:
    no = criarNo(raiz)                 # texto=TITULO_MENU, chave=CHAVE_CONTROLE,
    preencherFilhos(paginas, no, raiz.idPagina)   # tooltip=TOOLTIP, url=URL, aba=TITULO_ABA

preencherFilhos(paginas, noPai, idPai):
    filhos = paginas.filtrar(p => p.idPaginaPai == idPai).ordenar(p => p.ordem)
    para cada filho:
        noFilho = criarNo(filho)
        preencherFilhos(paginas, noFilho, filho.idPagina)   # recursão ilimitada
        noPai.filhos.adicionar(noFilho)
```

### 10.4 Checklist de fidelidade funcional

- [ ] Menu exibe exatamente as mesmas páginas para um dado perfil (comparar saída do SQL 5.1 legado × nova query)
- [ ] Pais implícitos aparecem quando ao menos 1 filho é permitido (RN-03)
- [ ] Ordenação por `ORDEM` em cada nível idêntica (RN-05)
- [ ] Grupo não navega; folha navega (RN-06)
- [ ] "Mais Acessados" limitado a 10, ordenado por quantidade (RN-07) — com correções D-04/D-05 aprovadas
- [ ] Telemetria upsert com efeito idêntico em `ACESSO_VISUALIZACAO_PAGINA` (RN-08)
- [ ] Link B.I. condicional a `USUARIO_EMPRESA` (RN-09)
- [ ] Acesso direto a rota sem permissão bloqueado no backend (RN-12) — nunca confiar só no frontend
- [ ] Nenhuma alteração estrutural nas 5 tabelas Oracle (Regra de Ouro nº 1)

---

## 11. Contrato de Sessão/Autenticação (legado → novo)

| Chave legada | Tipo | Uso no menu | Equivalente JWT |
|---|---|---|---|
| `Session["UserLogado"]` | `bool` | Gate do `Page_Load` | Presença/validade do token |
| `Session["User"]` | `Treis.Data.acesso.Usuario` | `IDPerfil` (filtro do menu), `IDUsuario` (mais acessados/telemetria), `Login` (label) | Claims `sub`, `perfil`, `login` |
| `Session.Timeout = 60` | min | Expiração | `exp` do token (60 min) + refresh |

---

*Documento gerado por engenharia reversa verificada contra: `Treis.Web\Principal.aspx(.cs)`, `Treis.Web\Default.aspx.cs`, `Treis.Data\DaoAcesso.cs`, `Treis.Data\acesso\Pagina.cs`, `Treis.Data\acesso\Usuario.cs`, `Treis.Web\content\js\tab-view.js`, `Treis.Web\content\js\rotina.js`. Nenhum código foi alterado.*
