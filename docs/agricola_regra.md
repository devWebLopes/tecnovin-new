# Especificação Técnica: Módulo Agrícola (Safra/Meta e Compras Frutas)

> **Fonte de verdade** gerada por engenharia reversa da base legada (`.NET Framework 4.8`, ASP.NET Web Forms, DevExpress v16.2.8, `System.Data.OracleClient`).
> **Stack legada dos artefatos**: `Treis.Web` (UI), `Treis.Data` (DAO/Oracle), `Treis.Util` (helpers).
> **Conexão Oracle**: connection string `Oracle` (provider `System.Data.OracleClient`, TNS alias `tecnovin`) — ver `Treis.Web/Web.config`.

---

## 1. Painel: Cad. Safra/Meta

### 1.1. Caminho e Inicialização

- **Navegação:** Agrícola > Cad. Safra/Meta
  - O menu é **data-driven** (montado a partir da tabela `acesso_cadastro_pagina`); a página é identificada pela chave de controle **`cadastroSafraMeta`** (constante `nomeTelaAtual` no code-behind).
  - A página é hospedada em **abas dinâmicas** de `Principal.aspx` (mesmo padrão documentado em `acesso_bi_regra.md`); ao final do markup há script inline `parent.MostraCarregando(false, nomeTela)`.
- **Artefatos Frontend:**
  - `Treis.Web/interna/agricola/CadastroSafraMeta.aspx` — markup da página (título de tela: **"Cadastro de Safra / Meta"**).
  - `Treis.Web/interna/agricola/CadastroSafraMeta.aspx.designer.cs` — designer gerado.
  - Master page: `~/interna/Interna.Master` (ContentPlaceHolders `cphHead` e `cphConteudo`).
  - Grid: `dx:ASPxGridView` **gvdDados** (tema `Office2010Blue`, `ClientInstanceName="gvdDados"`, `KeyFieldName="ID_META_COMPRA"`).
  - Botão: `dx:ASPxButton` **btnNovo** ("Nova Safra/Meta") — `AutoPostBack="False"`; click client-side executa `gvdDados.AddNewRow()`.
  - JS de suporte: `Treis.Web/content/js/rotina.js` (`FechaBloco` para colapsar o bloco, `MostraCarregando` no parent).
  - Hidden field `nomeTela` (valor `cadastroSafraMeta`), usado pelo script de carregamento.
- **Artefatos Backend:**
  - `Treis.Web/interna/agricola/CadastroSafraMeta.aspx.cs` — code-behind (2 membros apenas: `Page_Load` e `gvdDados_HtmlDataCellPrepared`).
  - `Treis.Data/DaoAcesso.cs` → `GetPaginaByChave(chaveControle, idPerfil)` — gate de permissão.
  - `Treis.Data/acesso/Usuario.cs` — classe `Usuario` em `Session["User"]` (propriedades: `IDUsuario`, `Nome`, `Login`, `IDPerfil`, `QuantidadeAcesso`, `Atualiza_Senha`).
  - **Não há DAO/Service/Procedure para o CRUD**: a persistência é 100% via `asp:SqlDataSource` declarativo no markup (SQL ad-hoc direto na tabela).
- **Inicialização (Page_Load, executado em todo postback):**
  1. `nomeTela.Value = "cadastroSafraMeta"`.
  2. `Session["idUsuario"] = ((Usuario)Session["User"]).IDUsuario` — usado pelo `sqlEmpresa` (SessionParameter `idUsuario`).
  3. Gate de permissão: `new DaoAcesso().GetPaginaByChave("cadastroSafraMeta", ((Usuario)Session["User"]).IDPerfil)`; se retornar `null` **ou lançar exceção** (ex.: `Session["User"]` nulo), redireciona para `~/interna/SemPermissao.aspx?pag=cadastroSafraMeta` (com `Response.Redirect(url, false)`).

### 1.2. Regras de Negócio e Validações

**Listagem (Select):**
- Fonte: `sqlMetaCompras` — `SELECT "ID_META_COMPRA", "CD_LINHA", "SAFRA", "DT_INICIAL", "DT_FINAL", "META_QTDE", "CD_EMPRESA" FROM "META_COMPRAS" ORDER BY SAFRA DESC`.
- Grid com paginação de **200 registros** (`SettingsPager PageSize="200"`), edição **inline** (`SettingsEditing Mode="Inline"`).

**Colunas da grid (ordem/visual):**

| Coluna (FieldName) | Caption | Editor | Validação (ValidationGroup=`validGroup`) | Observações |
|---|---|---|---|---|
| `ID_META_COMPRA` | — | Texto (ReadOnly, **invisível**) | — | Chave da grid (`KeyFieldName`); **nunca editada na UI** |
| `CD_EMPRESA` | EMPRESA | **ComboBox com itens hardcoded** | Obrigatório: "Informe a Empresa " | Itens fixos (ver tabela abaixo) |
| `CD_LINHA` | LINHA | Texto | Obrigatório: "Informe uma Linha" | Tipo `Decimal` no SqlDataSource |
| `SAFRA` | SAFRA | Texto | Obrigatório: "Informe uma Safra" | Tipo `String` |
| `DT_INICIAL` | DATA INICIAL | Data | Obrigatório: "Informe a Data de Inicio da Safra" | Tipo `DateTime` |
| `DT_FINAL` | DATA FINAL | Data | Obrigatório: "Informe a Data final da Safra" | Tipo `DateTime` |
| `META_QTDE` | META | Texto, `DisplayFormatString="{0:N0}"` | Obrigatório: "Informe a Meta a Ser Cadastrada" | Tipo `Decimal` |
| (CommandColumn) | — | Botões imagem **Editar** / **Deletar** | — | `ConfirmDelete="Deseja Excluir a Meta?"` |

**Domínio hardcoded de EMPRESA (combo `CD_EMPRESA`, ValueType `System.Decimal`):**

| Value (`CD_EMPRESA`) | Text |
|---|---|
| `2` | TECNOVIN DO BRASIL LTDA |
| `200` | SUVALAN SUCOS DE FRUTAS, INDUSTRIA E COM |
| `300` | SUMABRAS DO BRASIL LTDA |
| `700` | MAISONFORESTIER |

**Persistência (SqlDataSource `sqlMetaCompras` — SQL direto, sem procedure):**
- **Insert:** `INSERT INTO "META_COMPRAS" ("ID_META_COMPRA", "CD_LINHA", "SAFRA", "DT_INICIAL", "DT_FINAL", "META_QTDE", "CD_EMPRESA") VALUES (:ID_META_COMPRA, :CD_LINHA, :SAFRA, :DT_INICIAL, :DT_FINAL, :META_QTDE, :CD_EMPRESA)`
- **Update:** `UPDATE "META_COMPRAS" SET "CD_LINHA" = :CD_LINHA, "SAFRA" = :SAFRA, "DT_INICIAL" = :DT_INICIAL, "DT_FINAL" = :DT_FINAL, "META_QTDE" = :META_QTDE, "CD_EMPRESA" = :CD_EMPRESA WHERE "ID_META_COMPRA" = :ID_META_COMPRA`
- **Delete:** `DELETE FROM "META_COMPRAS" WHERE "ID_META_COMPRA" = :ID_META_COMPRA` (físico, sem inativação lógica; com confirmação client-side).

**Regras identificadas:**
- **Regra 1 — Obrigatoriedade:** todos os campos editáveis são obrigatórios (validação client-side DevExpress, grupo `validGroup`). **Não há validação server-side** nem no banco via aplicação.
- **Regra 2 — Ausência de validação de conflito de safra:** o legado **não valida** sobreposição de datas, duplicidade de safra/linha/empresa, nem coerência `DT_INICIAL <= DT_FINAL`. Qualquer consistência desse tipo, se existir, é apenas por constraints no Oracle (não visíveis no código).
- **Regra 3 — Geração do ID:** `ID_META_COMPRA` é parâmetro `Decimal` no Insert, mas a coluna é oculta/read-only na grid (a UI **não coleta** valor). Presume-se geração no banco (trigger/sequence) — **não confirmado no código; validar com DBA antes da reconstrução**. Se não houver trigger, o insert legado envia valor nulo.
- **Regra 4 — Destaque visual:** no evento `gvdDados_HtmlDataCellPrepared`, toda célula de linha cuja coluna `SAFRA` seja igual ao **ano corrente do servidor** (`DateTime.Now.Year`) é renderizada em **negrito**.
- **Regra 5 — Empresas disponíveis não respeitam o usuário:** embora exista o `sqlEmpresa` (filtra `VW_EMPRESA_NEW` × `ACESSO_USUARIO_EMPRESA_ESTAB` por `Session["idUsuario"]`), ele **não é referenciado** por nenhum controle — a combo de empresa usa os 4 itens fixos. O mesmo vale para `sqlLinhas` (`SELECT ... FROM VW_EMPRESA`): **configuração morta**.
- **Regra 6 — Permissão por perfil:** acesso à página condicionado à existência de registro em `ACESSO_PERFIL_PAGINA` para o `ID_PERFIL` do usuário logado + página ativa na chave `cadastroSafraMeta`.

**Defeitos latentes do legado (não replicar cegamente):**
1. `sqlEmpresa` e `sqlLinhas` declarados e nunca utilizados.
2. Inserção depende de geração implícita de PK no banco (acoplamento invisível).
3. Nenhuma validação de datas/sobreposição — possível cadastrar safra com `DT_FINAL < DT_INICIAL`.
4. Exclusão física imediata, sem auditoria.

### 1.3. Modelo de Dados

- **Tabelas Principais:**
  - **`META_COMPRAS`** — única tabela transacional do painel (CRUD direto).
    | Coluna | Tipo (inferido pelo SqlDataSource) | Papel |
    |---|---|---|
    | `ID_META_COMPRA` | NUMBER/Decimal | **PK presumida** (chave da grid e filtro de Update/Delete; geração presumida em banco) |
    | `CD_LINHA` | NUMBER/Decimal | Código da linha de produto/fruta |
    | `SAFRA` | VARCHAR2/String | Identificação da safra (ex.: "2026") |
    | `DT_INICIAL` | DATE | Início da safra |
    | `DT_FINAL` | DATE | Término da safra |
    | `META_QTDE` | NUMBER/Decimal | Quantidade meta (formato inteiro `N0`) |
    | `CD_EMPRESA` | NUMBER/Decimal | Empresa (domínio fixo: 2, 200, 300, 700) |
- **Tabelas/Views de apoio (somente leitura, configuração morta no painel):**
  - `VW_EMPRESA_NEW` (colunas usadas: `CD_EMPRESA`, `DS_RAZAO_SOCIAL`, `NM_FANTASIA`).
  - `VW_EMPRESA` (colunas usadas: `CD_EMPRESA`, `DS_RAZAO_SOCIAL`, `NM_FANTASIA`).
  - `ACESSO_USUARIO_EMPRESA_ESTAB` (`ID_USUARIO`, `CD_EMPRESA`, `CD_ESTABELECIMENTO`).
- **Tabelas de segurança (gate de acesso, compartilhadas por todo o sistema):**
  - `acesso_cadastro_pagina` (`ID_PAGINA` PK, `URL`, `TITULO_ABA`, `CHAVE_CONTROLE`, `TITULO_MENU`, `ID_PAGINA_PAI`, `ATIVO`, `ORDEM`, `TOOLTIP`).
  - `ACESSO_PERFIL_PAGINA` (`ID_PERFIL`, `ID_PAGINA`) — vínculo perfil × página.
  - `ACESSO_CADASTRO_USUARIO` (`ID_USUARIO` PK, `NOME`, `LOGIN`, `SENHA`, `ID_PERFIL`, `QUANTIDADE_ACESSO`, `ATUALIZA_SENHA`, `ATIVO`).
- **Relacionamentos e Chaves:**
  - `META_COMPRAS.CD_EMPRESA` → empresas do grupo (sem FK declarada visível no código; referência conceitual a `VW_EMPRESA_NEW.CD_EMPRESA`).
  - `META_COMPRAS.CD_LINHA` → linha de produto (tabela de domínio **não referenciada** no painel; o código da linha é digitado livremente).
  - FKs reais, sequences e triggers de `META_COMPRAS` **não constam na base de código** — levantar via dicionário Oracle (`USER_CONSTRAINTS`, `USER_TRIGGERS`, `USER_SEQUENCES`) na migração.

---

## 2. Painel: Compras Frutas

### 2.1. Caminho e Inicialização

- **Navegação:** Agrícola > Compras Frutas
  - Chave de controle: **`comprasFrutas`** (constante `nomeTelaAtual`). Título de tela: **"Relatório de Compra de Frutas"**.
  - Mesma hospedagem em abas de `Principal.aspx` e mesmo gate de permissão do painel anterior (`GetPaginaByChave("comprasFrutas", IDPerfil)` → redirect `SemPermissao.aspx?pag=comprasFrutas`).
- **Artefatos Frontend:**
  - `Treis.Web/interna/agricola/ComprasFrutas.aspx` — página principal.
  - `Treis.Web/interna/agricola/ucComprasFrutasDetalhamento.ascx` — user control do **2º nível de drill-down** (detalhamento por variedade/grau).
  - `Treis.Web/interna/agricola/ucDetalhamentoNotaFiscal.ascx` — user control do **3º nível** (notas fiscais).
  - Popups: `popDetalhamento` (não modal) e `popDetalhamentoGrauUva` (**modal**), ambos `dx:ASPxPopupControl` com `dx:ASPxCallbackPanel` (`callDetalhamento` / `callDetalhamentoGrauUva`); os UCs são carregados dinamicamente via `LoadControl` nos `PlaceHolder`s `pnlDetalheCompras` e `detalheGrauUva`.
  - Componentes de apoio: `dx:ASPxTimer` (`tmrTempoJob`, auto-refresh), `dx:ASPxCallback` (`callAtualizarTempo`), `dx:ASPxGridViewExporter` (`gvdDadosExport`), `dx:ASPxComboBox` (`cboTempo`), `dx:ASPxDateEdit` (`datePesquisa`), `dx:ASPxLabel` (`lblUltimaAtualizacao`), botões `btnRecarregar` e `btnExcel_Origem`.
  - JS inline na página (funções `AtualizaPesquisa`, `RecarregarGrid`, `MostraDetalhamento`, `MostraDetalhamentoGrau`, `ConcluiCallback`, `ConcluiCallbackDetalhamentoGrau`, `AjustaGrid`, `AjustaGridDetalheGrau`, `FechaPopUp`) + `rotina.js` (`AjustaCoresGrids`: células `rel="clikNeutro"` ficam com cursor pointer + sublinhado).
- **Artefatos Backend:**
  - `Treis.Web/interna/agricola/ComprasFrutas.aspx.cs` — orquestração da tela, montagem dinâmica da grid e drill-downs.
  - `Treis.Web/interna/agricola/ucComprasFrutasDetalhamento.ascx.cs` e `ucDetalhamentoNotaFiscal.ascx.cs`.
  - `Treis.Data/DaoPainel.cs` — métodos `GetComprasFrutas`, `GetDetalhesComprasFrutas`, `GetDetalhesNotaFiscalComprasFrutas`.
  - `Treis.Data/DaoBase.cs` — `GetDadosProcedure(package, nomeProcedure, List<Parametros>)`: resolve o nome físico da package via `ConfigurationManager.AppSettings[package]` e concatena com o nome da procedure; executa via `OracleCommand` (`CommandType.StoredProcedure`) e carrega `DataSet` com uma tabela por REF CURSOR de saída.
  - `Treis.Data/Parametros.cs` — DTO de parâmetro (`Nome`, `Valor` string, `Tipo` `OracleType`, `Output` bool). **Valor vazio/nulo → `DBNull.Value`**.
  - `Treis.Util/Dados.cs` — formatos: `formatoInteiro = "{0:0,0.}"`, `formatoDecimal = "{0:0,0.00}"`.
  - `Treis.Util/StringUtils.cs` — `TrocaCaracteres` (sanitiza nome de coluna trocando `. %$+()-<>/'` e espaço por `_`) e `ajustaColunaGrid` (largura = maior entre título e `MAX(coluna)` × 9–10px).
- **Mapeamento Oracle (verificado em `Web.config`):** appSetting `packageCompras` = **`pkg_bi_compras.`** → as procedures reais são:
  1. `pkg_bi_compras.sp_recebimento_frutas` — painel principal.
  2. `pkg_bi_compras.sp_recebimento_frutas_det` — detalhamento (2º nível).
  3. `pkg_bi_compras.sp_recebimento_frutas_det_nf` — notas fiscais (3º nível).
- **Inicialização (Page_Load):** gate de permissão; em `!IsPostBack` executa `CarregaCombo()` (define `datePesquisa.Text = hoje, dd/MM/yyyy`) e `CarregaDados()`; a **cada load** atualiza `lblUltimaAtualizacao.Text = DateTime.Now("dd/MM/yyyy HH:mm:ss")`.

### 2.2. Regras de Negócio e Validações

**Nível 0 — Painel principal (`gvdDados`):**
- **Regra 1 — Filtro único:** o único filtro é `datePesquisa` (data, padrão = dia corrente). Mudar a data dispara callback client-side (`gvdDados.PerformCallback()` → `gvdDados_CustomCallback` → `CarregaDados()`). Botão refresh faz postback (`btnRecarregar_Click` → `CarregaDados()`).
- **Regra 2 — Consulta:** `DaoPainel.GetComprasFrutas(data)` → `pkg_bi_compras.sp_recebimento_frutas` com parâmetros:
  | Parâmetro | Direção | Tipo Oracle | Origem |
  |---|---|---|---|
  | `p_data` | IN | `DateTime` | `datePesquisa.Text` (string "dd/MM/yyyy" enviada ao parâmetro DateTime — conversão implícita do driver) |
  | `r_resultado` | OUT | `Cursor` (REF CURSOR) | — |
- **Regra 3 — Grid 100% dinâmica:** colunas são geradas em runtime a partir do REF CURSOR (`FormataGrid`), sem tipagem. Caption = nome da coluna com `_`→espaço e `CD `→`CODIGO `. Colunas esperadas (inferidas do code-behind — **a lista definitiva é definida pela package Oracle**): `EMPRESA`, `LINHA`, `UF`, `DESCRICAO`, `VARIEDADE`, `EM_SAFRA`, `DIA_ANTERIOR`, `QTDE_D0`…`QTDE_D4`, `ACUMULADO`, mais colunas contendo `%`.
- **Regra 4 — Renomeação de colunas-dia:** `QTDE_D0` → caption = data pesquisada; `QTDE_D1` → D-1; `QTDE_D2` → D-2; `QTDE_D3` → D-3; `QTDE_D4` → D-4 (formato `dd/MM/yyyy`). Coluna `EM_SAFRA` é **ocultada**; coluna `VARIEDADE` tem largura fixa 220; colunas com `%` têm largura 80.
- **Regra 5 — Formatação numérica (`gvdDados_HtmlDataCellPrepared`):** colunas `QTDE_D*` e `ACUMULADO` → inteiro (`formatoInteiro`); colunas com `%` no nome → `decimal.ToString("N1")` (1 casa); demais → inteiro; em caso de exceção de conversão, exibe o valor bruto.
- **Regra 6 — Semáforo de linhas (por conteúdo de `DESCRICAO`, case-insensitive):** contém `LINHA` → fundo `#EEE9E9`; contém `EMPRESA` → fundo `#D3D3D3`; contém `GERAL` → fundo `#77889A`, fonte branca e **negrito** (linhas de totalização: "TOTAL EMPRESA" / "TOTAL GERAL").
- **Regra 7 — Drill-down (células clicáveis, `rel="clikNeutro"`):**
  | Coluna clicada | Linha | Ação (`quemChamou`) | `p_coluna_clicada` enviado |
  |---|---|---|---|
  | `DIA_ANTERIOR` | não-total | `DC` (detalhe compras) | `"ANTERIOR"` |
  | `DIA_ANTERIOR` | TOTAL EMPRESA / TOTAL GERAL | `NF` (notas fiscais) | `"ANTERIOR"` |
  | `QTDE_D0` | não-total | `DC` | `"QTDE_D0"` |
  | `QTDE_D0` | TOTAL EMPRESA / TOTAL GERAL | `NF` | `"QTDE_D0"` |
  | `ACUMULADO` | não-total | `DC` | `"ACUMULADO"` |
  - Parâmetros do popup (string pipe-delimitada): `data | EMPRESA | LINHA | UF | coluna | quemChamou`. Células vazias não são clicáveis. Colunas `QTDE_D1`–`QTDE_D4` **não** são clicáveis.
- **Regra 8 — Roteamento do popup (`callDetalhamento_Callback`):** `quemChamou == "NF"` → carrega `ucDetalhamentoNotaFiscal.ascx`; caso contrário → `ucComprasFrutasDetalhamento.ascx`.
- **Regra 9 — Sessão:** o `DataTable` principal é cacheado em `Session["comprasFrutas"]` e reutilizado em paginação/ordenação/exportação. Ordenação da grid está **desabilitada** (`AllowSort="False"`); paginação de 100.
- **Regra 10 — Auto-refresh:** `cboTempo` com itens **30 Minutos (1800000 ms)** e **01 Hora (3600000 ms)**; habilita `tmrTempoJob` cujo `Tick` executa `RecarregarGrid()` (refresh da grid + callback que atualiza `lblUltimaAtualizacao`).
- **Regra 11 — Exportação Excel:** `btnExcel_Origem` (postback `btnExcel_Click`) rebinda a sessão e exporta via `ASPxGridViewExporter.WriteXlsToResponse("comprasFrutas", true)` → arquivo **`comprasFrutas.xls`**.

**Nível 1 — Detalhamento (`ucComprasFrutasDetalhamento`, ação `DC`):**
- **Regra 12 — Consulta:** `DaoPainel.GetDetalhesComprasFrutas(data, empresa, linha, uf, colunaClicada)` → `pkg_bi_compras.sp_recebimento_frutas_det`:
  | Parâmetro | Tipo | Origem |
  |---|---|---|
  | `p_data_emissao` | DateTime | data pesquisa |
  | `p_empresa` | VarChar | valor `EMPRESA` da linha clicada |
  | `p_linha` | VarChar | valor `LINHA` |
  | `p_uf` | VarChar | valor `UF` |
  | `p_coluna_clicada` | VarChar | `ANTERIOR` / `QTDE_D0` / `ACUMULADO` |
  | `r_resultado` | OUT Cursor | — |
- **Regra 13 — Grid dinâmica:** nomes de coluna sanitizados por `StringUtils.TrocaCaracteres`; larguras por `ajustaColunaGrid` (exceto `VARIEDADE`=500; demais `MinWidth`=320). Paginação: **todos os registros** (`ShowAllRecords`) com scroll vertical.
- **Regra 14 — Formatação:** colunas contendo `MEDIO` ou `FATURADO` → `formatoDecimal` (2 casas); contendo `UNITARIO` → `ToString("N3")` (3 casas); demais → inteiro. Linhas cuja `VARIEDADE` contém "GERAL" → fundo `#77889A`, fonte branca, negrito.
- **Regra 15 — Drill-down nível 2:** toda célula cujo nome de coluna **não** contenha `MEDIO` e **não** contenha `VARIEDADE` chama `MostraDetalhamentoGrau(data, empresa, linha, uf, colunaClicada, colunaGrau=caption da coluna clicada, variedade, título)` → abre popup **modal** `popDetalhamentoGrauUva`.
- **Regra 16 — Exportações:** `btnExcelDetalhe_Click1` e `btnExcel_Click` exportam **`Compras Frutas.xls`**; `callExcel_Callback` exporta **`DetalhamentoNotaFiscal.xls`**.

**Nível 2 — Notas Fiscais (`ucDetalhamentoNotaFiscal`, ações `NF` e drill de grau):**
- **Regra 17 — Consulta:** `DaoPainel.GetDetalhesNotaFiscalComprasFrutas(...)` → `pkg_bi_compras.sp_recebimento_frutas_det_nf`:
  | Parâmetro | Tipo | Origem |
  |---|---|---|
  | `p_data_emissao` | DateTime | data pesquisa |
  | `p_empresa` | VarChar | empresa |
  | `p_linha` | VarChar | linha |
  | `p_uf` | VarChar | UF |
  | `p_coluna_clicada` | VarChar | coluna do nível 0 |
  | `p_coluna_grau_clicada` | VarChar | caption da coluna de grau clicada no nível 1 (nulo no fluxo `NF` direto) |
  | `p_cd_variedade` | VarChar | **2 primeiros caracteres** de `VARIEDADE`; se começar com `"TO"` → **nulo** (nulo no fluxo `NF` direto) |
  | `r_resultado` | OUT Cursor | — |
- **Regra 18 — Formatação:** `MEDIO`/`FATURADO` → decimal 2 casas; `UNITARIO` → 3 casas (`N3`); colunas contendo `DT` → data `dd/MM/yyyy`; demais (exceto `NR_NOTAFISCAL`, que fica em texto livre) → inteiro. Grid `ShowAllRecords`, exporta `DetalhamentoNotaFiscal.xls`.
- **Regra 19 — Cache compartilhado:** ambos os UCs gravam o resultado na **mesma** chave `Session["DetalhamentoNotaFiscal"]` (um sobrescreve o outro).

**Defeitos latentes do legado (não replicar cegamente):**
1. `callDetalhamentoGrauUva_Callback`: quando `colunaGrau == "QTDE TOTAL"` define `colunaGrauClicada = null` e **logo em seguida sobrescreve** com o valor literal — o tratamento é inócuo (código morto).
2. `ComprasFrutas.FormataGrid` e `ucComprasFrutasDetalhamento.CarregaDados` passam `Session[chave].ToString()` (resulta `"System.Data.DataTable"`) como nome de sessão — grava chave-lixo `Session["System.Data.DataTable"]`.
3. `ucComprasFrutasDetalhamento.FormataGrid`: caption de `DIA_ANTERIOR` para segunda-feira é calculada (D-3) e **imediatamente sobrescrita** por D-1.
4. JS `cboTempo.TextChanged`: ramo `else` referencia `tmrJobTempo` (nome inexistente; o correto seria `tmrTempoJob`).
5. `ucDetalhamentoNotaFiscal.callExcel_Callback` lê `Session["ComprasFrutasDetalhamento"]` — chave **diferente** da usada no carregamento (`DetalhamentoNotaFiscal`); exportação por callback vem vazia.
6. Código morto: `CreateTemplate()`/`linkHeaderTemplate` (template de header com link) nunca são invocados; `AbreDetalhamentoColuna` só exibe `alert`; `btnExcel_Click` vazio no UC de NF; auditoria de exportação (`DaoCompras.InsertRegistroRelatorio`) comentada.
7. `DaoBase.GetDadosProcedure` (overload com `ref OracleConnection`) **engole exceções** retornando `null` (o overload usado por este painel relança a exceção — comportamento inconsistente entre DAOs).
8. Parâmetros `DateTime` recebem strings "dd/MM/yyyy" (conversão implícita dependente de cultura/NLS); `lblUltimaAtualizacao` tem valor hardcoded "07/06/2012 15:03:00" no markup (sempre sobrescrito no load).

### 2.3. Modelo de Dados

- **Tabelas Principais:**
  - **Nenhuma tabela é acessada diretamente pela aplicação.** Todo o dado transacional vem dos REF CURSORs da package **`pkg_bi_compras`** (`sp_recebimento_frutas`, `sp_recebimento_frutas_det`, `sp_recebimento_frutas_det_nf`). As tabelas de origem (notas fiscais de entrada/recebimento de frutas, itens, variedades, empresas/estabelecimentos — provavelmente no schema do ERP, cf. uso de `HDS.` em outros DAOs) são **encapsuladas na package e não constam na base de código**.
  - **Ação obrigatória na migração:** mapear internamente `pkg_bi_compras` com o DBA (corpo da package) para documentar tabelas/colunas reais; na nova stack, consumir as **mesmas** procedures via `Oracle.ManagedDataAccess.Core` + Dapper (Regra de Ouro 1 e 2 do CLAUDE.md) — o contrato de parâmetros documentado em 2.2 é o contrato estável.
- **Colunas de saída esperadas (inferidas do consumo no código — confirmar com o DBA):**
  - `sp_recebimento_frutas`: `EMPRESA`, `LINHA`, `UF`, `DESCRICAO`, `VARIEDADE`, `EM_SAFRA`, `DIA_ANTERIOR`, `QTDE_D0`, `QTDE_D1`, `QTDE_D2`, `QTDE_D3`, `QTDE_D4`, `ACUMULADO`, colunas `%...%`.
  - `sp_recebimento_frutas_det`: `VARIEDADE` + colunas de grau/quantidade + colunas `MEDIO`/`FATURADO`/`UNITARIO`.
  - `sp_recebimento_frutas_det_nf`: `NR_NOTAFISCAL`, colunas `DT*` (datas), quantidades, `UNITARIO`, `MEDIO`/`FATURADO`.
- **Relacionamentos e Chaves:**
  - Sem PKs/FKs visíveis na aplicação (consulta read-only analítica). Relacionamentos conceituais: Compra/NF → Empresa (`CD_EMPRESA`), → Linha de fruta (`CD_LINHA`), → Variedade (`CD_VARIEDADE` — chave de 2 caracteres extraída da descrição), → UF do produtor.
  - Tabelas de segurança envolvidas no gate: `acesso_cadastro_pagina`, `ACESSO_PERFIL_PAGINA`, `ACESSO_CADASTRO_USUARIO` (idem painel 1).
  - Observação: a tabela **`META_COMPRAS`** (painel 1) é a contrapartida de "meta" para os volumes realizados exibidos neste painel — a amarração meta × realizado, se houver, ocorre **dentro da package Oracle** (não há join no código da aplicação).

---

**Nota de Engenharia:** Documento gerado para reconstrução via IA. Todas as regras, cálculos de negócio e nomenclaturas de banco refletem a base de código legado analisada. Itens marcados como "presumido/inferido" (geração de `ID_META_COMPRA`, corpo da `pkg_bi_compras`, FKs de `META_COMPRAS`, lista definitiva de colunas dos REF CURSORs) não puderam ser confirmados estaticamente e devem ser validados no dicionário Oracle antes da reconstrução.
