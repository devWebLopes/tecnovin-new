# Especificação Técnica: Painel "Acesso B.I." (CadastroUsuarioBi)

> **Documento gerado por engenharia reversa completa do código-fonte legado.**
> Fonte única de verdade para reconstrução deste painel em nova stack tecnológica, com 100% de fidelidade estrutural, visual e de dados.
> Stack original: ASP.NET WebForms (.NET Framework 4.8, C#) + DevExpress Web v16.2.8 + Oracle DB (via `System.Data.OracleClient`).

---

## Sumário

1. [Objetivo do Painel](#1-objetivo-do-painel)
2. [Domínios de Negócio](#2-domínios-de-negócio)
3. [Artefatos Visuais e Componentes](#3-artefatos-visuais-e-componentes)
4. [Regras de Relacionamento e Negócio](#4-regras-de-relacionamento-e-negócio)
5. [Camada de Dados (Banco e Queries)](#5-camada-de-dados-banco-e-queries)
6. [Anexo A — Stack, Arquivos e Assets](#anexo-a--stack-arquivos-e-assets)
7. [Anexo B — Ciclo de Vida Completo (Hosting em Abas)](#anexo-b--ciclo-de-vida-completo-hosting-em-abas)
8. [Anexo C — Contrato de Estado de Sessão](#anexo-c--contrato-de-estado-de-sessão)
9. [Anexo D — Defeitos/Comportamentos Latentes do Legado](#anexo-d--defeitoscomportamentos-latentes-do-legado)
10. [Anexo E — Checklist de Fidelidade para Reimplementação](#anexo-e--checklist-de-fidelidade-para-reimplementação)

---

## 1. OBJETIVO DO PAINEL

### 1.1. Propósito de negócio

O painel **"Acesso B.I."** (título exibido na tela: **"Usuários com acesso ao B.I. por empresas"**) é uma tela administrativa que gerencia **quais usuários do sistema ERP podem acessar o módulo externo de B.I. (Business Intelligence) e para quais empresas do grupo esse acesso é válido**.

O painel materializa a relação **usuário × empresa** na tabela Oracle `USUARIO_EMPRESA`. Essa relação é o "gate" de acesso ao B.I.:

1. Na página principal do sistema (`Principal.aspx`), o ícone de atalho **"Acesso ao BI"** (`lnkBi`, imagem `content/img/bi4.png`) só é exibido se o usuário logado possuir **pelo menos uma linha** em `USUARIO_EMPRESA`:

   ```csharp
   // Principal.aspx.cs — Page_Load
   DataTable dtUsuario = new DaoAcesso().GetUsuario(userLogado.IDUsuario.ToString());
   if (string.IsNullOrEmpty(dtUsuario.Rows[0]["ID_USUARIO_EMPRESA"].ToString()))
   {
       lnkBi.Visible = false;   // usuário sem vínculo não vê o atalho do B.I.
   }
   ```

2. Ao clicar no atalho, o sistema redireciona para a aplicação externa de B.I. via `CarregaBi.aspx`, que monta a URL a partir do `appSetting` `caminhoBI` (ex.: `http://localhost:3221/Login`) acrescida de uma chave gerada (`a`) e do `ID_USUARIO` (`b`):

   ```csharp
   // CarregaBi.aspx.cs — Page_Load
   string chave = util.Mv5.GeraChave();
   Response.Redirect(GetCaminho("caminhoBI") + "?a=" + chave + "&b=" + ((Usuario)Session["User"]).IDUsuario, false);
   ```

3. A coluna `EMPRESA` de `USUARIO_EMPRESA` restringe o escopo de empresas que o usuário pode visualizar dentro do B.I. (consumido pela aplicação externa, não por este painel).

### 1.2. Usuário final

- **Administrador de TI / Gestor de Segurança da Informação** da empresa (perfil com permissão à página de chave `cadastroUsuarioBi`).
- Operações que o usuário executa no painel:
  - **Conceder acesso ao B.I. para um usuário em TODAS as empresas** de uma vez (botão "Novo" do grid mestre).
  - **Conceder/ajustar acesso empresa a empresa** (grid detalhe, expansível por usuário).
  - **Visualizar** a lista de usuários com acesso e o resumo das empresas permitidas (coluna agregada `EMPRESAS`).

### 1.3. Identificação do painel

| Atributo | Valor |
|---|---|
| Rota/URL relativa | `~/interna/acesso/CadastroUsuarioBi.aspx` |
| Classe (code-behind) | `Treis.Web.interna.acesso.CadastroUsuarioBi` |
| **Chave de controle (permissão)** | `cadastroUsuarioBi` |
| Master Page | `~/interna/Interna.Master` |
| Navegação | Módulo de Acesso → item de menu cujo `CHAVE_CONTROLE = 'cadastroUsuarioBi'` (título do menu/aba definidos na tabela `ACESSO_CADASTRO_PAGINA`) |

---

## 2. DOMÍNIOS DE NEGÓCIO

| Domínio | Conceitos principais | Artefatos envolvidos |
|---|---|---|
| **Segurança / Controle de Acesso** | Usuário, Perfil, Página, Permissão de página por perfil | `ACESSO_CADASTRO_USUARIO`, `ACESSO_CADASTRO_PERFIL`, `ACESSO_CADASTRO_PAGINA`, `ACESSO_PERFIL_PAGINA`, sessão `Session["User"]`/`Session["UserLogado"]` |
| **Inteligência de Negócio (B.I.)** | Acesso ao B.I. externo, escopo por empresa, chave de autenticação transitória | `USUARIO_EMPRESA`, `CarregaBi.aspx`, `appSetting caminhoBI`, atalho `lnkBi` |
| **Estrutura Corporativa** | Empresa (grupo multiempresa), Razão Social, Nome Fantasia | View Oracle `VW_EMPRESA_NEW` (`CD_EMPRESA`, `DS_RAZAO_SOCIAL`, `NM_FANTASIA`) |

**Conceito central:** o sistema ERP é multiempresa. A tabela `USUARIO_EMPRESA` é uma tabela associativa N:N entre usuários (`ACESSO_CADASTRO_USUARIO`) e empresas (`VW_EMPRESA_NEW`), representando a autorização "usuário U pode ver dados da empresa E no B.I.".

---

## 3. ARTEFATOS VISUAIS E COMPONENTES

> Todos os componentes de UI são **DevExpress Web v16.2.8** com tema **Office2010Blue**. O painel **não possui gráficos nem KPIs** — é um painel cadastral composto por **1 barra de título colapsável, 2 botões "Novo", 1 grid mestre e 1 grid detalhe (master-detail)**.

### 3.1. Inventário exaustivo

| # | Artefato | Tipo | Propósito |
|---|---|---|---|
| 1 | **Barra de título colapsável** (`.barra` / `.barraTitulo`) | Container HTML + imagem de seta | Exibe o título "Usuários com acesso ao B.I. por empresas" e permite (teoricamente) colapsar/expandir o conteúdo — ver defeito D1 no Anexo D |
| 2 | **Botão "Novo" (mestre)** — `btnNovo` | `dx:ASPxButton` | Inicia a inclusão de um novo acesso de usuário ao B.I. (todas as empresas) via `gvdDados.AddNewRow()` (client-side) |
| 3 | **Grid mestre** — `gvdDados` | `dx:ASPxGridView` | Lista usuários com acesso ao B.I.; exibe nome (combo) e empresas agregadas; permite inserir novo acesso; expande linha de detalhe |
| 4 | **Coluna `ID_USUARIO_EMPRESA`** (mestre) | `GridViewDataTextColumn` oculta | Campo técnico, `ReadOnly`, `Visible=false` |
| 5 | **Coluna "NOMES" (`ID_USUARIO`)** (mestre) | `GridViewDataComboBoxColumn` | Exibe o nome do usuário; em edição vira combo com todos os usuários de `ACESSO_CADASTRO_USUARIO` |
| 6 | **Coluna `EMPRESAS`** (mestre) | `GridViewDataTextColumn` | Somente leitura; string agregada via `LISTAGG` com os primeiros nomes (nome fantasia até o 1º espaço) das empresas permitidas, separados por `" - "` |
| 7 | **Coluna de comando** (mestre) | `GridViewCommandColumn` (imagem, 50px) | Exibe botão **Salvar** (ícone `salvar.png`) durante a edição inline; configura botões Deletar/Cancelar — ver comportamento real na seção 4.4 |
| 8 | **Coluna `EMPRESA` oculta** (mestre) | `GridViewDataComboBoxColumn` `Visible=false` | Artefato vestigial (combo de empresas nunca exibido no mestre) |
| 9 | **Linha de detalhe** (`DetailRow` template) | Template HTML (`table.fonteGeral`) | Container do grid detalhe por usuário |
| 10 | **Botão "Novo" (detalhe)** | `dx:ASPxButton` | Inicia inclusão de UMA empresa para o usuário expandido via `gvdEmpresas.AddNewRow()` (client-side) |
| 11 | **Grid detalhe** — `gvdEmpresas` | `dx:ASPxGridView` | Lista as empresas vinculadas ao usuário expandido; permite adicionar nova empresa (combo filtrado: apenas empresas ainda não vinculadas) |
| 12 | **Coluna "EMPRESAS" (`EMPRESA`)** (detalhe) | `GridViewDataComboBoxColumn` | Exibe `NM_FANTASIA` da empresa; em edição vira combo alimentado dinamicamente (ver seção 4.5) |
| 13 | **Coluna de comando** (detalhe) | `GridViewCommandColumn` (imagem, 50px) | Exibe botão **Salvar** durante edição inline |
| 14 | **Campo oculto** `nomeTela` | `asp:HiddenField` | Carrega o valor `"cadastroUsuarioBi"`; consumido pelo script de encerramento do loading |
| 15 | **Script de startup** | JavaScript inline | `parent.MostraCarregando(false, 'cadastroUsuarioBi')` — desliga o overlay "Carregando..." da aba hospedeira |

### 3.2. Estrutura visual hierárquica (layout)

```
+-------------------------------------------------------------+
| .barra                                                      |
| +---------------------------------------------------------+ |
| | .barraTitulo  (fundo azul #0B74A3 + fundo_barra.png)    | |
| |  [Usuários com acesso ao B.I. por empresas]   [seta ▼]  | |
| +---------------------------------------------------------+ |
| | .barraDados  (id="dadosFiltro")                         | |
| |  [ Botão NOVO (ícone novo.png + texto) ]                | |
| |  +---------------------------------------------------+  | |
| |  | GRID MESTRE gvdDados (tema Office2010Blue, 100%)  |  | |
| |  |---------------------------------------------------|  | |
| |  | NOMES (combo) | EMPRESAS (texto) | [cmd 50px]     |  | |
| |  |---------------------------------------------------|  | |
| |  | [+] Joao Silva  | TECNO - VINICOLA - ... | [img]  |  | |
| |  |     +-------------------------------------------+ |  | |
| |  |     | DETALHE (DetailRow)                        | |  | |
| |  |     |  [ Botão NOVO ]                            | |  | |
| |  |     |  +---------------------------------------+ | |  | |
| |  |     |  | GRID DETALHE gvdEmpresas              | | |  | |
| |  |     |  | EMPRESAS (combo)        | [cmd 50px]  | | |  | |
| |  |     |  | TECNOVIN DO BRASIL ...  | [img]       | | |  | |
| |  |     |  +---------------------------------------+ | |  | |
| |  |     +-------------------------------------------+ |  | |
| |  +---------------------------------------------------+  | |
| +---------------------------------------------------------+ |
+-------------------------------------------------------------+
```

### 3.3. Marcação declarativa integral (fonte de verdade do markup)

Trechos fiéis do arquivo `Treis.Web/interna/acesso/CadastroUsuarioBi.aspx`:

```aspx
<%@ Page Title="" Language="C#" MasterPageFile="~/interna/Interna.Master" AutoEventWireup="true"
    CodeBehind="CadastroUsuarioBi.aspx.cs" Inherits="Treis.Web.interna.acesso.CadastroUsuarioBi" %>
<%@ Register assembly="DevExpress.Web.v16.2, Version=16.2.8.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    namespace="DevExpress.Web" tagprefix="dx" %>
```

**Barra de título + botão Novo mestre:**

```aspx
<div class="barra">
  <div class="barraTitulo" onclick="FechaBloco('imgFiltro','dadosDados')">
      <span class="barraTexto">Usuários com acesso ao B.I. por empresas</span>
      <span class="barraSeta"><img src="../../content/img/seta_aberta.png" id="imgFiltro" rel="aberto" /></span>
  </div>
  <div class="barraDados" id="dadosFiltro">
      <div style="margin-bottom:10px">
          <dx:ASPxButton ID="btnNovo" runat="server" Text="Novo" ValidationGroup="validGroup" AutoPostBack="False"
              CssFilePath="~/App_Themes/Office2010Blue/Editors/styles.css"
              CssPostfix="Office2010Blue"
              SpriteCssFilePath="~/App_Themes/Office2010Blue/Editors/sprite.css"
              Image-Url="~/content/img/menu/novo.png" HorizontalAlign="Center" ImageSpacing="10px">
              <ClientSideEvents Click="function(s, e) {  gvdDados.AddNewRow(); }" />
          </dx:ASPxButton>
      </div>
```

**Grid mestre (colunas e configurações):**

```aspx
<dx:ASPxGridView ID="gvdDados" ClientInstanceName="gvdDados" runat="server" Theme="Office2010Blue" Width="100%"
    DataSourceID="sqlUsuarioBi" KeyFieldName="ID_USUARIO" AutoGenerateColumns="False"
    OnDetailRowExpandedChanged="gvdDados_DetailRowExpandedChanged">
    <Styles><AlternatingRow Enabled="True"></AlternatingRow></Styles>
    <Columns>
        <dx:GridViewDataTextColumn FieldName="ID_USUARIO_EMPRESA" ReadOnly="True" Visible="False" VisibleIndex="0" />
        <dx:GridViewDataComboBoxColumn Caption="NOMES" FieldName="ID_USUARIO" VisibleIndex="1">
            <PropertiesComboBox DataSourceID="sqlUsuario" TextField="NOME" ValueField="ID_USUARIO" />
        </dx:GridViewDataComboBoxColumn>
        <dx:GridViewDataTextColumn FieldName="EMPRESAS" ReadOnly="true" Visible="true" VisibleIndex="2" />
        <dx:GridViewCommandColumn VisibleIndex="3" ButtonType="Image" Width="50px">
            <CellStyle Cursor="pointer"><Paddings PaddingLeft="3px" /></CellStyle>
        </dx:GridViewCommandColumn>
        <dx:GridViewDataComboBoxColumn FieldName="EMPRESA" VisibleIndex="2" Visible="false">
            <PropertiesComboBox TextField="NM_FANTASIA" ValueField="CD_EMPRESA" DataSourceID="sqlEmpresa" />
        </dx:GridViewDataComboBoxColumn>
    </Columns>
    <SettingsCommandButton>
        <DeleteButton Text="Deletar"><Image Url="~/content/img/menu/remover.gif" /></DeleteButton>
        <CancelButton Text="Cancelar"><Image Url="~/content/img/menu/cancelar1.png" /></CancelButton>
        <UpdateButton Text="Salvar"><Image Url="~/content/img/menu/salvar.png" /></UpdateButton>
    </SettingsCommandButton>
    <SettingsBehavior ConfirmDelete="True" />
    <SettingsPager AlwaysShowPager="True" Mode="ShowAllRecords" />
    <SettingsEditing Mode="Inline" />
    <SettingsText ConfirmDelete="Excluir o acesso desse usuário ao B.I.?" />
    <SettingsDetail ShowDetailRow="True" AllowOnlyOneMasterRowExpanded="True" />
    <!-- Template DetailRow: ver abaixo -->
</dx:ASPxGridView>
```

**Template de detalhe + grid detalhe:**

```aspx
<Templates>
  <DetailRow>
    <table class="fonteGeral" cellpadding="3" cellspacing="3" width="100%">
      <tr>
        <td>
          <dx:ASPxButton ID="btnNovo" runat="server" Text="Novo" ValidationGroup="validGroup" AutoPostBack="False"
              CssFilePath="~/App_Themes/Office2010Blue/Editors/styles.css" CssPostfix="Office2010Blue"
              SpriteCssFilePath="~/App_Themes/Office2010Blue/Editors/sprite.css"
              Image-Url="~/content/img/menu/novo.png" HorizontalAlign="Center" ImageSpacing="10px">
              <ClientSideEvents Click="function(s, e) { gvdEmpresas.AddNewRow(); }" />
          </dx:ASPxButton>
        </td>
      </tr>
      <tr>
        <td>
          <dx:ASPxGridView ID="gvdEmpresas" ClientInstanceName="gvdEmpresas" runat="server" Theme="Office2010Blue" Width="100%"
              OnBeforePerformDataSelect="gvdEmpresas_BeforePerformDataSelect" OnInit="gvdEmpresas_Init"
              DataSourceID="sqlUsuarioEmpresa" AutoGenerateColumns="False" KeyFieldName="ID_USUARIO_EMPRESA"
              OnCellEditorInitialize="gvdEmpresas_CellEditorInitialize">
              <Styles><AlternatingRow Enabled="True"></AlternatingRow></Styles>
              <Columns>
                  <dx:GridViewCommandColumn VisibleIndex="3" ButtonType="Image" Width="50px">
                      <CellStyle Cursor="pointer"><Paddings PaddingLeft="3px" /></CellStyle>
                  </dx:GridViewCommandColumn>
                  <dx:GridViewDataComboBoxColumn FieldName="EMPRESA" VisibleIndex="0" Caption="EMPRESAS" />
              </Columns>
              <SettingsCommandButton>
                  <DeleteButton Text="Deletar"><Image Url="~/content/img/menu/remover.gif" /></DeleteButton>
                  <CancelButton Text="Cancelar"><Image Url="~/content/img/menu/cancelar1.png" /></CancelButton>
                  <UpdateButton Text="Salvar"><Image Url="~/content/img/menu/salvar.png" /></UpdateButton>
              </SettingsCommandButton>
              <SettingsBehavior ConfirmDelete="True" />
              <SettingsEditing Mode="Inline" />
              <SettingsText ConfirmDelete="Deseja realmente excluir o acesso desse usuario ao BI da empresa selecionada?" />
          </dx:ASPxGridView>
        </td>
      </tr>
    </table>
  </DetailRow>
</Templates>
```

**Script final da página (encerramento do loading):**

```html
<asp:HiddenField ID="nomeTela" runat="server" />
<script type="text/javascript">
    parent.MostraCarregando(false, $('#<%=nomeTela.ClientID%>').val());
</script>
```

### 3.4. Folha de estilos envolvida (`content/css/estilosInterna.css`)

```css
.fonteGeral { font-family:Tahoma; font-size:12px; color:#213357; }
.barra       { border: 1px solid #ADADAD; width:100%; margin-top:10px; }
.barraTitulo { background-color:#0B74A3; background-image:url('../img/fundo_barra.png');
               background-repeat:repeat-x; color:#FFFFFF; font-size:12px; font-family:Tahoma;
               width:100%; height:25px; font-weight:bold; cursor:pointer; }
.barraDados  { background-color:##F2F2F2; margin:10px; }   /* obs: '##' inválido — ver defeito D5 */
.barraTexto  { float:left; margin-top:5px; margin-left:10px; }
.barraSeta   { float:right; margin-top:4px; margin-right:7px; }
.barraSeta img { cursor:pointer; }
```

### 3.5. Funções JavaScript globais consumidas (`content/js/rotina.js`)

```javascript
// Alterna seta e colapsa/expande um bloco (jQuery 1.7.2)
function FechaBloco(img, div) {
    if ($("#" + img).attr("rel") == "aberto") {
        $("#" + img).attr("rel", "fechado");
        $("#" + div).slideUp(300);
        $("#" + img).attr("src", $("#" + img).attr("src").replace("seta_aberta", "seta_fechada"));
    } else {
        $("#" + img).attr("rel", "aberto");
        $("#" + div).slideDown(300);
        $("#" + img).attr("src", $("#" + img).attr("src").replace("seta_fechada", "seta_aberta"));
    }
}

// Liga/desliga o overlay "Carregando... Aguarde!" da aba hospedeira (Principal.aspx)
function MostraCarregando(mostrar, parId) {
    var maskHeight = $("#tabsPages").height()-10;
    var maskWidth  = $("#tabsPages").width() - 10;
    var id = "#dialog_" + parId;
    var carregando = "#window_" + parId;
    $(id).css({ 'width': maskWidth, 'height': maskHeight });
    $(carregando).css({ 'margin-top': ((maskHeight / 2) - $(carregando).height()),
                        'margin-left': ((maskWidth / 2) - ($(carregando).width()/2)) });
    if (mostrar) { $(id).fadeIn(); $(id).fadeTo("slow", 0.5); $(carregando).fadeIn(); }
    else { $(carregando).hide(); $(id).hide(); }
}
```

---

## 4. REGRAS DE RELACIONAMENTO E NEGÓCIO

### 4.1. Gates de acesso (executados a cada carregamento da página)

**Gate 1 — Sessão (Master Page `Interna.Master.cs`):**
```csharp
if ((bool)Session["UserLogado"] == false)
    Response.Redirect("~/Default.aspx?url=" + Request.RawUrl);
```
Qualquer exceção (sessão expirada/nula) também redireciona para o login.

**Gate 2 — Permissão por perfil (Page_Load da tela):**
```csharp
private string nomeTelaAtual = "cadastroUsuarioBi";

protected void Page_Load(object sender, EventArgs e)
{
    nomeTela.Value = nomeTelaAtual;
    try
    {
        if (new DaoAcesso().GetPaginaByChave(nomeTelaAtual, ((Usuario)Session["User"]).IDPerfil) == null)
            Response.Redirect("~/interna/SemPermissao.aspx?pag=" + nomeTelaAtual, false);
    }
    catch (Exception)
    {
        Response.Redirect("~/interna/SemPermissao.aspx?pag=" + nomeTelaAtual, false);
    }
}
```
Regra: só acessa a tela quem tiver, na tabela `ACESSO_PERFIL_PAGINA`, vínculo entre seu `ID_PERFIL` e a página de `CHAVE_CONTROLE = 'cadastroUsuarioBi'`. Sem permissão → tela "Sem permissao de acesso!".

### 4.2. Regra de negócio RN-01 — Concessão de acesso TOTAL (grid mestre)

| Passo | Comportamento |
|---|---|
| 1 | Usuário clica no botão **"Novo"** (mestre) → JS client-side `gvdDados.AddNewRow()` abre linha em edição inline |
| 2 | A coluna **NOMES** vira combo listando **TODOS** os usuários de `ACESSO_CADASTRO_USUARIO` (sem filtro de quem já tem acesso — ver defeito D6) |
| 3 | Usuário seleciona o nome e clica no ícone **Salvar** (`salvar.png`) na coluna de comando |
| 4 | O `SqlDataSource sqlUsuarioBi` executa o **InsertCommand**: insere em `USUARIO_EMPRESA` **uma linha para CADA empresa** retornada por `VW_EMPRESA_NEW` (concessão em massa) |
| 5 | Grid re-executa o SELECT e a coluna `EMPRESAS` passa a exibir a agregação das empresas |

> **Não existe UpdateCommand no mestre** — o acesso concedido não pode ser alterado nesta tela; e, na prática, o botão Excluir não é renderizado (ver 4.4), embora o DeleteCommand exista.

### 4.3. Regra de negócio RN-02 — Concessão de acesso POR EMPRESA (grid detalhe)

| Passo | Comportamento |
|---|---|
| 1 | Usuário clica no botão **expandir (+)** da linha do usuário no grid mestre |
| 2 | Evento server `gvdDados_DetailRowExpandedChanged` grava o índice visível da linha: `Session["cadastroUsuarioBiUSERCLICADO"] = e.VisibleIndex` |
| 3 | Evento server `gvdEmpresas_Init`: lê o `ID_USUARIO` da linha mestre expandida e grava em `Session["ID_USUARIO"]`; configura dinamicamente o combo da coluna EMPRESA (Text=`NM_FANTASIA`, Value=`CD_EMPRESA`, fonte `sqlEmpresaBi` = **todas** as empresas) |
| 4 | Evento server `gvdEmpresas_BeforePerformDataSelect`: grava a chave da linha mestre em `Session["USUARIOCLICADO"] = GetMasterRowKeyValue()` (que é o `ID_USUARIO`, pois `KeyFieldName="ID_USUARIO"` no mestre) |
| 5 | O detalhe executa o SELECT filtrando `WHERE ID_USUARIO = :ID_USUARIO` (parâmetro de sessão `ID_USUARIO`) e exibe as empresas já vinculadas |
| 6 | Ao clicar em **"Novo"** do detalhe → `gvdEmpresas.AddNewRow()`; o evento `gvdEmpresas_CellEditorInitialize` **troca a query do combo** para listar SOMENTE empresas ainda NÃO vinculadas ao usuário (filtro `NOT IN`) |
| 7 | Salvar → InsertCommand do `sqlUsuarioEmpresa` insere 1 linha `(NULL, :ID_USUARIO da sessão, :EMPRESA selecionada)` |

> **Somente uma linha mestre expandida por vez** (`AllowOnlyOneMasterRowExpanded="True"`).

### 4.4. Comportamento real dos botões de comando (IMPORTANTE — fidelidade DevExpress v16.2)

As `GridViewCommandColumn` do mestre e do detalhe **não definem** `ShowNewButton`, `ShowEditButton` ou `ShowDeleteButton`. No DevExpress v16.2 os defaults são:

| Propriedade | Default | Efeito neste painel |
|---|---|---|
| `ShowNewButton` | `false` | Botão "Novo" NÃO aparece na coluna (a inclusão é feita pelos botões externos) |
| `ShowEditButton` | `false` | Botão "Editar" NÃO aparece |
| `ShowDeleteButton` | `false` | **Botão "Deletar" NÃO aparece** — os `DeleteCommand` e os textos `ConfirmDelete` configurados ficam INALCANÇÁVEIS pela UI |
| `ShowUpdateButton` | `true` | Ícone **Salvar** (`salvar.png`) aparece durante a edição inline |
| `ShowCancelButton` | `false` | Ícone **Cancelar** NÃO aparece durante a edição inline |

> Comparativo: as telas irmãs `CadastroPerfil.aspx` e `CadastroUsuario.aspx` setam `ShowEditButton="true" ShowDeleteButton="true"` explicitamente — evidência de que a ausência aqui é decisão/omissão do legado. **Na reimplementação, decidir explicitamente se a exclusão será oferecida; se a fidelidade for estrita, não exibir exclusão.**

### 4.5. Filtragem dinâmica do combo de empresas (detalhe)

```csharp
// gvdEmpresas_Init — combo com TODAS as empresas (exibição)
sqlEmpresaBi.SelectCommand = @"SELECT CD_EMPRESA, DS_RAZAO_SOCIAL, NM_FANTASIA FROM VW_EMPRESA_NEW";

// gvdEmpresas_CellEditorInitialize — combo SOMENTE com empresas ainda não vinculadas (edição/inserção)
sqlEmpresaBi.SelectCommand = @"SELECT CD_EMPRESA, NM_FANTASIA FROM VW_EMPRESA_NEW
                               WHERE CD_EMPRESA NOT IN (SELECT NVL(EMPRESA,0) FROM USUARIO_EMPRESA
                                                        WHERE ID_USUARIO = " + Session["ID_USUARIO"].ToString() + ")";
sqlEmpresaBi.DataBind();
```
> Observação de segurança: a query é montada por concatenação de string com valor de sessão (risco de SQL Injection — padrão do legado; na nova stack usar parâmetros).

### 4.6. Cálculo da coluna agregada EMPRESAS (mestre)

Feito 100% no banco (não há formatação em C#/JS):

```sql
LISTAGG(SUBSTR(V.NM_FANTASIA, 1, INSTR(V.NM_FANTASIA, ' ') - 1), ' - ')
       WITHIN GROUP (ORDER BY U.EMPRESA) AS EMPRESAS
```
- Extrai de cada empresa apenas o **primeiro token** do nome fantasia (texto até o primeiro espaço).
- Concatena os tokens na **ordem do código da empresa** (`U.EMPRESA`), separados por `" - "`.
- **Caso de borda:** se `NM_FANTASIA` não contiver espaço, `INSTR` retorna `0` → `SUBSTR(x, 1, -1)` retorna `NULL` → o segmento daquela empresa fica vazio na agregação (ex.: `"TECNO -  - FILIAL"`).

### 4.7. Interações entre artefatos (matriz)

| Evento do usuário | Artefato afetado | Efeito |
|---|---|---|
| Load da página | Grid mestre | Executa SELECT agregado; exibe todos os usuários com acesso |
| Clique no título da barra | Imagem de seta | Alterna `seta_aberta.png` ↔ `seta_fechada.png`; o bloco de conteúdo NÃO colapsa (defeito D1) |
| Clique "Novo" (mestre) | Grid mestre | Nova linha inline; combo NOMES habilitado |
| Salvar (mestre) | Banco + grid mestre | INSERT em massa (1 linha por empresa); rebind |
| Expandir (+) linha mestre | Grid detalhe + Sessão | Grava 3 variáveis de sessão; detalhe executa SELECT por usuário; expande inline; colapsa qualquer outro detalhe aberto |
| Clique "Novo" (detalhe) | Grid detalhe + combo EMPRESA | Nova linha inline; combo passa a listar apenas empresas não vinculadas |
| Salvar (detalhe) | Banco + grid detalhe | INSERT de 1 empresa; rebind do detalhe |
| Fim do carregamento | Página hospedeira (`parent`) | `MostraCarregando(false, 'cadastroUsuarioBi')` oculta o overlay da aba |

### 4.8. Validações

- **Não há validadores de campo obrigatório** configurados nas colunas (nenhum `ValidationSettings` nas grids).
- `ConfirmDelete=True` nos dois grids gera diálogo de confirmação nativo do DevExpress (inalcançável, ver 4.4):
  - Mestre: `"Excluir o acesso desse usuário ao B.I.?"`
  - Detalhe: `"Deseja realmente excluir o acesso desse usuario ao BI da empresa selecionada?"`
- A integridade (empresa duplicada para o mesmo usuário) é garantida apenas pela **filtragem do combo** no detalhe (4.5); **no mestre não há qualquer proteção contra duplicidade** (ver defeito D6).
- A PK `ID_USUARIO_EMPRESA` nunca é informada pela UI nos INSERTs (parâmetro vai `NULL`) — presume-se trigger/sequence Oracle que popula a PK (mesmo padrão documentado para `ACESSO_CADASTRO_PERFIL.ID_PERFIL` no documento `usuario_regra.md`).

---

## 5. CAMADA DE DADOS (BANCO E QUERIES)

### 5.1. Conexão

| Item | Valor |
|---|---|
| Nome da connection string | `Oracle` (Web.config → `<connectionStrings>`) |
| Provider legado | `System.Data.OracleClient` |
| Formato | `Data Source=tecnovin;User ID=<schema>;Password=<senha>` |
| Consumo neste painel | 100% via `asp:SqlDataSource` declarativo (sem DAO específico); apenas o gate de permissão usa `DaoAcesso.GetPaginaByChave` |
| appSettings relevantes | `caminhoBI` (URL do B.I. externo, ex.: `http://localhost:3221/Login`) |

### 5.2. Objetos de banco consumidos

| Objeto | Tipo | Uso no painel |
|---|---|---|
| `USUARIO_EMPRESA` | **Tabela** (alvo do CRUD) | Armazena os vínculos usuário↔empresa para o B.I. |
| `ACESSO_CADASTRO_USUARIO` | Tabela | Combo de usuários; JOIN para exibir o nome |
| `VW_EMPRESA_NEW` | **View** | Catálogo de empresas (combo + JOIN de exibição + fonte do INSERT em massa) |
| `ACESSO_CADASTRO_PAGINA` | Tabela | Gate de permissão (chave `cadastroUsuarioBi`) |
| `ACESSO_PERFIL_PAGINA` | Tabela | Gate de permissão (vínculo perfil×página) |

### 5.3. Schemas esperados

#### 5.3.1. `USUARIO_EMPRESA` (tabela principal — CRUD)

| Coluna | Tipo inferido | Papel | Observação |
|---|---|---|---|
| `ID_USUARIO_EMPRESA` | NUMBER | **PK** | Nunca informada pela aplicação nos INSERTs (vai NULL) → presume-se **trigger/sequence** Oracle |
| `ID_USUARIO` | NUMBER | **FK** → `ACESSO_CADASTRO_USUARIO.ID_USUARIO` | Parâmetro `:ID_USUARIO` (Decimal) |
| `EMPRESA` | NUMBER | **FK** → `VW_EMPRESA_NEW.CD_EMPRESA` | Parâmetro `:EMPRESA` (Decimal) |

#### 5.3.2. `ACESSO_CADASTRO_USUARIO` (somente leitura neste painel)

| Coluna | Tipo inferido | Uso |
|---|---|---|
| `ID_USUARIO` | NUMBER (PK) | ValueField do combo NOMES; JOIN do SELECT mestre |
| `NOME` | VARCHAR2 | TextField do combo NOMES |
| (`LOGIN`, `SENHA`, `ID_PERFIL`, `ATIVO`, `QUANTIDADE_ACESSO`, `ATUALIZA_SENHA`, `DATA_HORA_ULTIMO_ACESSO`, ...) | — | Usadas por outras telas; não consumidas aqui |

#### 5.3.3. `VW_EMPRESA_NEW` (view — somente leitura)

| Coluna | Tipo inferido | Uso |
|---|---|---|
| `CD_EMPRESA` | NUMBER | ValueField dos combos de empresa; JOINs; fonte do INSERT em massa |
| `DS_RAZAO_SOCIAL` | VARCHAR2 | Selecionada em `sqlEmpresa`/`sqlEmpresaBi` (não exibida na UI final) |
| `NM_FANTASIA` | VARCHAR2 | TextField dos combos; base do cálculo da coluna agregada `EMPRESAS` |

#### 5.3.4. Tabelas do gate de permissão (somente leitura)

`ACESSO_CADASTRO_PAGINA(ID_PAGINA PK, URL, TITULO_ABA, CHAVE_CONTROLE, TITULO_MENU, ID_PAGINA_PAI FK auto-relac., ATIVO CHAR(1), ORDEM, TOOLTIP)`
`ACESSO_PERFIL_PAGINA(ID_PERFIL FK, ID_PAGINA FK)`

### 5.4. Queries exatas por artefato visual

#### Q1 — Combo "NOMES" do grid mestre (`sqlUsuario`)

```sql
SELECT ID_USUARIO, NOME FROM ACESSO_CADASTRO_USUARIO
```
- Sem ORDER BY, sem filtro de `ATIVO` (lista inclusive usuários inativos).

#### Q2 — SELECT do grid mestre (`sqlUsuarioBi`)

```sql
SELECT U.ID_USUARIO,
       A.NOME,
       LISTAGG(SUBSTR(V.NM_FANTASIA,1,INSTR(V.NM_FANTASIA,' ')-1), ' - ')
           WITHIN GROUP (ORDER BY U.EMPRESA) EMPRESAS
  FROM USUARIO_EMPRESA U
 INNER JOIN ACESSO_CADASTRO_USUARIO A ON U.ID_USUARIO = A.ID_USUARIO
 INNER JOIN VW_EMPRESA_NEW V          ON U.EMPRESA    = V.CD_EMPRESA
 GROUP BY U.ID_USUARIO, A.NOME
```
- Requer Oracle **11gR2+** (função `LISTAGG`).
- Uma linha por usuário que tenha **pelo menos 1 vínculo** (INNER JOINs).
- O resultado inclui a coluna virtual `ID_USUARIO_EMPRESA`? **Não** — a coluna oculta `ID_USUARIO_EMPRESA` do grid não é retornada por este SELECT (campo declarado, mas sem correspondência no resultado — vestigial, assim como `EMPRESA`).
- `KeyFieldName="ID_USUARIO"` (único por linha devido ao GROUP BY).

#### Q3 — INSERT do grid mestre (concessão em massa)

```sql
INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA)
SELECT :ID_USUARIO_EMPRESA, :ID_USUARIO, CD_EMPRESA
  FROM VW_EMPRESA_NEW
```
Parâmetros (todos `Decimal`):
| Parâmetro | Origem do valor |
|---|---|
| `:ID_USUARIO_EMPRESA` | Não editável na UI → **NULL** (PK presumivelmente via trigger/sequence) |
| `:ID_USUARIO` | Valor selecionado no combo NOMES |

#### Q4 — DELETE do grid mestre (configurado, porém inalcançável via UI — ver 4.4)

```sql
DELETE FROM USUARIO_EMPRESA WHERE ID_USUARIO = :ID_USUARIO
```
Parâmetro `:ID_USUARIO` (Decimal) ← chave da linha (`KeyFieldName`). Remove **todas** as empresas do usuário.

#### Q5 — Catálogo de empresas (`sqlEmpresa` — usado só pela coluna oculta; e `sqlEmpresaBi` — modo exibição do detalhe)

```sql
SELECT CD_EMPRESA, DS_RAZAO_SOCIAL, NM_FANTASIA FROM VW_EMPRESA_NEW
```

#### Q6 — Catálogo de empresas DISPONÍVEIS para novo vínculo (`sqlEmpresaBi` — modo edição, definido em `CellEditorInitialize`)

```sql
SELECT CD_EMPRESA, NM_FANTASIA
  FROM VW_EMPRESA_NEW
 WHERE CD_EMPRESA NOT IN (SELECT NVL(EMPRESA,0)
                            FROM USUARIO_EMPRESA
                           WHERE ID_USUARIO = <ID_USUARIO_da_sessao>)
```
> Montada por concatenação C# com `Session["ID_USUARIO"]`.

#### Q7 — SELECT do grid detalhe (`sqlUsuarioEmpresa`)

```sql
SELECT B.ID_USUARIO_EMPRESA,
       B.ID_USUARIO,
       B.EMPRESA   AS EMPRESA1,
       E.NM_FANTASIA AS EMPRESA
  FROM USUARIO_EMPRESA B
 INNER JOIN VW_EMPRESA_NEW E ON B.EMPRESA = E.CD_EMPRESA
 WHERE ID_USUARIO = :ID_USUARIO
```
Parâmetro de SELECT: `SessionParameter SessionField="ID_USUARIO"` (Decimal).
- A coluna exibida no grid é `EMPRESA` (texto `NM_FANTASIA`); `EMPRESA1` (código) não tem coluna declarada no grid (não exibida).
- `KeyFieldName="ID_USUARIO_EMPRESA"`.

#### Q8 — INSERT do grid detalhe (concessão unitária)

```sql
INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA)
VALUES (:ID_USUARIO_EMPRESA, :ID_USUARIO, :EMPRESA)
```
| Parâmetro | Tipo | Origem |
|---|---|---|
| `:ID_USUARIO_EMPRESA` | Decimal | NULL (trigger/sequence) |
| `:ID_USUARIO` | Decimal | **SessionParameter** `Session["ID_USUARIO"]` |
| `:EMPRESA` | Decimal | Combo da linha em edição (`CD_EMPRESA`) |

#### Q9 — DELETE do grid detalhe (configurado, inalcançável via UI; contém o defeito D2)

```sql
DELETE FROM USUARIO_EMPRESA WHERE ID_USUARIO_EMPRESA = :ID_USUARIO_EMPRESA
```
Parâmetro: `SessionParameter SessionField="USUARIOCLICADO"` — **atenção**: essa sessão guarda a **chave da linha MESTRE** (`ID_USUARIO`), não o `ID_USUARIO_EMPRESA` da linha detalhe (defeito D2, Anexo D).

#### Q10 — Gate de permissão (`DaoAcesso.GetPaginaByChave(chave, idPerfil)`)

```sql
SELECT ACP.ID_PAGINA, ACP.URL, ACP.TITULO_ABA, ACP.CHAVE_CONTROLE, ACP.TITULO_MENU,
       ACP.ID_PAGINA_PAI, ACP.ATIVO, ACP.ORDEM, ACP.TOOLTIP
  FROM acesso_cadastro_pagina ACP
 INNER JOIN ACESSO_PERFIL_PAGINA APP ON APP.ID_PAGINA = ACP.ID_PAGINA
 WHERE ACP.CHAVE_CONTROLE = 'cadastroUsuarioBi'
   AND APP.ID_PERFIL = <idPerfil_do_usuario_logado>
```
Retorna a 1ª linha ou `null` (→ redirect `SemPermissao.aspx?pag=cadastroUsuarioBi`).

#### Q11 — Verificação de acesso ao B.I. no menu principal (`DaoAcesso.GetUsuario(idUsuario)`)

```sql
SELECT U.ID_USUARIO, U.NOME, U.LOGIN, BI.EMPRESA, BI.ID_USUARIO_EMPRESA
  FROM ACESSO_CADASTRO_USUARIO U
  LEFT JOIN USUARIO_EMPRESA BI ON BI.ID_USUARIO = U.ID_USUARIO
 WHERE U.ID_USUARIO = '<id>'
```
Usada por `Principal.aspx` para decidir a visibilidade do atalho `lnkBi` (`ID_USUARIO_EMPRESA` nulo → oculta).

### 5.5. Relacionamentos (DER textual)

```
ACESSO_CADASTRO_USUARIO                 VW_EMPRESA_NEW (view)
| ID_USUARIO (PK)                       | CD_EMPRESA
| NOME                                  | DS_RAZAO_SOCIAL
|                                       | NM_FANTASIA
+------ 1:N ------+              +------ 1:N ------+
                  |              |
                  v              v
               USUARIO_EMPRESA  (N:N usuário × empresa → acesso ao B.I.)
               | ID_USUARIO_EMPRESA (PK — trigger/sequence presumida)
               | ID_USUARIO  (FK → ACESSO_CADASTRO_USUARIO.ID_USUARIO)
               | EMPRESA     (FK → VW_EMPRESA_NEW.CD_EMPRESA)

Gate de permissão (transversal ao sistema):
ACESSO_CADASTRO_PERFIL --1:N--> ACESSO_PERFIL_PAGINA --N:1--> ACESSO_CADASTRO_PAGINA
                                                              (CHAVE_CONTROLE = 'cadastroUsuarioBi')
```

### 5.6. Dados mínimos necessários para reprodução

```sql
-- Usuário (para aparecer no combo NOMES)
INSERT INTO ACESSO_CADASTRO_USUARIO (ID_USUARIO, NOME, LOGIN, SENHA, ID_PERFIL, ATIVO)
VALUES (1, 'USUARIO TESTE', 'teste', '123', 1, 'S');

-- Vínculo B.I. (uma linha por empresa concedida)
INSERT INTO USUARIO_EMPRESA (ID_USUARIO_EMPRESA, ID_USUARIO, EMPRESA) VALUES (100, 1, 1);

-- Permissão da tela ao perfil
-- 1) página cadastrada:
-- INSERT INTO acesso_cadastro_pagina (ID_PAGINA, URL, TITULO_ABA, CHAVE_CONTROLE, TITULO_MENU, ID_PAGINA_PAI, ATIVO, ORDEM, TOOLTIP)
-- VALUES (<id>, '~/interna/acesso/CadastroUsuarioBi.aspx', '<titulo aba>', 'cadastroUsuarioBi', '<titulo menu>', <id_pai>, 'S', <ordem>, '<tooltip>');
-- 2) vínculo perfil×página:
-- INSERT INTO ACESSO_PERFIL_PAGINA (ID_PERFIL, ID_PAGINA) VALUES (<perfil>, <id_pagina>);
```

---

## Anexo A — Stack, Arquivos e Assets

### A.1. Arquivos-fonte do painel

| Arquivo | Papel |
|---|---|
| `Treis.Web/interna/acesso/CadastroUsuarioBi.aspx` | Markup (view) |
| `Treis.Web/interna/acesso/CadastroUsuarioBi.aspx.cs` | Code-behind (5 handlers: `Page_Load`, `gvdDados_DetailRowExpandedChanged`, `gvdEmpresas_BeforePerformDataSelect`, `gvdEmpresas_Init`, `gvdEmpresas_CellEditorInitialize`) |
| `Treis.Web/interna/acesso/CadastroUsuarioBi.aspx.designer.cs` | Designer (declaração dos controles) |
| `Treis.Web/interna/Interna.Master` (+`.cs`) | Master page: gate de sessão; injeta `jquery-1.7.2.min.js` e `rotina.js`; CSS `estilosInterna.css`; `ScriptManager` |
| `Treis.Web/Principal.aspx` (+`.cs`) | Shell de abas/iframes + menu em árvore (ver Anexo B) |
| `Treis.Web/CarregaBi.aspx.cs` | Redirect para o B.I. externo |
| `Treis.Web/content/css/estilosInterna.css` | Estilos da barra e fonte |
| `Treis.Web/content/js/rotina.js` | `FechaBloco`, `MostraCarregando` |
| `Treis.Data/DaoAcesso.cs` | `GetPaginaByChave` (gate), `GetUsuario(id)` (atalho B.I.) |
| `Treis.Data/acesso/Usuario.cs` | Entidade da sessão (`IDUsuario, Nome, Login, IDPerfil, QuantidadeAcesso, Atualiza_Senha`) |

### A.2. Imagens utilizadas

| Asset | Uso |
|---|---|
| `content/img/seta_aberta.png` / `seta_fechada.png` | Seta da barra de título (estado `rel="aberto"/"fechado"`) |
| `content/img/fundo_barra.png` | Background repeat-x da `.barraTitulo` |
| `content/img/menu/novo.png` | Ícone dos botões "Novo" |
| `content/img/menu/salvar.png` | Ícone do botão Update (Salvar) |
| `content/img/menu/cancelar1.png` | Ícone do botão Cancel (configurado, não exibido) |
| `content/img/menu/remover.gif` | Ícone do botão Delete (configurado, não exibido) |
| `content/img/carregando.gif` | Overlay "Carregando... Aguarde!" da aba |
| `content/img/bi4.png` | Ícone do atalho "Acesso ao BI" no shell |

### A.3. Dependências DevExpress

- Assembly `DevExpress.Web.v16.2, Version=16.2.8.0`; controles: `ASPxGridView`, `ASPxButton`.
- Tema `Office2010Blue` (`Theme="Office2010Blue"` nas grids; botões usam `CssPostfix="Office2010Blue"` + `~/App_Themes/Office2010Blue/Editors/styles.css` + `sprite.css`).
- `ASPxHttpHandlerModule` registrado no `Web.config` (handler `DX.ashx`).

---

## Anexo B — Ciclo de Vida Completo (Hosting em Abas)

O painel **não roda standalone**: é carregado dentro de um **iframe** no shell `Principal.aspx`:

1. O menu é uma `ASPxTreeView` (`treMenu`) montada a partir de `DaoAcesso.GetPaginas(IDPerfil)`. Cada nó tem `Name = CHAVE_CONTROLE` (→ `"cadastroUsuarioBi"`) e `Target = "<URL>|<TITULO_ABA>"`.
2. O clique no nó dispara `AbrePagina` → `AddAba(url, index, text, name)` (JS em `Principal.aspx`):

```javascript
function AddAba(url, index, text, name) {
    var content = "";
    content += '<div id="dialog_' + name + '" class="boxes"></div><div id="window_' + name + '" class="window">';
    content += '<img src="content/img/carregando.gif"/>';
    content += '<div>Carregando... Aguarde!</div>';
    content += '</div>';
    content += '<iframe name="' + name + '" scrolling="auto" frameborder="0" src="' + url + '" style="width:100%;height:98%;"></iframe>';
    if (getTabIndexByTitle(text) == -1) {
        callAtualizar.PerformCallback(name);      // registra acesso (ACESSO_VISUALIZACAO_PAGINA)
        OcultaMenu(); OcultaTopo();
        createNewTab('tabsPages', text, content, null, true);
        MostraCarregando(true, name);             // liga overlay
    } else {
        showTab('tabsPages', getTabIndexByTitle(text)[1]);
        OcultaMenu(); OcultaTopo();
    }
}
```

3. Contrato de nomenclatura (crítico para o loading desligar): o overlay é `dialog_<CHAVE_CONTROLE>` / `window_<CHAVE_CONTROLE>` e o iframe `name="<CHAVE_CONTROLE>"`. Ao terminar de carregar, a página interna executa `parent.MostraCarregando(false, '<CHAVE_CONTROLE>')` — por isso o `HiddenField nomeTela` recebe `"cadastroUsuarioBi"` no `Page_Load`.
4. O callback `callAtualizar` executa `DaoAcesso.RegistraAcessoMenu(chave, idUsuario)` → INSERT/UPDATE em `ACESSO_VISUALIZACAO_PAGINA` (contagem para o menu "Mais Acessados").

---

## Anexo C — Contrato de Estado de Sessão

Variáveis de `Session` lidas/escritas por este painel:

| Chave | Tipo | Escrita | Leitura | Finalidade |
|---|---|---|---|---|
| `UserLogado` | bool | Login/Logout | Master Page | Gate de autenticação |
| `User` | `Usuario` | Login | `Page_Load` (IDPerfil) | Gate de permissão |
| `cadastroUsuarioBiUSERCLICADO` | int (VisibleIndex) | `gvdDados_DetailRowExpandedChanged` | `gvdEmpresas_Init` | Índice da linha mestre expandida |
| `ID_USUARIO` | string | `gvdEmpresas_Init` (lido do `gvdDados.GetDataRow(idx)["ID_USUARIO"]`) | `sqlUsuarioEmpresa` (SelectParameter), `gvdEmpresas_CellEditorInitialize` (monta Q6), `sqlUsuarioEmpresa` (InsertParameter) | Usuário corrente do detalhe |
| `USUARIOCLICADO` | object (chave da linha mestre = `ID_USUARIO`) | `gvdEmpresas_BeforePerformDataSelect` (`GetMasterRowKeyValue()`) | `sqlUsuarioEmpresa` (DeleteParameter, como `:ID_USUARIO_EMPRESA`) | Alvo do DELETE do detalhe (defeito D2) |

> **Fragilidade:** as chaves `ID_USUARIO` e `USUARIOCLICADO` são genéricas e compartilhadas com outras telas do sistema; a navegação entre abas pode causar colisão de estado (característica do legado a ser eliminada na nova stack, que deve ser stateless).

---

## Anexo D — Defeitos/Comportamentos Latentes do Legado

| # | Descrição | Impacto | Recomendação p/ nova stack |
|---|---|---|---|
| D1 | `FechaBloco('imgFiltro','dadosDados')` referencia o id **`dadosDados`**, mas a div real é **`dadosFiltro`** | O clique no título só alterna a seta; o conteúdo nunca colapsa | Corrigir o id e manter o comportamento de colapsar |
| D2 | DELETE do detalhe usa `Session["USUARIOCLICADO"]` (que contém o `ID_USUARIO` da linha **mestre**) como `:ID_USUARIO_EMPRESA` | Se executado, excluiria a linha cuja PK coincide com o ID do usuário — **não** a linha clicada | Usar a PK real da linha detalhe (`ID_USUARIO_EMPRESA`) |
| D3 | Colunas de comando sem `ShowDeleteButton/ShowEditButton` (default `false`) | Botões Excluir/Editar **não são renderizados**; `DeleteCommand` e textos `ConfirmDelete` são configuração morta | Decidir explicitamente se haverá exclusão; se sim, implementá-la corretamente |
| D4 | `ID_USUARIO_EMPRESA` inserido como `NULL` pela UI | Depende de trigger/sequence Oracle não documentada no código | Confirmar no banco; na nova stack, gerar explicitamente (identity/sequence) |
| D5 | CSS `.barraDados { background-color:##F2F2F2; }` com `##` inválido | Declaração ignorada pelo browser (fundo fica transparente/branco) | Usar `#F2F2F2` |
| D6 | Combo NOMES (mestre) lista **todos** os usuários, sem excluir quem já tem acesso | Nova concessão a usuário já habilitado **duplica** todas as linhas em `USUARIO_EMPRESA` | Filtrar usuários já habilitados ou aplicar constraint única `(ID_USUARIO, EMPRESA)` |
| D7 | Queries montadas por concatenação (Q6, Q10, Q11) | Risco de SQL Injection | Queries parametrizadas (Dapper/ORM) |
| D8 | Colunas declaradas e nunca exibidas (`ID_USUARIO_EMPRESA` no SELECT mestre ausente; coluna `EMPRESA` oculta no mestre; `EMPRESA1` no detalhe sem coluna) | Código vestigial; não afeta o funcionamento | Podem ser eliminadas na reimplementação |
| D9 | `sqlEmpresa` (SqlDataSource) alimenta apenas a coluna oculta do mestre | Query extra executada a cada load, sem uso visível | Eliminar |

---

## Anexo E — Checklist de Fidelidade para Reimplementação

**Estrutura/Navegação**
- [ ] Rota equivalente a `interna/acesso/CadastroUsuarioBi.aspx`, protegida por autenticação (sessão → JWT) e por política de permissão com a chave `cadastroUsuarioBi`.
- [ ] Tela "Sem permissão" equivalente para quem não tiver o vínculo perfil×página.
- [ ] Cabeçalho colapsável azul `#0B74A3` com o texto exato **"Usuários com acesso ao B.I. por empresas"** e seta de colapsar/expandir.

**Grid Mestre**
- [ ] Colunas: `NOMES` (combo de usuários: value `ID_USUARIO`, text `NOME`), `EMPRESAS` (texto read-only agregado), coluna de ações (50px, ícones).
- [ ] Dados: 1 linha por usuário com ≥1 vínculo (GROUP BY + LISTAGG conforme Q2; ordenação da agregação por código da empresa; separador `" - "`).
- [ ] Botão "Novo" (ícone `novo.png`) → inserção inline; ao salvar, executar o INSERT em massa (Q3: 1 linha por empresa de `VW_EMPRESA_NEW`).
- [ ] Paginação: exibir todos os registros (sem paginação real), zebra (alternating rows) habilitada, largura 100%.

**Grid Detalhe (master-detail)**
- [ ] Expansão por linha (+), apenas um detalhe aberto por vez.
- [ ] Lista empresas do usuário expandido (Q7: texto `NM_FANTASIA`).
- [ ] Botão "Novo" do detalhe → combo com **somente empresas não vinculadas** (Q6); salvar executa Q8.

**Dados/Infra**
- [ ] Mesmas 5 entidades Oracle (`USUARIO_EMPRESA`, `ACESSO_CADASTRO_USUARIO`, `VW_EMPRESA_NEW`, `ACESSO_CADASTRO_PAGINA`, `ACESSO_PERFIL_PAGINA`) — **sem alteração estrutural no banco** (Regra 1 do CLAUDE.md).
- [ ] Substituir `System.Data.OracleClient` por `Oracle.ManagedDataAccess.Core` + Dapper (Regra 2 do CLAUDE.md) mantendo as queries Q1–Q11 semanticamente idênticas.
- [ ] Eliminar dependência de `Session` (estado do detalhe via parâmetros de rota/corpo na API).
- [ ] Expor endpoints versionados `/api/v1/...` (sugestão: `GET /api/v1/acesso-bi/usuarios`, `GET /api/v1/acesso-bi/usuarios/{id}/empresas`, `POST /api/v1/acesso-bi/usuarios` (concessão total), `POST /api/v1/acesso-bi/usuarios/{id}/empresas` (concessão unitária), `GET /api/v1/empresas?disponiveisPara={idUsuario}`).

**Correções recomendadas (registrar como débito técnico/decisão de produto)**
- [ ] D1 (colapso da barra), D2 (DELETE do detalhe), D3 (excluir ou não?), D6 (duplicidade) — ver Anexo D.

---

**Nota de Engenharia:** Documento autocontido gerado por engenharia reversa do código-fonte legado em 31/07/2026. Todas as queries, marcações, nomes de eventos, variáveis de sessão, textos de UI e comportamentos foram transcritos fielmente dos arquivos `CadastroUsuarioBi.aspx`, `CadastroUsuarioBi.aspx.cs`, `Interna.Master(.cs)`, `Principal.aspx(.cs)`, `CarregaBi.aspx.cs`, `DaoAcesso.cs`, `estilosInterna.css`, `rotina.js` e `Web.config`. A próxima IA não precisa de acesso ao código original para recriar o painel.
