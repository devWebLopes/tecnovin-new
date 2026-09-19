# Especificação Técnica: Módulo de Acesso (Perfil e Usuário)

> **Documento gerado por engenharia reversa do código-fonte legado.**
> Fonte única de verdade para reconstrução dos painéis em nova stack tecnológica.
> Stack original: ASP.NET WebForms (C#) + DevExpress v16.2 + Oracle DB (OracleClient legado).

---

## Stack Tecnológica Original

| Camada       | Tecnologia                                      |
|--------------|-------------------------------------------------|
| Frontend     | ASP.NET WebForms (.aspx) + DevExpress v16.2     |
| Backend      | C# (.NET Framework) — Code-Behind (.aspx.cs)   |
| Banco        | Oracle (via `System.Data.OracleClient`)         |
| Sessão       | ASP.NET `Session` (timeout: 60 min)             |
| UI Library   | DevExpress ASPxGridView, ASPxTreeView, ASPxPopupControl |
| Theme        | Office2010Blue / Aqua (DevExpress)              |

---

## Autenticação e Sessão (Contexto Geral)

Todo painel interno herda da Master Page `~/interna/Interna.Master`.

### Regras de Sessão (Master Page)
- Em todo `Page_Load` da Master Page, verifica se `Session["UserLogado"] == true`.
- Se `Session["UserLogado"]` for `false`, `null` ou lançar exceção → redireciona para `~/Default.aspx?url={RawUrl}`.
- O objeto `Session["User"]` contém uma instância da classe `Usuario` com os dados do usuário logado.

### Fluxo de Login (`Default.aspx`)
1. Usuário informa **Login** e **Senha** no formulário.
2. Backend chama `DaoAcesso.GetUsuario(login, senha)`.
3. Query executada:
   ```sql
   SELECT ID_USUARIO, NOME, LOGIN, ID_PERFIL, QUANTIDADE_ACESSO, ATUALIZA_SENHA
   FROM ACESSO_CADASTRO_USUARIO
   WHERE LOGIN = '{login}' AND SENHA = '{senha}' AND ATIVO = 'S'
   ```
4. Se retornar `null` → exibe mensagem `"Login ou senha inválido!"`, seta `Session["UserLogado"] = false`.
5. Se retornar usuário válido:
   - Seta `Session["UserLogado"] = true` e `Session["User"] = user`.
   - Se `user.Atualiza_Senha == "S"` → redireciona para `~/interna/acesso/AlteraSenha.aspx?idUsuario={id}`.
   - Caso contrário → registr
   a acesso (`SalvaAcessoUsuario`) e redireciona para `Principal.aspx` (ou URL original via QueryString `url`).

### Controle de Permissão por Página
- Cada página interna, no `Page_Load`, valida se o usuário tem permissão via:
  ```csharp
  new DaoAcesso().GetPaginaByChave(nomeTelaAtual, ((Usuario)Session["User"]).IDPerfil)
  ```
- Se retornar `null` → redireciona para `~/interna/SemPermissao.aspx?pag={chave}`.
- **Chave da tela Perfil:** `"cadastroPerfil"`
- **Chave da tela Usuário:** `"cadastroUsuario"`

---

## 1. Painel: Cad. Perfil

### 1.1. Caminho e Inicialização

- **Navegação:** Módulo de Acesso > Perfil
- **Título exibido na tela:** `"Cadastro de Perfil"`
- **Chave de controle (permissão):** `cadastroPerfil`

#### Artefatos Frontend

| Arquivo | Tipo | Descrição |
|---|---|---|
| `Treis.Web/interna/acesso/CadastroPerfil.aspx` | View (WebForms) | Markup da tela principal do painel de Perfil |
| `Treis.Web/interna/acesso/CadastroPerfil.aspx.cs` | Code-Behind | Lógica de servidor da tela |
| `Treis.Web/interna/acesso/CadastroPerfil.aspx.designer.cs` | Designer | Declarações de controles (gerado automaticamente) |
| `Treis.Web/interna/Interna.Master` | Master Page | Layout/template de todas as páginas internas |
| `Treis.Web/interna/Interna.Master.cs` | Code-Behind Master | Validação de sessão global |
| `Treis.Web/content/js/rotina.js` | JavaScript | Funções JS globais (ex: `FechaBloco`, `MostraCarregando`) |
| `Treis.Web/content/js/jquery-1.7.2.min.js` | JavaScript | jQuery 1.7.2 |

#### Artefatos Backend / Data

| Arquivo | Tipo | Descrição |
|---|---|---|
| `Treis.Data/DaoAcesso.cs` | Repository (DAO) | Todos os métodos de acesso a dados do módulo de Acesso |
| `Treis.Data/DaoBase.cs` | Base Repository | Métodos genéricos de conexão e execução SQL |
| `Treis.Data/acesso/Pagina.cs` | Model / Entidade | Entidade que representa uma página/menu do sistema |
| `Treis.Data/acesso/Usuario.cs` | Model / Entidade | Entidade do usuário logado (usada na sessão) |

---

### 1.2. Regras de Negócio e Validações

#### Grid Principal (lista de Perfis)
- Exibe os perfis cadastrados em um grid (`ASPxGridView`) com paginação de **50 registros por página**.
- Colunas exibidas:

| Campo DB    | Rótulo UI | Tipo     | Editável | Observação                    |
|-------------|-----------|----------|----------|-------------------------------|
| `ID_PERFIL` | CODIGO    | Numérico | Nao      | ReadOnly. Chave primária (PK) |
| `DESCRICAO` | DESCRICAO | Texto    | Sim      | Campo obrigatório             |

- **Ordenação:** Desabilitada (`AllowSort="False"`).
- **Filtro de linha:** Habilitado (`ShowFilterRowMenu="True"`).
- **Confirmação de exclusão:** Habilitada — mensagem: `"Confirma a exclusão?"`.
- **Modo de edição:** Inline (edição direta na linha do grid).

#### Validações do Campo DESCRICAO
- **Obrigatório:** Sim. Mensagem de erro: `"Obrigatório"`.
- **Tipo:** String (sem limite de tamanho explícito na UI).
- **Foco automático:** Ao entrar no modo de edição, o campo `DESCRICAO` recebe foco automaticamente (`OnCellEditorInitialize`).

#### Botões de Ação do Grid

| Botão        | Comportamento |
|--------------|---------------|
| **Novo**     | Adiciona nova linha no grid no modo inline (`gvdDados.AddNewRow()`) |
| **Editar**   | Coloca a linha em modo de edição inline |
| **Salvar**   | Persiste a alteração/inserção via `SqlDataSource` |
| **Cancelar** | Cancela a edição sem salvar |
| **Deletar**  | Exclui o registro com confirmação. Executa `DELETE FROM ACESSO_CADASTRO_PERFIL` |
| **Permissões** | Botão customizado (ícone `permissoes.png`). Abre popup de permissões para o perfil selecionado |

#### Popup de Permissões de Páginas
- Ao clicar em **Permissões** na linha de um perfil:
  1. Via JavaScript, captura o `ID_PERFIL` da linha (`a.cells[0].innerHTML`) e armazena no campo oculto `hidIDPerfil`.
  2. Dispara o botão server-side `btnCarregarTree` (via `btnCarregarTree.DoClick()`).
  3. Server-side (`btnRecarregar_Click`): busca as páginas já vinculadas ao perfil (`DaoAcesso.GetPaginas(idPerfil)`) e monta a `ASPxTreeView` com todas as páginas do sistema, marcando as que o perfil já tem acesso.
- **TreeView de Páginas:**
  - Carrega **todas as páginas ativas** do sistema (`ATIVO = 'S'`) em estrutura hierárquica (pai/filho).
  - Nós pai: páginas sem `ID_PAGINA_PAI` (módulos), ordenados por `ORDEM`.
  - Nós filho: páginas com `ID_PAGINA_PAI`, ordenados por `ORDEM`.
  - Seleção múltipla com checkboxes (`AllowCheckNodes="true"`).
  - Marcação recursiva ao marcar nó pai (`CheckNodesRecursive="true"`).
  - Páginas já permissionadas ao perfil aparecem pré-marcadas (`Checked = true`).

#### Salvar Permissões do Perfil (`btnSalvar_Click`)
- Percorre **todos os nós** da TreeView (recursivamente via `VarreNodes`).
- **Se nó MARCADO:** Chama `DaoAcesso.SalvarPerfilPagina(idPagina, idPerfil)`.
  - SQL: `INSERT INTO acesso_perfil_pagina (id_perfil, id_pagina) VALUES ({idPerfil}, {idPagina})`
  - ATENCAO: Não há verificação de duplicidade antes do INSERT no código (pode gerar erro de constraint no banco se já existir).
- **Se nó DESMARCADO (Unchecked explícito):** Busca a página pela chave de controle e chama `DaoAcesso.DeletaVinculoPaginaPerfil(idPerfil, idPagina)`.
  - SQL: `DELETE FROM acesso_perfil_pagina WHERE id_perfil = {idPerfil} AND id_pagina = {idPagina}`
- Após salvar, fecha o popup (`pnlPermissoes.ShowOnPageLoad = false`).

#### CRUD Principal (via SqlDataSource `sqlPerfil`)

| Operação   | SQL executado |
|------------|---------------|
| **SELECT** | `SELECT ID_PERFIL, DESCRICAO FROM ACESSO_CADASTRO_PERFIL` |
| **INSERT** | `INSERT INTO ACESSO_CADASTRO_PERFIL (ID_PERFIL, DESCRICAO) VALUES (:ID_PERFIL, :DESCRICAO)` |
| **UPDATE** | `UPDATE ACESSO_CADASTRO_PERFIL SET DESCRICAO = :DESCRICAO WHERE ID_PERFIL = :ID_PERFIL` |
| **DELETE** | `DELETE FROM ACESSO_CADASTRO_PERFIL WHERE ID_PERFIL = :ID_PERFIL` |

> ATENCAO: O `ID_PERFIL` no INSERT é passado como parâmetro pelo `SqlDataSource`. A geração do ID deve ser feita por uma trigger Oracle ou sequence no banco (não há lógica de geração no código da aplicação).

---

### 1.3. Modelo de Dados

#### Tabela Principal: `ACESSO_CADASTRO_PERFIL`
     ## Colunas
	 ID_PERFIL, DESCRICAO, ROWID
| Coluna      | Tipo     | Papel | Observação                                   |
|-------------|----------|-------|----------------------------------------------|
| `ID_PERFIL` | NUMBER   | PK    | Identificador único do perfil                |
| `DESCRICAO` | VARCHAR2 | -     | Nome/descrição do perfil. Obrigatório na UI. |

#### Tabela de Vínculo Perfil-Página: `ACESSO_PERFIL_PAGINA`

| Coluna      | Tipo   | Papel | Observação                                   |
|-------------|--------|-------|----------------------------------------------|
| `ID_PERFIL` | NUMBER | FK    | FK para `ACESSO_CADASTRO_PERFIL.ID_PERFIL`   |
| `ID_PAGINA` | NUMBER | FK    | FK para `ACESSO_CADASTRO_PAGINA.ID_PAGINA`   |

- Operações: INSERT e DELETE (sem UPDATE).

#### Tabela de Páginas/Menu: `ACESSO_CADASTRO_PAGINA`
    ## Colunas
	   ID_PAGINA, URL, TITULO_ABA, CHAVE_CONTROLE, TITULO_MENU, ID_PAGINA_PAI, ATIVO, ORDEM, TOOLTIP, ROWID

| Coluna           | Tipo     | Papel | Observação |
|------------------|----------|-------|------------|
| `ID_PAGINA`      | NUMBER   | PK    | Identificador da página |
| `URL`            | VARCHAR2 | -     | URL relativa da página no sistema |
| `TITULO_ABA`     | VARCHAR2 | -     | Título exibido na aba do navegador |
| `CHAVE_CONTROLE` | VARCHAR2 | -     | Chave única para controle de permissão (ex: `cadastroPerfil`) |
| `TITULO_MENU`    | VARCHAR2 | -     | Texto exibido no menu de navegação |
| `ID_PAGINA_PAI`  | NUMBER   | FK    | Auto-relacionamento. NULL = nó raiz (módulo) |
| `ATIVO`          | CHAR(1)  | -     | `'S'` = ativo / `'N'` = inativo |
| `ORDEM`          | NUMBER   | -     | Ordem de exibição no menu |
| `TOOLTIP`        | VARCHAR2 | -     | Texto de tooltip do menu |

#### Relacionamentos

```
ACESSO_CADASTRO_PERFIL (1) ──── (N) ACESSO_PERFIL_PAGINA (N) ──── (1) ACESSO_CADASTRO_PAGINA
                                                                         |
                                        auto-relacionamento: ID_PAGINA_PAI -> ID_PAGINA
```

---

## 2. Painel: Cadastro Usuário

### 2.1. Caminho e Inicialização

- **Navegação:** Módulo de Acesso > Usuário
- **Título exibido na tela:** `"Cadastro de Usuário"`
- **Chave de controle (permissão):** `cadastroUsuario`

#### Artefatos Frontend

| Arquivo | Tipo | Descrição |
|---|---|---|
| `Treis.Web/interna/acesso/CadastroUsuario.aspx` | View (WebForms) | Markup da tela principal do painel de Usuário |
| `Treis.Web/interna/acesso/CadastroUsuario.aspx.cs` | Code-Behind | Lógica de servidor da tela |
| `Treis.Web/interna/acesso/CadastroUsuario.aspx.designer.cs` | Designer | Declarações de controles (gerado automaticamente) |
| `Treis.Web/interna/Interna.Master` | Master Page | Layout/template de todas as páginas internas |
| `Treis.Web/interna/Interna.Master.cs` | Code-Behind Master | Validação de sessão global |
| `Treis.Web/content/js/rotina.js` | JavaScript | Funções JS globais |
| `Treis.Web/content/js/jquery-1.7.2.min.js` | JavaScript | jQuery 1.7.2 |

#### Artefatos Backend / Data

| Arquivo | Tipo | Descrição |
|---|---|---|
| `Treis.Data/DaoAcesso.cs` | Repository (DAO) | Todos os métodos de acesso a dados do módulo de Acesso |
| `Treis.Data/DaoBase.cs` | Base Repository | Métodos genéricos de conexão e execução SQL |
| `Treis.Data/acesso/Usuario.cs` | Model / Entidade | Entidade de usuário |
| `Treis.Data/acesso/Estabelecimento.cs` | Model / Entidade | Entidade de empresa/estabelecimento |
| `Treis.Data/acesso/Pagina.cs` | Model / Entidade | Entidade de página (usada no controle de acesso) |

---

### 2.2. Regras de Negócio e Validações

#### Grid Principal (lista de Usuários)
- Exibe os usuários em um grid (`ASPxGridView`) com paginação de **50 registros por página**.
- Lista ordenada por `NOME` (`ORDER BY NOME` no SELECT).
- Colunas exibidas:

| Campo DB                  | Rótulo UI       | Tipo      | Editável | Observação |
|---------------------------|-----------------|-----------|----------|------------|
| `ID_USUARIO`              | CODIGO          | Numérico  | Nao      | ReadOnly. PK |
| `NOME`                    | NOME            | Texto     | Sim      | Obrigatório |
| `LOGIN`                   | LOGIN           | Texto     | Sim      | Obrigatório. MaxLength: 50 |
| `SENHA`                   | SENHA           | Senha     | Sim      | Obrigatório. MaxLength: 50. Campo mascarado (Password mode) |
| `ID_PERFIL`               | PERFIL          | ComboBox  | Sim      | Obrigatório. Dropdown de `ACESSO_CADASTRO_PERFIL` |
| `ATIVO`                   | ATIVO           | ComboBox  | Sim      | Obrigatório. Valores: `S`=SIM / `N`=NAO |
| `ATUALIZA_SENHA`          | ATUALIZAR SENHA | ComboBox  | Sim      | Nao obrigatório. Valores: `S`=SIM / `N`=NAO |
| `DATA_HORA_ULTIMO_ACESSO` | ULTIMO ACESSO   | Data/Hora | Nao      | ReadOnly. Formato: `dd/MM/yyyy : HH:mm` |
| `QUANTIDADE_ACESSO`       | QTDE ACESSO     | Numérico  | Nao      | ReadOnly. Contador de acessos |

- **Ordenação:** Desabilitada.
- **Filtro de linha:** Habilitado.
- **Confirmação de exclusão:** Habilitada — mensagem: `"Confirma a exclusão?"`.
- **Modo de edição:** Inline.

#### Validações dos Campos

| Campo           | Obrigatório | MaxLength | Regra extra |
|-----------------|-------------|-----------|-------------|
| `NOME`          | Sim         | -         | Recebe foco automático ao entrar no modo de edição |
| `LOGIN`         | Sim         | 50        | - |
| `SENHA`         | Sim         | 50        | Campo mascarado. Ao editar, exibe o valor atual via script JS init |
| `ID_PERFIL`     | Sim         | -         | Deve ser um perfil existente em `ACESSO_CADASTRO_PERFIL` |
| `ATIVO`         | Sim         | -         | Valores aceitos: `'S'` ou `'N'` |
| `ATUALIZA_SENHA`| Nao         | -         | Default no INSERT: `'N'` |

#### Comportamento da Senha na Edição
- Ao inicializar o editor da coluna SENHA (`OnCellEditorInitialize`), um script cliente é injetado para exibir o valor atual no campo:
  ```csharp
  ((ASPxTextBox)e.Editor).ClientSideEvents.Init =
      "function(s, e) {s.SetText('" + ((ASPxTextBox)e.Editor).Text + "');}";
  ```
- CRITICO: A senha é armazenada em **texto puro** no banco (sem hash/criptografia). Aspecto de segurança do sistema legado que DEVE ser corrigido na nova implementação.

#### Botões de Ação do Grid

| Botão             | Comportamento |
|-------------------|---------------|
| **Novo**          | Adiciona nova linha no grid no modo inline |
| **Editar**        | Coloca a linha em modo de edição inline |
| **Salvar**        | Persiste via `SqlDataSource` |
| **Cancelar**      | Cancela sem salvar |
| **Deletar**       | Exclui com confirmação |
| **Ícone Peça** (id=`configurar`) | Abre popup de vínculo com Empresas/Estabelecimentos |

#### Popup de Vínculo com Empresas/Estabelecimentos
- Ao clicar no botão **configurar** (ícone `peca.png`) na linha de um usuário:
  1. Via JavaScript (`PermissoesAcesso`): captura `ID_USUARIO` da linha, armazena em `hidIDUsuario`, exibe loading e dispara `btnCarregarTree.DoClick()`.
  2. Server-side (`btnRecarregar_Click`): carrega todos os estabelecimentos via `DaoAcesso.GetNode(idUsuario)` e monta a `ASPxTreeView`.
- **Fonte de dados:** View Oracle `VW_ESTABELECIMENTO_NEW`.
  - Query: `SELECT EMPRESA, ESTABELECIMENTO, DESCRITIVO FROM VW_ESTABELECIMENTO_NEW`
  - Mapeamento dos campos:
    - `EMPRESA` -> `Estabelecimento.CdEmpresa`
    - `ESTABELECIMENTO` -> `Estabelecimento.CdEstabelecimento`
    - `DESCRITIVO` -> Parse: texto até o primeiro espaço = `DsEmpresa`; texto a partir do "-" = sufixo do `DsEstabelecimento`
- **Estrutura da TreeView:**
  - Nós pai (empresas): registros onde `CdEstabelecimento == 1`.
  - Nós filho (estabelecimentos): todos com o mesmo `CdEmpresa` do pai, ordenados por `CdEstabelecimento`.
  - Nome do nó filho (`TreeViewNode.Name`): `"{CdEmpresa}|{CdEstabelecimento}"` (separador pipe `|`).
  - Cada nó filho é verificado em `ACESSO_USUARIO_EMPRESA_ESTAB` e pré-marcado se já houver vínculo.

#### Salvar Vínculo Usuário-Estabelecimento (`btnSalvar_Click`)
- Percorre **todos os nós** da TreeView recursivamente (`VarreNodes`).
- **Se nó MARCADO:**
  - Verifica existência: `SELECT * FROM ACESSO_USUARIO_EMPRESA_ESTAB WHERE ID_USUARIO={0} AND CD_EMPRESA={1} AND CD_ESTABELECIMENTO={2}`
  - Se **não existir**: `INSERT INTO ACESSO_USUARIO_EMPRESA_ESTAB (ID_USUARIO, CD_EMPRESA, CD_ESTABELECIMENTO) VALUES({idUsuario}, {cdEmpresa}, {cdEstabelecimento})`
- **Se nó DESMARCADO (Unchecked explícito):**
  - Verifica existência (mesmo SELECT).
  - Se **existir**: `DELETE FROM ACESSO_USUARIO_EMPRESA_ESTAB WHERE ID_USUARIO={idUsuario} AND CD_EMPRESA={cdEmpresa} AND CD_ESTABELECIMENTO={cdEstabelecimento}`
- Após salvar, fecha o popup.

> NOTA: Nós pai da TreeView não têm o padrão `"empresa|estabelecimento"` no Name, portanto não acionam INSERT/DELETE. Apenas os nós filhos são processados.

#### CRUD Principal (via SqlDataSource `sqlUsuario`)

| Operação   | SQL executado |
|------------|---------------|
| **SELECT** | `SELECT ID_USUARIO, NOME, LOGIN, SENHA, ID_PERFIL, DATA_HORA_ULTIMO_ACESSO, ATIVO, QUANTIDADE_ACESSO, ATUALIZA_SENHA FROM ACESSO_CADASTRO_USUARIO ORDER BY NOME` |
| **INSERT** | `INSERT INTO ACESSO_CADASTRO_USUARIO (ID_USUARIO, NOME, LOGIN, SENHA, ID_PERFIL, ATIVO, DATA_HORA_ULTIMO_ACESSO, QUANTIDADE_ACESSO, ATUALIZA_SENHA) VALUES (:ID_USUARIO, :NOME, :LOGIN, :SENHA, :ID_PERFIL, :ATIVO, :DATA_HORA_ULTIMO_ACESSO, :QUANTIDADE_ACESSO, :ATUALIZA_SENHA)` |
| **UPDATE** | `UPDATE ACESSO_CADASTRO_USUARIO SET NOME = :NOME, LOGIN = :LOGIN, SENHA = :SENHA, ID_PERFIL = :ID_PERFIL, ATIVO = :ATIVO, ATUALIZA_SENHA = :ATUALIZA_SENHA WHERE ID_USUARIO = :ID_USUARIO` |
| **DELETE** | `DELETE FROM ACESSO_CADASTRO_USUARIO WHERE ID_USUARIO = :ID_USUARIO` |

**Parâmetros e defaults no INSERT:**

| Parâmetro               | Tipo     | Default |
|-------------------------|----------|---------|
| `ID_USUARIO`            | Decimal  | -       |
| `NOME`                  | String   | -       |
| `LOGIN`                 | String   | -       |
| `SENHA`                 | String   | -       |
| `ID_PERFIL`             | Decimal  | -       |
| `ATIVO`                 | String   | -       |
| `DATA_HORA_ULTIMO_ACESSO` | DateTime | -     |
| `QUANTIDADE_ACESSO`     | Decimal  | `0`     |
| `ATUALIZA_SENHA`        | String   | `'N'`   |

> ATENCAO: O UPDATE **não atualiza** `DATA_HORA_ULTIMO_ACESSO` e `QUANTIDADE_ACESSO`. Esses campos são atualizados exclusivamente via `DaoAcesso.SalvaAcessoUsuario()` no momento do login.

#### Dropdown de Perfil no Grid de Usuários
- A coluna PERFIL usa um segundo `SqlDataSource` (`sqlPerfil`) na própria tela:
  ```sql
  SELECT ID_PERFIL, DESCRICAO FROM ACESSO_CADASTRO_PERFIL ORDER BY DESCRICAO
  ```
- Exibe `DESCRICAO`, armazena `ID_PERFIL` (tipo `System.Decimal`).

---

### 2.3. Modelo de Dados

#### Tabela Principal: `ACESSO_CADASTRO_USUARIO`
   ## Colunas
   ID_USUARIO, NOME, LOGIN, SENHA, ID_PERFIL, DATA_HORA_ULTIMO_ACESSO, QUANTIDADE_ACESSO, ATIVO, ID_REPRESENTANTE, CD_USUARIO_ERP, ATUALIZA_SENHA, IP_RESTRITO, ROWID

| Coluna                    | Tipo         | Papel | Observação |
|---------------------------|--------------|-------|------------|
| `ID_USUARIO`              | NUMBER       | PK    | Identificador único do usuário |
| `NOME`                    | VARCHAR2     | -     | Nome completo. Obrigatório |
| `LOGIN`                   | VARCHAR2(50) | -     | Login de acesso. Obrigatório. Max 50 chars |
| `SENHA`                   | VARCHAR2(50) | -     | Senha em texto puro (sem hash). Obrigatória. Max 50 chars |
| `ID_PERFIL`               | NUMBER       | FK    | FK para `ACESSO_CADASTRO_PERFIL.ID_PERFIL`. Obrigatório |
| `ATIVO`                   | CHAR(1)      | -     | `'S'` = ativo / `'N'` = inativo. Obrigatório |
| `DATA_HORA_ULTIMO_ACESSO` | DATE         | -     | Atualizado pelo Oracle `sysdate` no login. ReadOnly na UI |
| `QUANTIDADE_ACESSO`       | NUMBER       | -     | Contador. Incrementado no login. Default `0`. ReadOnly na UI |
| `ATUALIZA_SENHA`          | CHAR(1)      | -     | `'S'` = força troca de senha no próximo login / `'N'` = normal |

#### Tabela de Vínculo Usuário-Empresa-Estabelecimento: `ACESSO_USUARIO_EMPRESA_ESTAB`

| Coluna               | Tipo   | Papel | Observação |
|----------------------|--------|-------|------------|
| `ID_USUARIO`         | NUMBER | FK    | FK para `ACESSO_CADASTRO_USUARIO.ID_USUARIO` |
| `CD_EMPRESA`         | NUMBER | FK    | Código da empresa (ref. via view `VW_ESTABELECIMENTO_NEW`) |
| `CD_ESTABELECIMENTO` | NUMBER | FK    | Código do estabelecimento (ref. via view `VW_ESTABELECIMENTO_NEW`) |

- Registros são inseridos/excluídos individualmente (sem UPDATE).
- Verificação de duplicidade via SELECT antes do INSERT.

#### View Oracle: `VW_ESTABELECIMENTO_NEW`

| Coluna           | Tipo     | Observação |
|------------------|----------|------------|
| `EMPRESA`        | NUMBER   | Código da empresa (CdEmpresa) |
| `ESTABELECIMENTO`| NUMBER   | Código do estabelecimento (CdEstabelecimento) |
| `DESCRITIVO`     | VARCHAR2 | Texto descritivo. Formato esperado: `"{Nome Empresa} - {Nome Estab}"` |

> A view é consultada sem filtro por usuário. Todos os estabelecimentos são retornados e o vínculo atual é verificado campo a campo em `ACESSO_USUARIO_EMPRESA_ESTAB`.

#### Tabela de Log de Visualização: `ACESSO_VISUALIZACAO_PAGINA`

| Coluna                   | Tipo   | Observação |
|--------------------------|--------|------------|
| `ID_VISUALIZACAO_PAGINA` | NUMBER | PK |
| `ID_USUARIO`             | NUMBER | FK para `ACESSO_CADASTRO_USUARIO.ID_USUARIO` |
| `ID_PAGINA`              | NUMBER | FK para `ACESSO_CADASTRO_PAGINA.ID_PAGINA` |
| `DATA_HORA`              | DATE   | Data/hora do primeiro acesso à página |
| `QUANTIDADE`             | NUMBER | Quantidade de acessos a essa página por esse usuário |

#### Diagrama de Relacionamentos Completo do Módulo

```
ACESSO_CADASTRO_PERFIL
    | ID_PERFIL (PK)
    |
    +--- (FK) --- ACESSO_CADASTRO_USUARIO.ID_PERFIL  [1 perfil para N usuarios]
    |
    +--- (FK) --- ACESSO_PERFIL_PAGINA.ID_PERFIL     [1 perfil para N paginas]
                        |
                        +--- (FK) --- ACESSO_CADASTRO_PAGINA.ID_PAGINA
                                            |
                                            +-- ID_PAGINA_PAI -> ID_PAGINA (auto-relac. hierarquico)
                                            +-- (FK) -- ACESSO_VISUALIZACAO_PAGINA.ID_PAGINA

ACESSO_CADASTRO_USUARIO
    | ID_USUARIO (PK)
    |
    +--- (FK) --- ACESSO_USUARIO_EMPRESA_ESTAB.ID_USUARIO
    |                   |
    |                   +-- CD_EMPRESA         -> VW_ESTABELECIMENTO_NEW.EMPRESA
    |                   +-- CD_ESTABELECIMENTO -> VW_ESTABELECIMENTO_NEW.ESTABELECIMENTO
    |
    +--- (FK) --- ACESSO_VISUALIZACAO_PAGINA.ID_USUARIO
```

---

## 3. Funcionalidade Complementar: Alterar Senha

- **Arquivo:** `Treis.Web/interna/acesso/AlteraSenha.aspx` + `.aspx.cs`
- **Acionamento:** Automático no login quando `ATUALIZA_SENHA = 'S'`.
- **URL:** `~/interna/acesso/AlteraSenha.aspx?idUsuario={id}`

### Fluxo
1. Recebe `idUsuario` via QueryString.
2. Busca a senha atual: `SELECT SENHA FROM ACESSO_CADASTRO_USUARIO WHERE ID_USUARIO = {id}` e exibe em campo `txtKey`.
3. O usuário informa a nova senha. Um campo oculto `txtStatus` deve conter `"ok"` para a operação ser aceita (validação via JS).
4. Se `txtStatus == "ok"`:
   - Executa: `UPDATE ACESSO_CADASTRO_USUARIO SET SENHA='{novaSenha}', ATUALIZA_SENHA='N' WHERE ID_USUARIO = {id}`
   - Executa `SalvaAcessoUsuario` para registrar acesso.
   - Seta `Session["UserLogado"] = false`.
   - Redireciona para `~/Default.aspx` (forçando novo login).

---

## 4. Métodos do DaoAcesso Utilizados pelos Painéis

| Método | Utilizado por | Descrição |
|---|---|---|
| `GetUsuario(login, senha)` | Login | Autentica usuário por login+senha+ativo='S' |
| `GetPaginaByChave(chave, idPerfil)` | Todas as páginas internas | Valida permissão de acesso à página |
| `GetPaginaByChave(chave)` | Perfil (salvar permissões) | Busca página pelo CHAVE_CONTROLE sem filtro de perfil |
| `GetPaginas()` | Perfil (popup permissões) | Lista todas as páginas ativas do sistema |
| `GetPaginas(idPerfil)` | Perfil (popup permissões) | Lista páginas vinculadas a um perfil específico |
| `SalvarPerfilPagina(idPagina, idPerfil)` | Perfil (salvar permissões) | INSERT em `acesso_perfil_pagina` |
| `DeletaVinculoPaginaPerfil(idPerfil, idPagina)` | Perfil (salvar permissões) | DELETE em `acesso_perfil_pagina` por pagina especifica |
| `DeletaVinculoPaginaPerfil(idPerfil)` | (metodo existente, comentado no código) | DELETE total de um perfil em `acesso_perfil_pagina` |
| `GetNode(idUsuario)` | Usuário (popup empresas) | Lista todos os estabelecimentos de `VW_ESTABELECIMENTO_NEW` |
| `GetRegistro(cdEstab, cdEmpresa, idUsuario)` | Usuário (popup empresas) | Verifica existência de vínculo em `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `RegistraEstabelecimento(cdEstab, cdEmpresa, idUsuario)` | Usuário (popup empresas) | INSERT em `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `ExcluiEstabelecimento(cdEstab, cdEmpresa, idUsuario)` | Usuário (popup empresas) | DELETE em `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `SalvaAcessoUsuario(idUsuario, dataAcesso)` | Login / AlteraSenha | UPDATE: incrementa `QUANTIDADE_ACESSO` e atualiza `DATA_HORA_ULTIMO_ACESSO = sysdate` |
| `AlterarSenha(idUsuario, novaSenha)` | AlteraSenha | UPDATE: nova senha e `ATUALIZA_SENHA = 'N'` |
| `GetSenha(idUsuario)` | AlteraSenha | SELECT da senha atual |

---

## 5. Configuração da Conexão

- **Provider:** `System.Data.OracleClient` (legado .NET Framework)
- **Connection String name:** `"Oracle"` (definida em `Web.config`, secao `<connectionStrings>`)
- O `ProviderName` no `SqlDataSource` referencia `ConnectionStrings:Oracle.ProviderName`.

---

**Nota de Engenharia:** Documento gerado por engenharia reversa completa do código-fonte legado em 2026-07-30. Todas as regras, nomenclaturas de banco, SQLs e comportamentos refletem fielmente a base de código analisada.

Pontos criticos de segurança identificados para correção na nova implementação:
1. Senha armazenada em **texto puro** (sem hash/criptografia).
2. SQLs construídos por concatenação de string em alguns métodos do DaoAcesso (vulnerável a SQL Injection).
3. A nova implementação DEVE implementar hash de senha (ex: bcrypt) e usar ORM ou queries parametrizadas.
