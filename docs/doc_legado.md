# 🧠 Documentação Completa de Domínios e Artefatos do Sistema TreisTecnovin

> **Propósito**: Documento de referência para orientar uma Inteligência Artificial na renovação/recriação completa da aplicação TreisTecnovin em tecnologias modernas (.NET 8/9, ASP.NET Core Web API, Blazor/React).
> **Público-alvo**: Agentes de IA, desenvolvedores, arquitetos de software.
> **Última atualização**: 28/07/2026

---

## Índice

1. [Sumário Executivo](#1-sumário-executivo)
2. [Visão Geral do Sistema](#2-visão-geral-do-sistema)
3. [Arquitetura do Sistema Legado](#3-arquitetura-do-sistema-legado)
4. [Mapeamento Completo de Domínios de Negócio](#4-mapeamento-completo-de-domínios-de-negócio)
5. [Catálogo de Artefatos - Camada de Dados (DAOs)](#5-catálogo-de-artefatos---camada-de-dados-daos)
6. [Catálogo de Artefatos - Entidades de Domínio](#6-catálogo-de-artefatos---entidades-de-domínio)
7. [Catálogo de Artefatos - Camada Web (ASP.NET Web Forms)](#7-catálogo-de-artefatos---camada-web-aspnet-web-forms)
8. [Catálogo de Artefatos - Windows Service](#8-catálogo-de-artefatos---windows-service)
9. [Catálogo de Artefatos - Utilitários](#9-catálogo-de-artefatos---utilitários)
10. [Mapeamento de Banco de Dados Oracle](#10-mapeamento-de-banco-de-dados-oracle)
11. [Regras de Negócio e Lógicas Específicas](#11-regras-de-negócio-e-lógicas-específicas)
12. [Fluxos de Dados Críticos](#12-fluxos-de-dados-críticos)
13. [Guia de Migração para .NET 8/9](#13-guia-de-migração-para-net-89)
14. [Checklist de Verificação para IA](#14-checklist-de-verificação-para-ia)

---

## 1. Sumário Executivo

### O que é o TreisTecnovin?

Sistema de Gestão Empresarial (ERP) com módulos de Business Intelligence (BI), desenvolvido para a empresa Tecnovin. O sistema abrange gestão de compras, vendas, financeiro, fluxo de caixa, DRE (Demonstrativo de Resultados do Exercício), planejamento financeiro, controle de acesso e monitoramento de sessões.

### Stack Atual (Legado)

| Componente | Tecnologia |
|------------|-----------|
| **Framework** | .NET Framework 4.8 |
| **Interface** | ASP.NET Web Forms + DevExpress v16.2 (ASPxGridView, ASPxPivotGrid) |
| **Acesso a Dados** | System.Data.OracleClient (DEPRECIADO) |
| **Banco de Dados** | Oracle Database (via stored procedures, packages, views e REF CURSORs) |
| **Serviços Windows** | Windows Service (.NET Framework) com System.Timers.Timer |
| **Autenticação** | Forms Authentication + Session State (HttpContext.Current.Session) |
| **Camada de Dados** | ADO.NET puro com DataSet/DataTable, sem tipagem forte |

### Stack Alvo (Modernização)

| Componente | Tecnologia |
|------------|-----------|
| **Framework** | .NET 8/9 |
| **Backend** | ASP.NET Core Web API (REST/JSON) |
| **Frontend** | Blazor WebAssembly ou React/Vite + TypeScript |
| **Acesso a Dados** | Oracle.ManagedDataAccess.Core + Dapper |
| **Banco de Dados** | Oracle Database (PRESERVADO SEM ALTERAÇÕES) |
| **Serviços** | Worker Service (.NET) com BackgroundService |
| **Autenticação** | JWT Bearer Token |

### Estrutura da Solution Atual (Legada)

```
TreisTecnovin.sln
├── Treis.Web/          # ASP.NET Web Forms (UI)
├── Treis.Data/         # Class Library - Acesso a Dados + Modelos
├── Treis.Util/         # Class Library - Utilitários
├── Treis.Service/      # Windows Service (tarefas agendadas)
└── Treis.Teste/        # Aplicação WinForms de testes manuais
```

---

## 2. Visão Geral do Sistema

### Navegadores/Módulos (User Stories Macro)

O sistema é acessado por usuários com perfis de acesso específicos. Após login, o usuário vê um menu lateral baseado em suas permissões. Abaixo está o catálogo completo de funcionalidades:

### 2.1. Módulo de Acesso / Segurança (Prioridade P0 - CRÍTICA)

Funcionalidade de login, cadastro de usuários, perfis, permissões e controle de acesso a páginas e relatórios.

**Fluxo de Autenticação**:
1. Usuário acessa `Default.aspx` (tela de login)
2. Informa login e senha
3. Sistema consulta `ACESSO_CADASTRO_USUARIO` validando `LOGIN`, `SENHA`, `ATIVO = 'S'`
4. Em caso de sucesso:
   - Retorna `ID_USUARIO`, `NOME`, `LOGIN`, `ID_PERFIL`, `QUANTIDADE_ACESSO`, `ATUALIZA_SENHA`
   - Registra acesso (`SalvaAcessoUsuario`) atualizando `DATA_HORA_ULTIMO_ACESSO` e incrementando `QUANTIDADE_ACESSO`
   - Carrega páginas permitidas via `GetPaginas(IDPerfil)` que consulta `ACESSO_CADASTRO_PAGINA` JOIN `ACESSO_PERFIL_PAGINA`
   - Constrói menu hierárquico (páginas pai/filhas)
5. Redireciona para `Principal.aspx` (dashboard/BI)

**Lógica de Menu Hierárquico (GetPaginas)**:
- Busca páginas filhas do perfil (INNER JOIN com `ACESSO_PERFIL_PAGINA`)
- Busca também as páginas PAI dessas filhas (subquery com UNION)
- Monta estrutura de árvore: pai → filhos
- Cada página tem: `ID_PAGINA`, `URL`, `TITULO_ABA`, `CHAVE_CONTROLE`, `TITULO_MENU`, `ID_PAGINA_PAI`, `ORDEM`, `TOOLTIP`

**Funcionalidades do Módulo de Acesso**:
- Cadastro de Perfil (`CadastroPerfil.aspx`)
- Cadastro de Usuário (`CadastroUsuario.aspx`)
- Alterar Senha (`AlteraSenha.aspx`)
- Verificação de Relatórios (`VerificaRelatorios.aspx`) - controle de acesso a relatórios

### 2.2. Módulo de Compras (Prioridade P1 - ALTA)

Gestão completa do processo de compras, comitê de compras, previsão e controle por nota fiscal.

**Submódulos**:

1. **Cadastro Comitê Compras** (`CadastroComiteCompras.aspx`)
   - Gestão de grupos/comitês de compras
   - Tabelas: `COMITE_COMPRA`, `COMITE_COMPRA_ITEM`
   - Gerencia vigências de itens (`DT_VIGENCIA_INICIAL`, `DT_VIGENCIA_FINAL`)

2. **Cadastro Previsão Compra** (`CadastroPrevisaoCompra.aspx`)
   - Definição de períodos de compra (`COMPRA_PERIODO`: `DT_INICIAL`, `DT_FINAL`, `DS_PERIODO`)
   - Previsão de itens por período (`COMPRA_PERIODO_PRODMAT`)
   - Replicação de itens entre períodos
   - Associação com produtos do catálogo HDS (`PRODMAT`)

3. **Resumo Anual Compras** (`ResumoAnualCompras.aspx`)
   - Visão consolidada de compras por mês/ano
   - Package Oracle: `packageCompraNf.sp_realizado`
   - Retorna 3 cursores: `p_resultado` (dados), `p_total` (totais), `p_grafico` (dados para gráfico), `p_grafico2`

4. **Comitê Compras NF** (`ComiteCompras_NF.aspx`)
   - Análise detalhada por nota fiscal do comitê
   - Procedimento: `packageCompraNf.sp_realizado`
   - Parâmetros: empresa, data, atual, vigência, mostraEstabelecimento, estabelecimento

5. **Controle Compras Mês NF** (`ControleComprasMes_NF.aspx`)
   - Controle mensal de compras por NF
   - Previsto vs Realizado por item (`packageCompraNf.sp_previsto_realizado_item`)

6. **Progressão Preço NF** (`ProgressaoPrecoNF.aspx`)
   - Evolução histórica de preços por produto/NF
   - Procedure: `packageCompraNf.sp_progressao_preco`

7. **Compras Centro Custo** (`ComprasCentroCusto.aspx`)
   - Visão de compras alocadas por centro de custo
   - Procedures: `sp_agrup_conta_ccusto`, `sp_agrup_conta_ccusto_detalhe`, `sp_agrup_conta_ccusto_det_prod`

8. **Previsão Compra Almoxarifado** (`PrevisaoCompraAlmoxarifado.aspx`)
   - Previsão de compras específica para almoxarifado

9. **Relatório de Compras** 
   - `packageCompraNf.sp_relatorio_compra` (versão original)
   - `packageCompraNf.sp_relatorio_compra2` (versão expandida com estabelecimento)

10. **Gestão de CFOP** (Transferências e Exceções)
    - Tabelas: `CFOP_TRANSFERENCIA`, `CFOP_EXCECAO`, `LOG_CFOP_TRANSFERENCIA`, `LOG_CFOP_EXCECAO`
    - CRUD completo de CFOPs de transferência e exceção
    - Log de alterações com usuário e data

### 2.3. Módulo Financeiro - Fluxo de Caixa (Prioridade P1)

Monitoramento financeiro com múltiplas visões e níveis de detalhamento.

**Submódulos**:

1. **Posição Financeira Mês** (`PosicaoFinanceiraMes.aspx`)
   - Visão consolidada da posição financeira por mês
   - Procedimento: `packageFinanceiroNew.sp_posicao`
   - Parâmetros: empresa, data inicial, data final, estabelecimento, consideraConf, ajusteDia
   - VERSÕES MÚLTIPLAS (3 packages):
     - `packageFinanceira.sp_posicao` (legado)
     - `packageFinanceiroNew.sp_posicao` (versão atual)
     - `PKG_POSICAO_FINANCEIRA_SREAL.sp_posicao` (versão "sem real")

2. **Posição Semanal** (`PosicaoSemanal.aspx`)
   - Visão semanal com remoção dinâmica de colunas vazias
   - Procedimento: `packageFinanceiroNew.sp_posicao_semanal`
   - Lógica de pós-processamento: remove colunas de semanas sem dados (primeira linha vazia)

3. **Posição Financeira Portador** (`PosicaoFinanceiraPortador.aspx`)
   - Visão por portador/banco
   - Procedures: `packageFinanceiroNewPor.sp_posicao_portador`, `sp_posicao_portador_detalhe`

4. **Resumo Posição Financeira**
   - Visão resumida com 3 cursores: principal, externo, aplicações
   - Procedimento: `packageFinanceiroNew.sp_resumo_posicao`

5. **Previsto vs Realizado (Diferença)**
   - Comparação entre previsto e realizado
   - Procedimento: `packageFinanceiroNew.sp_posicao_prev_real`

6. **Detalhamento de Posição Financeira**
   - Drill-down por dia, agrupamento e conta
   - Procedures: `sp_posicao_detalhe` (v3 packages), `sp_posicao_prev_real_detalhe`

7. **Fluxo de Caixa** (`FluxoDre.aspx`)
   - Visão detalhada de contas a pagar/receber
   - Procedimento: `packageFinanceiro.sp_fluxo_caixa`
   - Retorna 2 cursores: `r_resultado_master`, `r_resultado_detalhe`
   - Parâmetros: tipo (R=receber, P=pagar), data inicial, data final, empresa, portador

8. **Fluxo de Caixa Analítico**
   - Detalhamento a nível de documento
   - Procedimento: `packageFinanceiro.sp_fluxo_caixa_analitico`
   - Remove colunas: PORT, PORTADOR, CLIENTE, NOME_CLIENTE

9. **Fluxo Contábil**
   - Visão contábil de contas a pagar
   - Procedimento: `packageFinanceiro.sp_fluxo_ccontabil_pagar`

10. **Totais Fluxo de Caixa**
    - Resumo geral de entradas/saídas
    - Procedimento: `packageFinanceiro.sp_resumo_geral_flxcxa`

### 2.4. Módulo DRE (Demonstrativo de Resultados) (Prioridade P1)

Três visões diferentes do DRE, cada uma com sua própria package Oracle:

1. **Fluxo DRE** (`FluxoDre.aspx`)
   - Package: `packageFinanceiro.sp_fluxo_dre`
   - DRE padrão com visão anual

2. **DRE Homologado** (`DRE_HOMOLOG.aspx`)
   - Package: `packageFinanceiroHomolog.sp_fluxo_dre`
   - DRE oficial homologado
   - Detalhamento: `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev`, `sp_fluxo_dre_detalha_prev_mut`
   - Documento detalhe: `sp_documento` (com 8 parâmetros incluindo série NF)

3. **DRE Out** (`DRE_OUT.aspx`)
   - Package: `packageFinanceiroOut.sp_fluxo_dre`
   - DRE para sistemas externos
   - Detalhamento: `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev`

### 2.5. Planejamento Financeiro (Prioridade P1)

1. **Planejamento Financeiro** (`Planejamento.aspx`)
   - Planejamento anual com projeções
   - Entities: `PLANEJAMENTO_DRE`

2. **Projeção Financeira** (`ProjecaoFinanceira.aspx`)
   - Projeções futuras com cenários
   - Procedimento: `packageProjecaoFinanceira.sp_projecao`
   - Detalhamento: `sp_projecao_detalhe`

3. **Projeção Fluxo DRE** (`ProjecaoFluxoDre.aspx`)
   - Projeção do fluxo de DRE
   - Procedimento: `packagePlanejamentoDre.sp_projecao`
   - Percentuais: `sp_percentual_agrupamento`

4. **Contas Pagar/Receber** (`ContasPagarReceber.aspx`)
   - Controle de contas a pagar e receber

### 2.6. Cadastro de Agrupamentos DRE (Prioridade P3)

Gestão da estrutura hierárquica do DRE:
- Tabela mestre: `AGRUPAMENTO_DRE`
- Tabela de detalhes por empresa: `AGRUPAMENTO_DRE_EMPRESA_DET`
- CRUD completo via `DaoPainel.UpDateCadastroComite`, `InsertCadastroComite`, `DeleteAgrupamentos`
- Campos: `NOME_AGRUPAMENTO`, `ORDEM_EXIBICAO`, `TIPO_AGRUPAMENTO`, `ID_AGRUPAMENTO_PAI`, `FORMA_EXIBICAO_DETALHAMENTO`, `CENARIO_PREVISAO`, `PERC_APLICAR_PROJECAO_FIN`, `ORIGEM_INFORMACAO_PROJECAO_FIN`

### 2.7. Comercial - Vendas (Prioridade P2)

Análise de desempenho de vendas com múltiplas perspectivas.

**Submódulos**:

1. **Análise de Vendas** (`AnaliseVendas.aspx`)
   - Visão detalhada de vendas
   - Procedimento: `packageVendas.sp_painel_vendas_mi` (parâmetros: data inicial, data final, linha, empresa, operação)
   - Package BI: `packageComercial.prc_result_vendas`

2. **Análise Vendas Pivot** (`AnaliseVendasPivot.aspx`)
   - Tabela dinâmica de vendas

3. **Análise Cliente** (`AnaliseCliente.aspx`)
   - Ranking de clientes
   - Procedimento: `packageBi.sp_receita_empr_cliente_comp`
   - Detalhamento por produto: `PKG_BI.FN_RECEITA_EMPR_PRODUTO` (função table-valued)
   - Detalhamento por NF: `PKG_BI.FN_RECEITA_EMPRESA_DOC` (função table-valued)

4. **Plano Vendas Resultado** (`PlanoVendasResultado.aspx`)
   - Plano vs realizado de vendas
   - Procedimento: `packageBi.sp_plano_vendas_resultado`

5. **Comercial Mercado Interno**
   - Visão específica para mercado interno
   - Procedimento: `packageVenda.sp_comercial_mi`
   - Pós-processamento: remove colunas de diferença que não são do mês atual, remove colunas decimais zeradas, remove última linha (total)

6. **Comercial Valores MI**
   - Valores do mercado interno
   - Procedimento: `packageVenda.sp_comercial_vlr_mi`

7. **Margens**
   - Análise de margens por período
   - Procedimento: `packageBiPeriodo.sp_receita_empresa_doc_it_rel`

### 2.8. Financeiro - Prazo Médio (Prioridade P2)

Métricas de prazo médio de recebimento e pagamento.

**Submódulos**:

1. **Pivot Prazo Médio** (`PivotPrazoMedio.aspx`)
   - Tabela dinâmica de prazos médios

2. **PZM Recebimento** (`PzmRecebimento.aspx`)
   - Prazo médio de recebimento
   - Funções Oracle (3 níveis de drill-down):
     - Nível 1 - Mensal: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_MENSAL_RECEBE` / `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_MENSAL`
     - Nível 2 - Pessoa: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_RECEBE` / `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_PESSOA`
     - Nível 3 - Documentos: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_RECEBIMENTO` / `PKG_PRAZO_MEDIO.FN_RECEBIMENTO`
   - Empresa: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_EMPRESA_RECEBE` / `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_EMPRESA`

3. **PZM Pagamento** (`PzmPagamento.aspx`)
   - Prazo médio de pagamento
   - Funções Oracle:
     - Nível 1 - Mensal: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_MENSAL_PAGA` (consolidado com UNION ALL de 7 tipos: G, I, O, U, L, M, S)
     - Nível 2 - Pessoa: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_PAGA` / `PKG_PRAZO_MEDIO.FN_PAGAMENTO_PESSOA`
     - Nível 3 - Documentos: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PAGAMENTO` / `PKG_PRAZO_MEDIO.FN_PAGAMENTO`
   - Empresa: `PKG_FINANCEIRO.FN_PRAZO_MEDIO_EMPRESA_PAGA` / `PKG_PRAZO_MEDIO.FN_PAGAMENTO_EMPRESA`

**Nova Package PZM (PKG_PRAZO_MEDIO_PMZ)**:
- Versão mais recente das funções de prazo médio
- `FN_RECEBIMENTO_DETALHADO` - Para Pivot de Recebimento
- `FN_PAGAMENTO_DETALHADO` - Para Pivot de Pagamento
- `FN_RECEBIMENTO`, `FN_RECEBIMENTO_PESSOA`
- `FN_PAGAMENTO`, `FN_PAGAMENTO_PESSOA`

### 2.9. Agrícola (Prioridade P2)

Gestão de compras de frutas e safras.

**Submódulos**:

1. **Cadastro Safra/Meta** (`CadastroSafraMeta.aspx`)
   - Cadastro de safras e metas agrícolas
   - Entidades: `SAFRAEGF`, `METAPRODUCAO`

2. **Compras Frutas** (`ComprasFrutas.aspx`)
   - Compras de frutas (insumo agrícola)
   - Procedimento: `packageCompras.sp_recebimento_frutas`
   - Detalhamento: `sp_recebimento_frutas_det`
   - Detalhamento NF: `sp_recebimento_frutas_det_nf`
   - Parâmetros de drill-down: data, empresa, linha, UF, coluna clicada, grau, variedade

3. **Compras Frutas por Empresa** (`ComprasFrutasPorEmpresas.aspx`)
   - Segmentação por empresa

### 2.10. DBA / Administrativo (Prioridade P3)

Monitoramento e administração do banco de dados.

**Submódulos**:

1. **Tabelas** (`Tabela.aspx`)
   - Visualização de tabelas do sistema

2. **Sessão Usuário** (`SessaoUsuario.aspx`)
   - Monitor de sessões Oracle ativas
   - Procedures: `packageDba.sessoes_agrupadas`, `packageDba.sessoes`
   - Funcionalidade: Matar sessão (`packageDba.matar_sessao`)

3. **Usuário Lock** (`UsuarioLock.aspx`)
   - Desbloqueio de usuários
   - Procedure: `packageDba.bloqueio_usuario`

4. **Tabelas Bloqueadas**
   - Monitoramento de locks
   - Procedures: `packageDba.ses_tab_bloqueios`, `packageDba.sessao_tabela`

### 2.11. BI / Painel (Dashboard)

1. **Principal / Dashboard** (`Principal.aspx`)
   - Página inicial com indicadores e BI
   - Visão consolidada dos principais indicadores

2. **Carrega BI** (`CarregaBi.aspx`)
   - Carregamento de dados para o BI

### 2.12. Cadastros Financeiros (Prioridade P3)

1. **Agrupamento Contas** (`AgrupamentoContas.aspx`)
   - Agrupamento de contas contábeis

2. **Cenário DRE** (`CenarioDre.aspx`)
   - Cenários de DRE

3. **Calendário Financeiro** (`CalendarioFinanceiro.aspx`)
   - Gestão de dias úteis e feriados por estabelecimento
   - Tabela: `CALENDARIO_FINANCEIRO` (empresa, estabelecimento, data_efetiva, dia_util, usuario)
   - Lógica de cores: vermelho (todos estabelecimentos feriado), azul (todos dia útil), amarelo (misto)

4. **Cadastro Defasagem DRE** (`CadastroDefasagemDre.aspx`)
   - Configuração de defasagem para DRE

### 2.13. Cadastro de Saldo por Portador

Gestão de saldos iniciais de portadores:
- Listagem de portadores: `packageCadastroSaldo.sp_lista_Portadores`
- Saldo inicial período: `packageCadastroSaldo.sp_lista_saldo_inicial_periodo` (com opção de atualizar)
- Saldo portador dia: `packageCadastroSaldo.sp_lista_saldo_portador_dia`
- Gravar saldo: `packageCadastroSaldo.sp_salva_saldo_inicial`
- Atualizar período: `packageCadastroSaldo.sp_atualiza_saldo_periodo`
- Preparar digitação: `packageCadastroSaldo.sp_prepara_digitacao_saldo`
- Tabela: `SALDO_INI_POSICAO_FINAN_PORT`

### 2.14. Ajuste de Posição Financeira

Ferramenta administrativa para reclassificar lançamentos financeiros:
- **Ajustes de Documentos**: Tabela `AJUSTE_POSICAO_FINANCEIRA`
- **Ajustes de Movimentos**: Tabela `AJUSTE_POSICAO_FINANCEIRA_MOV`
- Consulta de lançamentos: `HDS.FIDOCUMENTO`, `HDS.FIDOCUMENTOMOV`
- Operações: inserir novo ajuste ou atualizar existente (UPSERT manual)
- Filtros: empresa, número doc, data vencimento, valor doc, data baixa, série NF (FIN, MUT, MJR, MIO)

### 2.15. Ajuste de Saldo Diário

- Tabela: `AJUSTE_SALDO_DIARIO`
- Operação: Inativar (`STATUS = 'I'`)

### 2.16. Ajuste de Agenda (Transferências)

- Tabela: `AJUSTE_AGENDA`
- Campos: tipo, data, banco, valor, entrada/saída, observação, usuário
- CRUD de transferências bancárias

### 2.17. Configuração de Agrupamentos para Relatório

- Tabela: `AGRUPA_RELATORIOS`
- Campos: `CD_EMPRESA`, `ID_AGRUPAMENTOS`
- Funcionalidade: salvar/recuperar configuração de agrupamentos por empresa

### 2.18. Ajuda Contextual

- Tabela: `AJUDA`
- Relaciona com: `ACESSO_CADASTRO_PAGINA` (pela CHAVE_CONTROLE)
- Campos: ID_AJUDA, ID_PAINEL, AJUDA (texto), ID_USUARIO_CADASTRO, DATA_CADASTRO, ID_USUARIO_ALTER, DATA_ALTER

---

## 3. Arquitetura do Sistema Legado

### 3.1. Diagrama de Camadas

```
┌──────────────────────────────────────────────────────────────┐
│                     IIS (Windows Server)                       │
│  ┌────────────────────────────────────────────────────────┐   │
│  │              Treis.Web (ASP.NET Web Forms)               │   │
│  │  ├── Default.aspx (Login)                               │   │
│  │  ├── Principal.aspx (Dashboard/BI)                      │   │
│  │  ├── interna/*.aspx (~30+ páginas de módulos)           │   │
│  │  ├── DevExpress v16.2 (ASPxGridView, ASPxPivotGrid)     │   │
│  │  ├── FormsAuthentication + HttpContext.Current.Session  │   │
│  │  └── Code-behind com lógica de negócio acoplada         │   │
│  └──────────────────┬─────────────────────────────────────┘   │
│                     │                                          │
│  ┌──────────────────▼─────────────────────────────────────┐   │
│  │              Treis.Data (Class Library)                  │   │
│  │  ├── DaoBase.cs (base com GetDadosProcedure, GetConsulta)│   │
│  │  ├── DaoAcesso.cs (autenticação, perfis, permissões)    │   │
│  │  ├── DaoCompras.cs (compras, comitê, NF, CFOP)         │   │
│  │  ├── DaoVendas.cs (vendas, análise, ranking)            │   │
│  │  ├── DaoPainel.cs (BI, financeiro, DRE, PZM, agrícola)  │   │
│  │  ├── DaoSessao.cs (monitoramento DBA)                   │   │
│  │  ├── Parametros.cs (wrapper de OracleParameter)         │   │
│  │  └── acesso/{Usuario.cs, Pagina.cs, Estabelecimento.cs}  │   │
│  └──────────────────┬─────────────────────────────────────┘   │
│                     │                                          │
│  ┌──────────────────▼─────────────────────────────────────┐   │
│  │              Treis.Util (Class Library)                  │   │
│  │  ├── Dados.cs (helpers de conversão de dados)           │   │
│  │  ├── Email.cs (envio de e-mails)                        │   │
│  │  ├── Servico.cs (integração com serviços externos)      │   │
│  │  └── StringUtils.cs (manipulação de strings)            │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                  Windows Service                              │
│  ┌────────────────────────────────────────────────────────┐   │
│  │              Treis.Service                              │   │
│  │  ├── Program.cs (entry point)                           │   │
│  │  ├── srvPrincipal.cs (tarefas agendadas)                │   │
│  │  └── System.Timers.Timer (agendamento de tarefas)       │   │
│  └────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                  Oracle Database (PRESERVADO)                  │
│  ├── Schemas: HDS, TREISBI, PROWISE_*, VETORH, SESUITE      │
│  ├── ~20+ Packages Oracle (PKG_COMPRAS, PKG_FINANCEIRO, ...) │
│  ├── ~100+ Stored Procedures                                 │
│  ├── Functions (FN_*) table-valued                           │
│  ├── Views (VW_*)                                            │
│  └── Tables (ACESSO_*, COMPRA_*, FINANCEIRO_*, AGRUPAMENTO_*)│
└──────────────────────────────────────────────────────────────┘
```

### 3.2. Padrão de Acesso a Dados (DaoBase)

O `DaoBase` fornece os métodos fundamentais usados por todos os DAOs:

1. **GetDadosProcedure** (com conexão reutilizável): Executa stored procedure Oracle com parâmetros de entrada e saída (REF CURSORs). Usado para operações que precisam compartilhar conexão (transações, múltiplas consultas).
2. **GetDadosProcedure** (com conexão própria): Versão que abre e fecha sua própria conexão.
3. **GetConsulta**: Executa query SQL pura (SELECT) retornando DataTable.
4. **ExecutaComando**: Executa comando DML (INSERT/UPDATE/DELETE).

**Mecanismo de Mapeamento de REF CURSOR**:
- Parâmetros de saída do tipo `OracleType.Cursor` são mapeados para `DataTable`s dentro de um `DataSet`
- O nome do parâmetro de saída define o nome da tabela dentro do DataSet
- Múltiplos cursores são carregados como múltiplas tabelas

### 3.3. Padrão de Conexão

- String de conexão obtida via `ConfigurationManager.ConnectionStrings["Oracle"]`
- Nome da package Oracle obtido via `ConfigurationManager.AppSettings["package"]`
- Conexões são abertas e fechadas a cada operação (não há pool gerenciado pela aplicação)
- `System.Data.OracleClient` - totalmente depreciado, sem suporte no .NET Core

---

## 4. Mapeamento Completo de Domínios de Negócio

### 4.1. Diagrama de Domínios

```
┌───────────────────────────────────────────────────────────────┐
│                     TREISTECNOVIN ERP                          │
├───────────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │  Acesso  │  │ Compras  │  │  Vendas  │  │Financeiro│     │
│  │  (P0)    │  │  (P1)    │  │  (P2)    │  │  (P1)    │     │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘     │
│       │             │             │             │             │
│  ┌────┴─────────────┴─────────────┴─────────────┴────┐       │
│  │              Oracle Database (HDS / TREISBI)       │       │
│  │  ┌──────────┐ ┌──────────┐ ┌────────────────────┐ │       │
│  │  │Cadastros │ │Transações│ │  Views Analíticas  │ │       │
│  │  │(ACESSO_*)│ │(COMPRA_*)│ │(VW_*, TREISBI.VW_*)│ │       │
│  │  └──────────┘ └──────────┘ └────────────────────┘ │       │
│  └───────────────────────────────────────────────────┘       │
│                                                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │   DRE    │  │ Prazo    │  │ Agrícola │  │   DBA    │     │
│  │  (P1)    │  │ Médio(P2)│  │  (P2)    │  │  (P3)    │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│                                                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │Planejamen│  │Cadastros │  │ Ajuste   │  │   BI     │     │
│  │  to (P1) │  │ Fin. (P3)│  │Financ.   │  │ Painel   │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
└───────────────────────────────────────────────────────────────┘
```

### 4.2. Relacionamentos Entre Domínios

- **Usuário → Perfil → Páginas**: Controle de acesso RBAC simples
- **Usuário → Estabelecimento**: Controle de acesso multi-empresa (via `ACESSO_USUARIO_EMPRESA_ESTAB`)
- **Empresa → Estabelecimento**: Hierarquia organizacional (via `VW_ESTABELECIMENTO_NEW`)
- **Compra → Produto → Comitê**: Gestão de suprimentos
- **Compra → Centro de Custo**: Alocação contábil
- **Financeiro → Agrupamento DRE**: Estrutura hierárquica do demonstrativo
- **Venda → Cliente → Produto**: Análise comercial
- **Compra Fruta → Safra**: Gestão agrícola

---

## 5. Catálogo de Artefatos - Camada de Dados (DAOs)

### 5.1. DaoBase

**Arquivo**: `Treis.Data/DaoBase.cs`
**Namespace**: `Treis.Data`
**Herança**: Nenhuma (classe base)

| Método | Assinatura | Descrição | Parâmetros de Entrada | Retorno |
|--------|-----------|-----------|----------------------|---------|
| `GetConnectionString` | `static string (string nome)` | Obtém connection string do Web.config | `nome`: chave da connection string | string de conexão |
| `GetPackage` | `static string (string nome)` | Obtém nome do package do AppSettings | `nome`: chave no AppSettings | nome do package |
| `GetDadosProcedure` | `DataSet (string package, string nomeProcedure, List<Parametros> parametros, ref OracleConnection connection)` | Executa SP com conexão reutilizável (para transações) | package, procedure, lista de parâmetros, conexão ref | DataSet com tables nomeadas pelos cursores |
| `GetDadosProcedure` | `DataSet (string package, string nomeProcedure, List<Parametros> parametros)` | Executa SP com conexão própria | package, procedure, lista de parâmetros | DataSet |
| `GetConsulta` | `DataTable (string query)` | Executa query SQL pura (SELECT) | query SQL | DataTable |
| `ExecutaComando` | `bool (string query)` | Executa comando DML (INSERT/UPDATE/DELETE) | query SQL | true/false (sempre true ou throw) |

**Mecanismo de parametrização (GetDadosProcedure)**:
1. Para cada `Parametros`:
   - Se `Output == true`: Adiciona como `ParameterDirection.Output`, acumula nome em `tablesPar`
   - Se `Output == false`: Adiciona como parâmetro de entrada com `.Value`
2. Executa `ExecuteReader()` e carrega no DataSet usando `Load(dataReader, LoadOption.OverwriteChanges, tablesPar.Split(','))`
3. Isso faz com que cada REF CURSOR de saída se torne uma tabela nomeada dentro do DataSet

### 5.2. Parametros (Modelo de Parâmetro Oracle)

**Arquivo**: `Treis.Data/Parametros.cs`
**Namespace**: `Treis.Data`

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Nome` | `string` | Nome do parâmetro (ex: "p_empresa") |
| `Valor` | `string` | Valor do parâmetro (sempre string, convertido) |
| `Tipo` | `OracleType` | Tipo Oracle (VarChar, DateTime, Cursor, Int32, Float) |
| `Output` | `bool` | Se true, é parâmetro de saída (REF CURSOR) |

### 5.3. DaoAcesso

**Arquivo**: `Treis.Data/DaoAcesso.cs`
**Namespace**: `Treis.Data`
**Herança**: `DaoBase`
**Responsabilidade**: Autenticação, autorização, gestão de usuários, perfis, páginas e estabelecimentos.

| Método | Descrição | SQL/Procedure | Tabelas Envolvidas |
|--------|-----------|---------------|-------------------|
| `GetUsuario(string idUsuario)` | Busca usuário + empresa por ID | SELECT com LEFT JOIN | `ACESSO_CADASTRO_USUARIO`, `USUARIO_EMPRESA` |
| `GetUsuario(string login, string senha)` | **AUTENTICAÇÃO**: Valida login/senha e retorna usuário se ativo | SELECT com filtro `ATIVO = 'S'` | `ACESSO_CADASTRO_USUARIO` |
| `SalvaAcessoUsuario(int idUsuario, DateTime dataAcesso)` | Registra último acesso e incrementa contador | UPDATE | `ACESSO_CADASTRO_USUARIO` |
| `GetPaginas(int IDPerfil)` | **MENU HIERÁRQUICO**: Busca páginas do perfil + páginas pai (subquery UNION) | SELECT com subquery UNION | `ACESSO_CADASTRO_PAGINA`, `ACESSO_PERFIL_PAGINA` |
| `GetPaginas()` | Lista todas as páginas ativas | SELECT simples | `ACESSO_CADASTRO_PAGINA` |
| `GetPaginaByChave(string chaveControle, int idPerfil)` | Busca página por chave e perfil (valida permissão) | SELECT com JOIN | `ACESSO_CADASTRO_PAGINA`, `ACESSO_PERFIL_PAGINA` |
| `GetPaginaByChave(string chaveControle)` | Busca página apenas por chave | SELECT simples | `ACESSO_CADASTRO_PAGINA` |
| `GetPaginasAcessadas(int IdUsuario)` | **TOP 10**: Páginas mais acessadas pelo usuário (ordenado por quantidade DESC, LIMIT 10) | SELECT com JOIN e ORDER BY | `ACESSO_CADASTRO_PAGINA`, `ACESSO_VISUALIZACAO_PAGINA` |
| `RegistraAcessoMenu(string ChaveControle, int idUsuario)` | **UPSERT**: Registra ou incrementa acesso a uma página | SELECT → INSERT ou UPDATE | `ACESSO_VISUALIZACAO_PAGINA` |
| `SalvarPerfilPagina(int idPagina, string idPerfil)` | Vincula página a perfil | INSERT | `ACESSO_PERFIL_PAGINA` |
| `DeletaVinculoPaginaPerfil(string idPerfil, int idPagina)` | Remove vínculo específico | DELETE | `ACESSO_PERFIL_PAGINA` |
| `DeletaVinculoPaginaPerfil(string idPerfil)` | Remove TODOS os vínculos de um perfil | DELETE | `ACESSO_PERFIL_PAGINA` |
| `AlterarSenha(int idUsuario, string novaSenha)` | Altera senha e marca `ATUALIZA_SENHA='N'` | UPDATE | `ACESSO_CADASTRO_USUARIO` |
| `GetNode(int idUsuario)` | Lista estabelecimentos da view (ignora idUsuario no código atual - BUG?) | SELECT | `VW_ESTABELECIMENTO_NEW` |
| `GetTree()` | Retorna todos os estabelecimentos | SELECT * | `VW_ESTABELECIMENTO_NEW` |
| `GetRegistro(...)` | Verifica se usuário tem acesso a um estabelecimento específico | SELECT | `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `RegistraEstabelecimento(...)` | Concede acesso a estabelecimento | INSERT | `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `ExcluiEstabelecimento(...)` | Remove acesso a estabelecimento | DELETE | `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `DeletaAcessoUsuarioEstabelecimento(...)` | Remove TODOS acessos a estabelecimentos de um usuário | DELETE | `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `IncluiAcessoEstabelecimento(...)` | Concede acesso a estabelecimento (parâmetros trocados: cdEstabelecimento no lugar de cdEmpresa) | INSERT | `ACESSO_USUARIO_EMPRESA_ESTAB` |
| `GetSenha(string idUsuario)` | Busca hash da senha do usuário | SELECT | `ACESSO_CADASTRO_USUARIO` |
| `GetOracleVersion()` | Obtém versão do Oracle | SELECT from v$version | `v$version` |
| `AcessoUsuEmpEstab(string idUsuario)` | Verifica se usuário tem vínculo com algum estabelecimento | SELECT | `ACESSO_USUARIO_EMPRESA_ESTAB` |

### 5.4. DaoCompras

**Arquivo**: `Treis.Data/DaoCompras.cs`
**Namespace**: `Treis.Data`
**Herança**: `DaoBase`
**Responsabilidade**: Gestão de compras, comitê de compras, previsão, controle por NF, CFOP, relatórios.

| Método | Descrição | Oracle Package.Procedure | Cursores de Saída |
|--------|-----------|--------------------------|-------------------|
| `GetValoresComiteNew` | Dados do comitê de compras (versão NF) | `packageCompraNf.sp_realizado` | p_resultado, p_total, p_grafico, p_grafico2 |
| `GetValoresComiteDetalhe` | Detalhe do comitê (conexão compartilhada) | `packageCompraNf.sp_ret_prev_real_it_s_comite` | p_resultado |
| `GetItemCompraSemComiteNew` | Itens de compra sem comitê | `packageCompraNf.sp_item_compra_sem_comite` | p_resultado |
| `GetValoresRealizadoItemNew` | Realizado por item específico | `packageCompraNf.sp_realizado_item` | p_resultado |
| `GetPrevistoRealizadoMesNew` | Previsto vs Realizado por mês (conexão compartilhada) | `packageCompraNf.sp_previsto_realizado_item` | p_resultado |
| `GetProgressaoPreco` | Evolução de preços | `packageCompraNf.sp_progressao_preco` | p_resultado |
| `GetProdutosPeriodo` | Produtos de um período de compra (query complexa com subqueries de segurança) | QUERY DIRETA | - |
| `GetProdutosComiteMeta` | Produtos do comitê com meta definida | QUERY DIRETA | - |
| `GetProdutosComiteSemMeta` | Produtos do comitê sem meta (ROWNUM como ID) | QUERY DIRETA | - |
| `GetProdutosComiteEstoqueMeta` | Consulta itens do comitê com estoque | `packageCompraNew.sp_consulta_itens` | p_resultado |
| `UpdateProdutosPrevisao` | Atualiza previsão de produto | UPDATE DIRETO | - |
| `InsertProdutosPrevisao` | Insere previsão de produto | INSERT DIRETO | - |
| `GetEmpresaEstabelecimento` | Lista estabelecimentos que usam comitê | QUERY DIRETA (tabela `UTILIZA_COMITE_ESTABELECIMENTO`) | - |
| `GetItensComite` | Detalha itens de um comitê específico | `packageCompraNf.sp_detalha_comite` | p_resultado |
| `AtualizaPrecosProdutos` | Registra preço de produto | INSERT DIRETO (`PRODUTO_PRECO`) | - |
| `GetCentrosContas` | Agrupamento por centro de custo/conta | `packageCompraNf.sp_agrup_conta_ccusto` | p_resultado |
| `GetCentrosContasDetalhe` | Detalhe centro/conta ou conta/centro | `packageCompraNf.sp_agrup_conta_ccusto_detalhe` | p_resultado |
| `GetCentrosContasDetalheProduto` | Detalhe por produto | `packageCompraNf.sp_agrup_conta_ccusto_det_prod` | p_resultado |
| `GetPrevRealProdutosSemComite` | Previsto/Realizado de produtos sem comitê | `packageCompraNf.sp_prev_real_item_sem_comite` | p_resultado |
| `GeraCargaPrevistoRealizado` | Gera carga de dados previsto/realizado | `packageCompraNf.sp_carga_prev_real_item` | - |
| `GetDetalhePrevistoRealizado` | Detalhamento pós-carga (conexão compartilhada) | `packageCompraNf.sp_ret_prev_real_it` | p_resultado |
| `GetNotaFiscal` | Busca notas fiscais por filtros | `packageCompraNf.sp_busca_nf` | p_resultado |
| `GetTotalPrevReal` | Total previsto vs realizado | `packageCompraNf.sp_retorna_total_prev_real` | p_resultado |
| `GetCfop` | Lista CFOPs de transferência com log | QUERY DIRETA com CTE | - |
| `GetCfopExcecao` | Lista CFOPs de exceção com log | QUERY DIRETA com CTE | - |
| `InsertCfopTransferencia` | Adiciona CFOP de transferência | INSERT DIRETO | - |
| `GravaLogTransferencia` | Registra log de alteração CFOP transferência | INSERT DIRETO | - |
| `DeletaCfopTransferencia` | Remove CFOP de transferência | DELETE DIRETO | - |
| `InsertCfoptExcecao` | Adiciona CFOP de exceção | INSERT DIRETO | - |
| `GravaLogExcecao` | Registra log de alteração CFOP exceção | INSERT DIRETO | - |
| `DeletaCfopExcecao` | Remove CFOP de exceção | DELETE DIRETO | - |
| `CarregaCfops` | Lista todos CFOPs disponíveis | QUERY DIRETA | - |
| `VerificaCompraPeriodo` | Verifica se período já existe | QUERY DIRETA | - |
| `CadastraCompraPeriodo` | Cria novo período de compra | INSERT DIRETO | - |
| `GetRelatorioCompra2` | Relatório expandido (com estabelecimento, valor NF custo) | `packageCompraNf.sp_relatorio_compra2` | p_resultado, p_dados_prod |
| `GetRelatorioCompra` | Relatório de compras | `packageCompraNf.sp_relatorio_compra` | p_resultado, p_dados_prod |
| `GetResumoComiteLinha` | Resumo por comitê e linha (legado) | `packageCompraNf.sp_relatorio_compra_com_lin` | p_resultado |
| `GetResumoComiteLinhaNew` | Resumo por comitê e linha (expandido) | `packageCompraNf.sp_relatorio_compra_com_lin` | p_resultado, p_dados_prod |
| `InsertRegistroRelatorio` | Log de geração de relatório | INSERT DIRETO (`REGISTRO_RELATORIOS`) | - |
| `AcessoUsuEmpEstab` | Verifica se usuário NÃO tem vínculo com estabelecimento (retorna TRUE se NÃO tem) | QUERY DIRETA | - |

### 5.5. DaoVendas

**Arquivo**: `Treis.Data/DaoVendas.cs`
**Namespace**: `Treis.Data`
**Herança**: `DaoBase`
**Responsabilidade**: Análise de vendas, ranking de clientes, plano de vendas.

| Método | Descrição | Oracle Package.Procedure |
|--------|-----------|--------------------------|
| `GetVendas` | Painel de vendas (mercado interno) | `packageVendas.sp_painel_vendas_mi` |
| `GetRankingClientes` | Ranking de clientes por receita | `packageBi.sp_receita_empr_cliente_comp` |
| `DetalhaClienteProduto` | Detalhamento por produto (função table-valued) | `PKG_BI.FN_RECEITA_EMPR_PRODUTO` |
| `DetalhaClienteNf` | Detalhamento por nota fiscal (função table-valued) | `PKG_BI.FN_RECEITA_EMPRESA_DOC` |
| `GetVendasResultado` | Plano de vendas vs resultado | `packageBi.sp_plano_vendas_resultado` |
| `GetAnaliseVendas` | Análise de resultado de vendas | `packageComercial.prc_result_vendas` |

### 5.6. DaoPainel

**Arquivo**: `Treis.Data/DaoPainel.cs` (1742 linhas - o mais extenso)
**Namespace**: `Treis.Data`
**Herança**: `DaoBase`
**Responsabilidade**: BI, Financeiro, DRE, Prazo Médio, Agrícola, Posição Financeira, Ajustes, Calendário, Projeções.

**Grupos de Métodos**:

**A - Compras de Frutas (Agrícola)**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetComprasFrutas` | `packageCompras.sp_recebimento_frutas` |
| `GetDetalhesComprasFrutas` | `packageCompras.sp_recebimento_frutas_det` |
| `GetDetalhesNotaFiscalComprasFrutas` | `packageCompras.sp_recebimento_frutas_det_nf` |

**B - Fluxo de Caixa**:
| Método | Oracle Package.Procedure | Cursores |
|--------|--------------------------|----------|
| `GetFluxoDeCaixa` | `packageFinanceiro.sp_fluxo_caixa` | r_resultado_master, r_resultado_detalhe |
| `GetTotaisFluxoDeCaixa` | `packageFinanceiro.sp_resumo_geral_flxcxa` | r_resultado |
| `GetFluxoDeCaixaAnalitico` | `packageFinanceiro.sp_fluxo_caixa_analitico` | r_resultado (remove colunas PORT, PORTADOR, CLIENTE, NOME_CLIENTE) |
| `GetFluxoContabil` | `packageFinanceiro.sp_fluxo_ccontabil_pagar` | r_resultado_master, r_resultado_detalhe |

**C - Prazo Médio Recebimento (2 packages: PKG_FINANCEIRO e PKG_PRAZO_MEDIO)**:
| Método | Oracle Function |
|--------|-----------------|
| `GetPrazoMedioMensalRecebimento` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_MENSAL_RECEBE` |
| `GetPrazoMedioPessoaRecebe` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_RECEBE` |
| `GetPrazoMedioRecebeMulti` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_RECEBE` (mesma, para multi-seleção) |
| `GetPrazoMedioMesAnoPessoaRecebe` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_RECEBIMENTO` |
| `GetPrazoMedioMesAnoRecebeMulti` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_RECEBIMENTO` (mesma, para multi-seleção) |
| `GetPrazoMedioRecebimentoEmpresa` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_EMPRESA_RECEBE` |
| `GetPzmRecebimentoMensal` | `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_MENSAL` |
| `GetPzmPessoaRecebe` | `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_PESSOA` |
| `GetPzmRecebimentoEmpresa` | `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_EMPRESA` |
| `GetPzmRecebeDetalhaPessoa` | `PKG_PRAZO_MEDIO.FN_RECEBIMENTO` |
| `GetPzmLinhaRecebe` | `PKG_PRAZO_MEDIO.FN_RECEBIMENTO_LINHA` |

**D - Prazo Médio Pagamento (2 packages: PKG_FINANCEIRO e PKG_PRAZO_MEDIO)**:
| Método | Oracle Function |
|--------|-----------------|
| `GetPrazoMedioMensalPagamento` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_MENSAL_PAGA` (UNION ALL de 7 tipos: G, I, O, U, L, M, S) |
| `GetPrazoMedioPessoaPaga` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_PAGA` |
| `GetPrazoMedioPagaMulti` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_PAGA` (multi-seleção) |
| `GetPrazoMedioPagamento` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PAGAMENTO` |
| `GetPrazoMedioPagamentoMulti` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_PAGAMENTO` (multi-seleção) |
| `GetPrazoMedioPagamentoEmpresa` | `PKG_FINANCEIRO.FN_PRAZO_MEDIO_EMPRESA_PAGA` |
| `GetPzmPagamentoMensal` | `PKG_PRAZO_MEDIO.FN_PAGAMENTO_MENSAL` |
| `GetPzmPessoaPaga` | `PKG_PRAZO_MEDIO.FN_PAGAMENTO_PESSOA` |
| `GetPzmPagaLinha` | `PKG_PRAZO_MEDIO.FN_PAGAMENTO_LINHA` |
| `GetPzmePagamento` | `PKG_PRAZO_MEDIO.FN_PAGAMENTO` |
| `GetPzmPagamentoEmpresa` | `PKG_PRAZO_MEDIO.FN_PAGAMENTO_EMPRESA` |

**E - Nova Package PZM (PKG_PRAZO_MEDIO_PMZ)**:
| Método | Oracle Function |
|--------|-----------------|
| `GetPzmPessoaRecebePmz` | `PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO_PESSOA` |
| `GetPzmPessoaPagaPmz` | `PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_PESSOA` |
| `GetPzmePagamentoPmz` | `PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO` |
| `GetPzmRecebeDetalhaPessoaPmz` | `PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO` |
| `GetPmzNewPagamento` | `PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO` |
| `GetPmzNewPagamentoG` | `PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO` (com colunas explicitamente nomeadas) |
| `GetPmzNewRecebimento` | `PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO` (NOTA: query com erro - faltou "SELECT") |
| `GetPmzNewRecebimentoG` | `PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO` (com colunas explicitamente nomeadas) |
| `GetPmzNewPagamentoPessoa` | `PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_PESSOA` |
| `GetRecebimentoPivot` | `PKG_PRAZO_MEDIO_PMZ.FN_RECEBIMENTO_DETALHADO` (WHERE TOTALIZADOR IS NULL) |
| `GetPagamentoPivot` | `PKG_PRAZO_MEDIO_PMZ.FN_PAGAMENTO_DETALHADO` (WHERE TOTALIZADOR IS NULL) |

**F - DRE e Projeções**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetFluxoDre` | `packageFinanceiro.sp_fluxo_dre` |
| `GetDreOut` | `packageFinanceiroOut.sp_fluxo_dre` |
| `GetDreHomolog` | `packageFinanceiroHomolog.sp_fluxo_dre` |
| `GetProjecaoFluxoDre` | `packagePlanejamentoDre.sp_projecao` |
| `GetFluxoDreDoc` | `packageFinanceiro.sp_documento` |
| `GetFluxoDreDocMut` | `packageFinanceiroHomolog.sp_documento` (com 8 parâmetros) |
| `GetFluxoDreDocHomolog` | `packageFinanceiroHomolog.sp_detalha_docto_mutuo` |
| `GetDetalhamentoConta` | `packageFinanceiro.sp_fluxo_dre_detalhamento` |
| `GetDetalhamentoContaPrev` | `packageFinanceiro.sp_fluxo_dre_detalha_prev` |
| `GetDetalhamentoContaOut` | `packageFinanceiroOut.sp_fluxo_dre_detalhamento` |
| `GetDetalhamentoContaPrevOut` | `packageFinanceiroOut.sp_fluxo_dre_detalha_prev` |
| `GetDetalhamentoContaHomolog` | `packageFinanceiroHomolog.sp_fluxo_dre_detalhamento` |
| `GetDetalhamentoContaPrevHomolog` | `packageFinanceiroHomolog.sp_fluxo_dre_detalha_prev_mut` |
| `GetPercentuaisProjecao` | `packagePlanejamentoDre.sp_percentual_agrupamento` |
| `RegistraParametros` | `packagePlanejamentoDre.sp_salva_percentual_periodo` |

**G - Posição Financeira (3 versões de package)**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetPosicaoFinanceira` (legado) | `packageFinanceiroLcto.sp_posicao` |
| `GetPosicaoFinanceiraAjuste` | `packageFinanceira.sp_posicao` |
| `GetPosicaoFinanceiraNew` | `packageFinanceiroNew.sp_posicao` |
| `GetPosicaoFinanceiraSReal` | `PKG_POSICAO_FINANCEIRA_SREAL.sp_posicao` |
| `GetPosicaoFinanceiraMensal` | `packageFinanceiroNew.sp_posicao_dia_util` |
| `GetResumoPosicao` | `packageFinanceiroNew.sp_resumo_posicao` (3 cursores: resultado, externo, aplicações) |
| `GetDiferenca` | `packageFinanceiroNew.sp_posicao_prev_real` |
| `GetDetalhePosicaoFinanceira` (legado) | `packageFinanceira.sp_posicao_detalhe` |
| `GetDetalhePosicaoFinanceiraNew` | `packageFinanceiroNew.sp_posicao_detalhe` |
| `GetDetalhePosicaoFinanceiraSreal` | `PKG_POSICAO_FINANCEIRA_SREAL.sp_posicao_detalhe` |
| `GetDetalheProjecao` | `packageFinanceiroNew.sp_posicao_detalhe` (alias) |
| `GetPrevRealDetalhe` | `packageFinanceiroNew.sp_posicao_prev_real_detalhe` |
| `GetPosicaoSemana` | `packageFinanceiroNew.sp_posicao_semanal` (com pós-processamento de remoção de colunas vazias) |

**H - Projeção Financeira**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetProjecaoFinanceiraMensal` | `packageProjecaoFinanceira.sp_projecao` |
| `GetProjecaoFinanceiraDetalhe` | `packageProjecaoFinanceira.sp_projecao_detalhe` |
| `GetOrigem` | QUERY DIRETA: `AGRUPAMENTO_DRE.ORIGEM_INFORMACAO_PROJECAO_FIN` |

**I - Portador (Posição por Banco)**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetRelatorioPortador` | `packageFinanceiroNewPor.sp_posicao_portador` |
| `GetPortadorDetalhe` | `packageFinanceiroNewPor.sp_posicao_portador_detalhe` |

**J - Cadastro de Saldo Portador**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetPortadores` | `packageCadastroSaldo.sp_lista_Portadores` |
| `GetSaldoInicialPeriodo` | `packageCadastroSaldo.sp_lista_saldo_inicial_periodo` |
| `GetSaldoPortadorDia` | `packageCadastroSaldo.sp_lista_saldo_portador_dia` |
| `SetSaldoInicial` | `packageCadastroSaldo.sp_salva_saldo_inicial` |
| `AtualizaSaldoPeriodo` | `packageCadastroSaldo.sp_atualiza_saldo_periodo` |
| `PreparaDigitacaoSaldo` | `packageCadastroSaldo.sp_prepara_digitacao_saldo` |
| `GetSaldoInicialPortadores` | QUERY DIRETA: `SALDO_INI_POSICAO_FINAN_PORT` |

**K - Compras (versão no DaoPainel - duplicada/legada)**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetValoresComite` (legado) | `packageCompra.sp_realizado` |
| `GetValoresComiteNF` | `packageCompraNf.sp_realizado` |
| `GetValoresComiteMes` | `packageCompra.sp_realizado` |
| `GetValoresComiteMesNf` | `packageCompraNf.sp_realizado` |
| `GetValoresComiteItensMes` | `packageCompra.sp_realizado_item` |
| `GetValoresComiteItensMesNf` | `packageCompraNf.sp_realizado_item` |
| `GetItemCompraSemComite` | `packageCompraNf.sp_item_compra_sem_comite` |
| `GetItemCompraSemComiteNew` | `packageCompraNew.sp_item_compra_sem_comite` |
| `GetProgressaoPreco` (DaoPainel) | `packageCompraNew.sp_progressao_preco` |
| `GetProgressaoPrecoNF` (DaoPainel) | `packageCompraNf.sp_progressao_preco` |
| `GetPrevistoRealizadoMes` | `packageCompra.sp_previsto_realizado_item` |
| `GetPrevistoRealizadoMesNew` | `packageCompraNew.sp_previsto_realizado_item` |
| `GetPrevistoRealizadoMesNf` | `packageCompraNf.sp_previsto_realizado_item` |
| `GetProdutosSemPrevisao` | `PKG_COMPRA_NEW.FN_ITEM_COMPRA` |

**L - Ajustes Financeiros**:
| Método | Descrição |
|--------|-----------|
| `GetAjuste` | Lista ajustes ativos com JOIN de usuário |
| `GetAjusteMovimento` | Lista ajustes de movimento ativos |
| `GetAjusteFeito` | Verifica se ajuste já existe (para UPSERT) |
| `GetAjusteMovimentoFeito` | Verifica se ajuste de movimento já existe |
| `AlteraRegistro` | UPSERT de ajuste de documento (verifica → INSERT ou UPDATE) |
| `AlteraRegistroMovimento` | UPSERT de ajuste de movimento |
| `AlteraStatus` | Inativa ajuste (STATUS = 'I') |
| `GetAjusteLancamentos` | Consulta lançamentos em `HDS.FIDOCUMENTO` |
| `GetAjusteLancamentosMov` | Consulta lançamentos em `HDS.FIDOCUMENTOMOV` (séries MUT, MJR, MIO) |

**M - Calendário Financeiro**:
| Método | Descrição |
|--------|-----------|
| `InsereDatasFinanceiro` | Insere data no calendário |
| `GetDatas` | Lista datas do calendário com JOINs |
| `UpDateCalendario` | Atualiza dia útil |
| `CorDodia` | Lógica de cor: red (todos feriado), blue (todos útil), yellow (misto) |
| `NumDays` | Conta dias úteis/feriados |
| `NumEstbalecimentos` | Conta estabelecimentos da empresa |
| `Feriados` | Lista feriados em período fixo (01/10/2020 a 31/01/2022 - HARDCODED!) |
| `feriado` | Verifica se data específica é feriado |

**N - Comercial**:
| Método | Oracle Package.Procedure |
|--------|--------------------------|
| `GetComercialMercadoInterno` | `packageVenda.sp_comercial_mi` (pós-processamento: remove colunas zeradas, remove última linha) |
| `GetComercialValoresMi` | `packageVenda.sp_comercial_vlr_mi` |
| `GetMargens` | `packageBiPeriodo.sp_receita_empresa_doc_it_rel` |

**O - Gestão de Agrupamentos DRE e Produtos**:
| Método | Descrição |
|--------|-----------|
| `ExcluiProduto` | Remove produto de período |
| `ExcluiTodosProduto` | Remove todos produtos de período |
| `InsereProdutos` | Insere produto em período com previsão zero |
| `UpdateDatasVigencia` | Atualiza vigência de item do comitê |
| `GetPeriodos` | Lista períodos de compra |
| `ReplicaPeriodos` | Replica produtos entre períodos (INSERT SELECT) |
| `UpDateCadastroComite` | UPSERT de agrupamento DRE + detalhes empresa |
| `InsertCadastroComite` | INSERT de agrupamento DRE + detalhes empresa |
| `DeleteAgrupamentos` | DELETE de agrupamento e detalhes (ordem: detalhes primeiro) |
| `InativaAjuste` | Inativa ajuste de saldo diário |
| `GetAgrupamentos` | Lista agrupamentos DRE ordenados |
| `AgrupamentosRelatorio` | Obtém IDs de agrupamentos para relatório |
| `RegistraAgrupamentos` | Salva configuração de agrupamentos |
| `GetRelatorioPosicao` | Relatório de posição financeira |
| `GravaSaldoInicial` | Dispara gravação de saldo inicial |
| `GetAjuda` | Busca ajuda contextual por CHAVE_CONTROLE |

### 5.7. DaoSessao

**Arquivo**: `Treis.Data/DaoSessao.cs`
**Namespace**: `Treis.Data`
**Herança**: Nenhuma (independente, NÃO herda DaoBase)
**Package Oracle**: `packageDba`

| Método | Descrição | Procedure |
|--------|-----------|-----------|
| `GetSessaoUsuario` | Lista sessões agrupadas | `packageDba.sessoes_agrupadas` |
| `GetSessaoUsuarioDetalhes` | Detalhes de sessão por owner e OS user | `packageDba.sessoes` |
| `GetSessaoTabela` | Lista tabelas com locks | `packageDba.ses_tab_bloqueios` |
| `GetSessaoTabelaDetalhes` | Detalhes de lock por tabela | `packageDba.sessao_tabela` |
| `MatarSessao` | Mata sessão Oracle (KILL SESSION) | `packageDba.matar_sessao` |
| `GetSessaoUsuarioLock` | Lista usuários bloqueados | `packageDba.bloqueio_usuario` |

---

## 6. Catálogo de Artefatos - Entidades de Domínio

### 6.1. Usuario
**Namespace**: `Treis.Data.acesso`
**Tabela**: `ACESSO_CADASTRO_USUARIO`

| Propriedade | Tipo | Coluna Oracle | Descrição |
|-------------|------|---------------|-----------|
| `IDUsuario` | `int` | `ID_USUARIO` | Chave primária |
| `Nome` | `string` | `NOME` | Nome completo |
| `Login` | `string` | `LOGIN` | Nome de usuário |
| `IDPerfil` | `int` | `ID_PERFIL` | FK para perfil |
| `QuantidadeAcesso` | `int` | `QUANTIDADE_ACESSO` | Contador de logins |
| `Atualiza_Senha` | `string` | `ATUALIZA_SENHA` | Flag 'S'/'N' para forçar troca |

**Colunas Oracle adicionais** (existentes na tabela mas não mapeadas na entidade):
- `SENHA` (armazenada, validada no login)
- `ATIVO` ('S'/'N')
- `DATA_HORA_ULTIMO_ACESSO`
- `ID_USUARIO_EMPRESA` (FK para USUARIO_EMPRESA)

### 6.2. Pagina
**Namespace**: `Treis.Data.Acesso`
**Tabela**: `ACESSO_CADASTRO_PAGINA`

| Propriedade | Tipo | Coluna Oracle | Descrição |
|-------------|------|---------------|-----------|
| `IDPagina` | `int` | `ID_PAGINA` | Chave primária |
| `Url` | `string` | `URL` | Caminho do aspx |
| `TituloAba` | `string` | `TITULO_ABA` | Título da aba/navegador |
| `ChaveControle` | `string` | `CHAVE_CONTROLE` | Identificador único para controle de acesso |
| `TituloMenu` | `string` | `TITULO_MENU` | Texto exibido no menu |
| `IDPaginaPai` | `int?` | `ID_PAGINA_PAI` | FK auto-referência (nullable = página raiz) |
| `Ordem` | `int` | `ORDEM` | Ordem de exibição no menu |
| `ToolTip` | `string` | `TOOLTIP` | Tooltip do item de menu |

**Coluna Oracle adicional**: `ATIVO` ('S'/'N') - páginas inativas não aparecem no menu

### 6.3. Estabelecimento
**Namespace**: `Treis.Data.acesso`
**Tabela/View**: `VW_ESTABELECIMENTO_NEW`

| Propriedade | Tipo | Coluna Oracle | Descrição |
|-------------|------|---------------|-----------|
| `IdAcessoUsuEmpEst` | `int` | (não mapeado diretamente) | ID do vínculo usuário-estabelecimento |
| `IdUsuario` | `int` | (não mapeado diretamente) | ID do usuário |
| `CdEmpresa` | `int` | `EMPRESA` | Código da empresa |
| `DsEmpresa` | `string` | Derivado de `DESCRITIVO` | Nome da empresa (extraído do descritivo) |
| `CdEstabelecimento` | `int` | `ESTABELECIMENTO` | Código do estabelecimento |
| `DsEstabelecimento` | `string` | Derivado de `DESCRITIVO` | Nome do estabelecimento |

**Lógica de parse do descritivo**: O campo `DESCRITIVO` da view contém algo como "NOME_EMPRESA - NOME_ESTABELECIMENTO". O código extrai:
- `DsEmpresa`: substring até o primeiro espaço
- `DsEstabelecimento`: `DsEmpresa + substring a partir do "-"`

### 6.4. Entidades de Negócio (não tipadas - DataTable/DataSet)

As seguintes entidades são manipuladas como `DataTable`/`DataSet` no código legado e precisam ser tipadas na modernização:

| Entidade Conceitual | DAO | Módulo |
|---------------------|-----|--------|
| `ResumoAnualCompras` | DaoCompras | Compras |
| `ComiteComprasNF` | DaoCompras, DaoPainel | Compras |
| `ProgressaoPrecoNF` | DaoCompras, DaoPainel | Compras |
| `PrevisaoCompra` | DaoCompras | Compras |
| `CentroCustoCompras` | DaoCompras | Compras |
| `FluxoCaixa` | DaoPainel | Financeiro |
| `FluxoCaixaAnalitico` | DaoPainel | Financeiro |
| `FluxoContabil` | DaoPainel | Financeiro |
| `DRE` | DaoPainel | Financeiro |
| `PosicaoFinanceira` | DaoPainel | Financeiro |
| `ProjecaoFinanceira` | DaoPainel | Financeiro |
| `AnaliseVendas` | DaoVendas | Vendas |
| `RankingClientes` | DaoVendas | Vendas |
| `PlanoVendasResultado` | DaoVendas | Vendas |
| `PrazoMedioRecebimento` | DaoPainel | Financeiro |
| `PrazoMedioPagamento` | DaoPainel | Financeiro |
| `ComprasFrutas` | DaoPainel | Agrícola |
| `SafraMeta` | DaoPainel | Agrícola |
| `PlanejamentoFinanceiro` | DaoPainel | Financeiro |
| `AgrupamentoDRE` | DaoPainel | Financeiro |
| `CalendarioFinanceiro` | DaoPainel | Financeiro |
| `AjustePosicaoFinanceira` | DaoPainel | Financeiro |
| `SaldoPortador` | DaoPainel | Financeiro |
| `CFOPTransferencia` | DaoCompras | Compras |
| `Ajuda` | DaoPainel | Geral |

---

## 7. Catálogo de Artefatos - Camada Web (ASP.NET Web Forms)

### 7.1. Estrutura de Páginas

| Página ASPX | Chave de Controle (aproximada) | Módulo | DAO Principal |
|-------------|-------------------------------|--------|---------------|
| `Default.aspx` | `default` | Login | DaoAcesso |
| `Principal.aspx` | `principal` | Dashboard/BI | DaoPainel (múltiplos) |
| `CarregaBi.aspx` | `carregaBi` | BI | - |
| `interna/CadastroPerfil.aspx` | `cadastroPerfil` | Acesso | DaoAcesso |
| `interna/CadastroUsuario.aspx` | `cadastroUsuario` | Acesso | DaoAcesso |
| `interna/AlteraSenha.aspx` | `alteraSenha` | Acesso | DaoAcesso |
| `interna/VerificaRelatorios.aspx` | `verificaRelatorios` | Acesso | DaoAcesso |
| `interna/CadastroComiteCompras.aspx` | `cadastroComiteCompras` | Compras | DaoCompras |
| `interna/CadastroPrevisaoCompra.aspx` | `cadastroPrevisaoCompra` | Compras | DaoCompras |
| `interna/ResumoAnualCompras.aspx` | `resumoAnualCompras` | Compras | DaoCompras |
| `interna/ComiteCompras_NF.aspx` | `comiteComprasNf` | Compras | DaoCompras |
| `interna/ControleComprasMes_NF.aspx` | `controleComprasMesNf` | Compras | DaoCompras |
| `interna/ProgressaoPrecoNF.aspx` | `progressaoPrecoNf` | Compras | DaoCompras |
| `interna/ComprasCentroCusto.aspx` | `comprasCentroCusto` | Compras | DaoCompras |
| `interna/PrevisaoCompraAlmoxarifado.aspx` | `previsaoCompraAlmoxarifado` | Compras | DaoCompras |
| `interna/PosicaoFinanceiraMes.aspx` | `posicaoFinanceiraMes` | Financeiro | DaoPainel |
| `interna/PosicaoSemanal.aspx` | `posicaoSemanal` | Financeiro | DaoPainel |
| `interna/PosicaoFinanceiraPortador.aspx` | `posicaoFinanceiraPortador` | Financeiro | DaoPainel |
| `interna/FluxoDre.aspx` | `fluxoDre` | Financeiro/DRE | DaoPainel |
| `interna/DRE_HOMOLOG.aspx` | `dreHomolog` | Financeiro/DRE | DaoPainel |
| `interna/DRE_OUT.aspx` | `dreOut` | Financeiro/DRE | DaoPainel |
| `interna/Planejamento.aspx` | `planejamento` | Financeiro | DaoPainel |
| `interna/ProjecaoFinanceira.aspx` | `projecaoFinanceira` | Financeiro | DaoPainel |
| `interna/ProjecaoFluxoDre.aspx` | `projecaoFluxoDre` | Financeiro | DaoPainel |
| `interna/ContasPagarReceber.aspx` | `contasPagarReceber` | Financeiro | DaoPainel |
| `interna/AnaliseVendas.aspx` | `analiseVendas` | Vendas | DaoVendas |
| `interna/AnaliseVendasPivot.aspx` | `analiseVendasPivot` | Vendas | DaoVendas |
| `interna/AnaliseCliente.aspx` | `analiseCliente` | Vendas | DaoVendas |
| `interna/PlanoVendasResultado.aspx` | `planoVendasResultado` | Vendas | DaoVendas |
| `interna/PivotPrazoMedio.aspx` | `pivotPrazoMedio` | Prazo Médio | DaoPainel |
| `interna/PzmRecebimento.aspx` | `pzmRecebimento` | Prazo Médio | DaoPainel |
| `interna/PzmPagamento.aspx` | `pzmPagamento` | Prazo Médio | DaoPainel |
| `interna/CadastroSafraMeta.aspx` | `cadastroSafraMeta` | Agrícola | DaoPainel |
| `interna/ComprasFrutas.aspx` | `comprasFrutas` | Agrícola | DaoPainel |
| `interna/ComprasFrutasPorEmpresas.aspx` | `comprasFrutasPorEmpresas` | Agrícola | DaoPainel |
| `interna/AgrupamentoContas.aspx` | `agrupamentoContas` | Cadastros Fin. | DaoPainel |
| `interna/CenarioDre.aspx` | `cenarioDre` | Cadastros Fin. | DaoPainel |
| `interna/CalendarioFinanceiro.aspx` | `calendarioFinanceiro` | Cadastros Fin. | DaoPainel |
| `interna/CadastroDefasagemDre.aspx` | `cadastroDefasagemDre` | Cadastros Fin. | DaoPainel |
| `interna/Tabela.aspx` | `tabela` | DBA | DaoSessao |
| `interna/SessaoUsuario.aspx` | `sessaoUsuario` | DBA | DaoSessao |
| `interna/UsuarioLock.aspx` | `usuarioLock` | DBA | DaoSessao |

### 7.2. Componentes DevExpress Utilizados

- **ASPxGridView**: Grades de dados principais em praticamente todas as páginas
- **ASPxPivotGrid**: Tabelas dinâmicas (Análise de Vendas Pivot, Pivot Prazo Médio)
- **ASPxChartControl / WebChartControl**: Gráficos (Barras, Pizza, Linhas) nos dashboards
- **ASPxComboBox**: Filtros suspensos (empresa, estabelecimento, período)
- **ASPxDateEdit**: Seletores de data
- **ASPxCallbackPanel**: Atualização parcial de painéis (AJAX)
- **ASPxMenu**: Menu lateral hierárquico
- **ASPxTreeView**: Árvore de estabelecimentos (seleção multi-empresa)
- **ASPxPopupControl**: Popups para cadastros e detalhamentos

### 7.3. Estado da Sessão (HttpContext.Current.Session)

O sistema usa `HttpContext.Current.Session` para armazenar:
- `Usuario` (objeto Usuario completo após login)
- `Estabelecimento` selecionado
- `Empresa` selecionada
- `Paginas` (lista de páginas permitidas)
- `Perfil` do usuário
- Tokens/filtros de navegação entre páginas

**Na migração para .NET Core**: Session state será substituído por JWT claims + armazenamento client-side (localStorage/sessionStorage) ou redux store.

---

## 8. Catálogo de Artefatos - Windows Service

### 8.1. srvPrincipal (Treis.Service)

**Arquivo**: `Treis.Service/srvPrincipal.cs`
**Tipo**: Windows Service (.NET Framework)
**Mecanismo de Agendamento**: `System.Timers.Timer`

O serviço executa tarefas agendadas em background. Baseado na estrutura do projeto e menções no código, as tarefas incluem:

1. **Processamento de NF** (`ProcessamentoNFJob`): Processamento batch de notas fiscais
2. **Envio de Email** (`EnvioEmailJob`): Envio de emails automáticos (relatórios, alertas)
3. **Carga de Dados BI**: Geração/atualização de dados para dashboards

### 8.2. Migração para Worker Service (.NET)

No .NET 8/9, o `System.Timers.Timer` será substituído por:
- `BackgroundService` com `PeriodicTimer`
- Ou Quartz.NET para agendamento mais complexo (cron expressions)

---

## 9. Catálogo de Artefatos - Utilitários

### 9.1. Treis.Util

**Namespace**: `Treis.Util`

| Classe | Arquivo | Descrição |
|--------|---------|-----------|
| `Dados` | `Dados.cs` | Helpers de conversão e manipulação de dados (DataSet/DataTable) |
| `Email` | `Email.cs` | Envio de e-mails (SMTP) |
| `Servico` | `Servico.cs` | Integração com serviços externos |
| `StringUtils` | `StringUtils.cs` | Utilitários de manipulação de string |

---

## 10. Mapeamento de Banco de Dados Oracle

### 10.1. Schemas

| Schema | Descrição |
|--------|-----------|
| `HDS` | ERP principal - tabelas de produção, fiscal, cadastros |
| `TREISBI` | Camada de BI - views analíticas e tabelas de fatos |
| `PROWISE_*` | Instâncias por unidade fabril |
| `VETORH` | Gestão de RH/Folha |
| `SESUITE` | Plataforma SoftExpert |

### 10.2. Tabelas Mapeadas (usadas pelo sistema)

**Acesso/Segurança**:
- `ACESSO_CADASTRO_USUARIO` - Usuários do sistema
- `ACESSO_CADASTRO_PAGINA` - Páginas/módulos (menu)
- `ACESSO_PERFIL_PAGINA` - Relacionamento perfil x páginas (N:N)
- `ACESSO_VISUALIZACAO_PAGINA` - Log de acessos a páginas
- `ACESSO_USUARIO_EMPRESA_ESTAB` - Vínculo usuário x estabelecimento
- `USUARIO_EMPRESA` - Vínculo usuário x empresa

**Compras**:
- `COMITE_COMPRA` - Comitês de compra
- `COMITE_COMPRA_ITEM` - Itens do comitê com vigência
- `COMPRA_PERIODO` - Períodos de compra
- `COMPRA_PERIODO_PRODMAT` - Previsão de produtos por período
- `PRODUTO_PRECO` - Histórico de preços
- `UTILIZA_COMITE_ESTABELECIMENTO` - Configuração de estabelecimentos que usam comitê
- `CFOP_TRANSFERENCIA` - CFOPs de transferência
- `CFOP_EXCECAO` - CFOPs de exceção
- `LOG_CFOP_TRANSFERENCIA` - Log de alterações CFOP transferência
- `LOG_CFOP_EXCECAO` - Log de alterações CFOP exceção
- `CFOP` - Catálogo de CFOPs

**Financeiro**:
- `AGRUPAMENTO_DRE` - Estrutura hierárquica do DRE
- `AGRUPAMENTO_DRE_CONTAS` - Contas vinculadas a agrupamentos
- `AGRUPAMENTO_DRE_EMPRESA_DET` - Detalhes por empresa (percentual, origem projeção)
- `AGRUPAMENTO_DRE_CONTAS_PREV` - Previsões por conta
- `POSICAO_FINANCEIRA_CONF` - Posição financeira configurada
- `DRE_SALDO_INICIAL` - Saldos iniciais DRE
- `CALENDARIO_FINANCEIRO` - Dias úteis/feriados por estabelecimento
- `AJUSTE_POSICAO_FINANCEIRA` - Ajustes de posição financeira (documentos)
- `AJUSTE_POSICAO_FINANCEIRA_MOV` - Ajustes de posição financeira (movimentos)
- `AJUSTE_SALDO_DIARIO` - Ajustes de saldo diário
- `AJUSTE_AGENDA` - Transferências/agenda bancária
- `AGRUPA_RELATORIOS` - Configuração de agrupamentos para relatório
- `SALDO_INI_POSICAO_FINAN_PORT` - Saldos iniciais por portador

**Geral**:
- `AJUDA` - Ajuda contextual
- `REGISTRO_RELATORIOS` - Log de geração de relatórios

**ERP (HDS - somente leitura)**:
- `HDS.PRODMAT` - Catálogo de produtos/materiais
- `HDS.FIDOCUMENTO` - Documentos financeiros
- `HDS.FIDOCUMENTOMOV` - Movimentos financeiros
- `HDS.CFOP` - Catálogo de CFOPs (ERP)
- `HDS.CLIFOR` - Clientes/Fornecedores

**Views (VW_*)**:
- `VW_ESTABELECIMENTO_NEW` - Estabelecimentos (usada em GetNode/GetTree)
- `VW_ESTABELECIMENTO_NEWC` - Estabelecimentos (usada no calendário)
- `VW_EMPRESA` - Empresas
- `VW_PORTADOR` - Portadores/bancos
- `VW_CLIENTES_FORNECEDOR` - Hub central de parceiros
- `VW_LINHA` - Linhas de produtos
- `VW_AGRUPAMENTOS_DRE` - Agrupamentos DRE (camada semântica)
- `VW_SALDOITEM` - Saldos de itens
- `VW_ESTOQUEBASICO` - Estoque básico

### 10.3. Packages Oracle (com procedures mapeadas)

| Package | Procedures/Functions Mapeadas | Módulo |
|---------|------------------------------|--------|
| `packageCompraNf` | `sp_realizado`, `sp_item_compra_sem_comite`, `sp_realizado_item`, `sp_previsto_realizado_item`, `sp_progressao_preco`, `sp_agrup_conta_ccusto`, `sp_agrup_conta_ccusto_detalhe`, `sp_agrup_conta_ccusto_det_prod`, `sp_prev_real_item_sem_comite`, `sp_carga_prev_real_item`, `sp_ret_prev_real_it`, `sp_ret_prev_real_it_s_comite`, `sp_busca_nf`, `sp_retorna_total_prev_real`, `sp_relatorio_compra`, `sp_relatorio_compra2`, `sp_relatorio_compra_com_lin`, `sp_detalha_comite` | Compras |
| `packageCompraNew` | `sp_item_compra_sem_comite`, `sp_consulta_itens`, `sp_previsto_realizado_item`, `sp_progressao_preco` | Compras |
| `packageCompra` | `sp_realizado`, `sp_realizado_item`, `sp_previsto_realizado_item` | Compras (legado) |
| `packageFinanceiro` | `sp_fluxo_caixa`, `sp_resumo_geral_flxcxa`, `sp_fluxo_caixa_analitico`, `sp_fluxo_ccontabil_pagar`, `sp_fluxo_dre`, `sp_documento`, `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev` | Financeiro |
| `packageFinanceiroNew` | `sp_posicao`, `sp_posicao_dia_util`, `sp_posicao_prev_real`, `sp_posicao_detalhe`, `sp_posicao_prev_real_detalhe`, `sp_posicao_semanal`, `sp_resumo_posicao`, `sp_relatorio_posicao_finan_2`, `sp_grava_saldo_inicial_periodo` | Financeiro |
| `packageFinanceiroNewPor` | `sp_posicao_portador`, `sp_posicao_portador_detalhe` | Financeiro |
| `packageFinanceira` | `sp_posicao`, `sp_posicao_detalhe` | Financeiro (legado) |
| `packageFinanceiroLcto` | `sp_posicao` | Financeiro (legado) |
| `PKG_POSICAO_FINANCEIRA_SREAL` | `sp_posicao`, `sp_posicao_detalhe` | Financeiro |
| `packageFinanceiroHomolog` | `sp_fluxo_dre`, `sp_documento`, `sp_detalha_docto_mutuo`, `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev_mut` | DRE |
| `packageFinanceiroOut` | `sp_fluxo_dre`, `sp_fluxo_dre_detalhamento`, `sp_fluxo_dre_detalha_prev` | DRE |
| `packagePlanejamentoDre` | `sp_projecao`, `sp_percentual_agrupamento`, `sp_salva_percentual_periodo` | Planejamento |
| `packageProjecaoFinanceira` | `sp_projecao`, `sp_projecao_detalhe` | Projeção |
| `packageCadastroSaldo` | `sp_lista_Portadores`, `sp_lista_saldo_inicial_periodo`, `sp_lista_saldo_portador_dia`, `sp_salva_saldo_inicial`, `sp_atualiza_saldo_periodo`, `sp_prepara_digitacao_saldo` | Saldo Portador |
| `packageVendas` | `sp_painel_vendas_mi` | Vendas |
| `packageVenda` | `sp_comercial_mi`, `sp_comercial_vlr_mi` | Vendas |
| `packageComercial` | `prc_result_vendas` | Vendas |
| `packageBi` | `sp_receita_empr_cliente_comp`, `sp_plano_vendas_resultado` | BI |
| `packageBiPeriodo` | `sp_receita_empresa_doc_it_rel` | BI |
| `packageCompras` | `sp_recebimento_frutas`, `sp_recebimento_frutas_det`, `sp_recebimento_frutas_det_nf` | Agrícola |
| `packageDba` | `sessoes_agrupadas`, `sessoes`, `ses_tab_bloqueios`, `sessao_tabela`, `matar_sessao`, `bloqueio_usuario` | DBA |
| `PKG_BI` | `FN_RECEITA_EMPR_PRODUTO`, `FN_RECEITA_EMPRESA_DOC` | BI (funções) |
| `PKG_FINANCEIRO` | `FN_PRAZO_MEDIO_MENSAL_RECEBE`, `FN_PRAZO_MEDIO_PESSOA_RECEBE`, `FN_PRAZO_MEDIO_RECEBIMENTO`, `FN_PRAZO_MEDIO_EMPRESA_RECEBE`, `FN_PRAZO_MEDIO_MENSAL_PAGA`, `FN_PRAZO_MEDIO_PESSOA_PAGA`, `FN_PRAZO_MEDIO_PAGAMENTO`, `FN_PRAZO_MEDIO_EMPRESA_PAGA` | PZM (funções) |
| `PKG_PRAZO_MEDIO` | `FN_RECEBIMENTO_MENSAL`, `FN_RECEBIMENTO_PESSOA`, `FN_RECEBIMENTO`, `FN_RECEBIMENTO_EMPRESA`, `FN_RECEBIMENTO_LINHA`, `FN_PAGAMENTO_MENSAL`, `FN_PAGAMENTO_PESSOA`, `FN_PAGAMENTO_LINHA`, `FN_PAGAMENTO`, `FN_PAGAMENTO_EMPRESA` | PZM (funções) |
| `PKG_PRAZO_MEDIO_PMZ` | `FN_RECEBIMENTO`, `FN_RECEBIMENTO_PESSOA`, `FN_RECEBIMENTO_DETALHADO`, `FN_PAGAMENTO`, `FN_PAGAMENTO_PESSOA`, `FN_PAGAMENTO_DETALHADO` | PZM (nova versão) |
| `PKG_COMPRA_NEW` | `FN_ITEM_COMPRA` | Compras (função) |
| `PKG_BI` | `FN_RECEITA_EMPR_PRODUTO`, `FN_RECEITA_EMPRESA_DOC` | Vendas (funções) |

### 10.4. Estratégia de Acesso a Dados

O sistema usa duas estratégias de acesso:

1. **Stored Procedures com REF CURSORs** (maioria das consultas de relatório/BI):
   - Chamadas via `GetDadosProcedure`
   - Parâmetros de entrada tipados
   - REF CURSORs de saída mapeados para DataTables

2. **Queries SQL Diretas** (operações CRUD simples e consultas específicas):
   - Chamadas via `GetConsulta` (SELECT) ou `ExecutaComando` (DML)
   - Strings SQL concatenadas (⚠️ VULNERÁVEL A SQL INJECTION - prioridade de correção na migração)

### 10.5. Vulnerabilidades de Segurança Identificadas

1. **SQL Injection em TODAS as queries concatenadas**: Uso de `String.Format` e concatenação com `+` para montar queries com parâmetros vindos do frontend. Na migração, TODOS os parâmetros devem usar `DynamicParameters` do Dapper.
2. **Senha em texto puro na query**: A senha é concatenada na string SQL em `GetUsuario(login, senha)`. Migrar para hash + parâmetro.
3. **Erro de SQL em GetPmzNewRecebimento**: Query começa com `ID_REGISTRO` ao invés de `SELECT ROWNUM AS ID_REGISTRO` (bug).

---

## 11. Regras de Negócio e Lógicas Específicas

### 11.1. Autenticação e Sessão

1. **Login**: Valida `LOGIN`, `SENHA` e `ATIVO = 'S'` na tabela `ACESSO_CADASTRO_USUARIO`
2. **Registro de Acesso**: A cada login bem-sucedido, atualiza `DATA_HORA_ULTIMO_ACESSO = SYSDATE` e incrementa `QUANTIDADE_ACESSO`
3. **Forçar Troca de Senha**: Se `ATUALIZA_SENHA = 'S'`, o usuário deve trocar a senha antes de acessar o sistema
4. **Menu Hierárquico**: Montado a partir das páginas do perfil + páginas pai (subquery UNION)
5. **TOP 10 Páginas**: Exibe as 10 páginas mais acessadas pelo usuário (ordenado por `QUANTIDADE DESC`)

### 11.2. Controle de Acesso Multi-Empresa

1. Usuários são vinculados a estabelecimentos via `ACESSO_USUARIO_EMPRESA_ESTAB`
2. As queries de compras incluem subqueries de segurança verificando esse vínculo:
   ```sql
   AND EXISTS (SELECT 1 FROM ACESSO_USUARIO_EMPRESA_ESTAB A
   WHERE A.ID_USUARIO = '{usuario}'
   AND A.CD_EMPRESA = CP.CD_EMPRESA
   AND (A.CD_ESTABELECIMENTO IS NULL OR A.CD_ESTABELECIMENTO = NVL('{estab}', A.CD_ESTABELECIMENTO)))
   ```
3. Parâmetro `p_retorna_estabelecimento` controla se filtra ou não por estabelecimento
4. Parâmetro `p_mostra_estabelecimento` controla se a coluna de estabelecimento aparece no resultado

### 11.3. Gestão de Comitê de Compras

1. Itens do comitê têm vigência (`DT_VIGENCIA_INICIAL` e `DT_VIGENCIA_FINAL`)
2. A vigência é usada como filtro nas consultas de realizado vs previsto
3. Itens sem comitê são tratados separadamente (`sp_item_compra_sem_comite`)
4. Produtos podem ter meta ou não (`GetProdutosComiteMeta` vs `GetProdutosComiteSemMeta`)

### 11.4. Posição Financeira e DRE

1. **Múltiplas versões**: Existem 3 versões de packages para posição financeira (legado, new, sreal) e 3 para DRE (padrão, homologado, out)
2. **Agrupamentos hierárquicos**: `AGRUPAMENTO_DRE` tem estrutura de árvore (`ID_AGRUPAMENTO_PAI`)
3. **Dias úteis**: Cálculo de posição financeira considera calendário de dias úteis por estabelecimento (`p_considera_ajuste_dia`)
4. **Configurações confirmadas**: Parâmetro `p_considera_conf` inclui/exclui valores confirmados

### 11.5. Prazo Médio (PZM)

1. **7 tipos de operação no pagamento**: G (Geral), I, O, U, L, M, S - consolidados via UNION ALL
2. **3 níveis de drill-down**: Mensal → Pessoa (cliente/fornecedor) → Documentos
3. **3 versões de packages**: `PKG_FINANCEIRO` (legado), `PKG_PRAZO_MEDIO` (atual), `PKG_PRAZO_MEDIO_PMZ` (mais recente)
4. **Pivot**: Versões detalhadas filtram `WHERE TOTALIZADOR IS NULL` para remover linhas de totais

### 11.6. Calendário Financeiro

1. Cada data pode ter múltiplos registros (um por estabelecimento)
2. Lógica de cores:
   - **Vermelho**: Todos os estabelecimentos marcaram como feriado (`DIA_UTIL = 'N'`)
   - **Azul**: Todos os estabelecimentos marcaram como dia útil (`DIA_UTIL = 'S'`)
   - **Amarelo**: Situação mista (alguns útil, outros feriado)

### 11.7. Pós-Processamento de Dados

1. **Posição Semanal**: Remove colunas de semanas sem dados (primeira linha vazia)
2. **Comercial Mercado Interno**: Remove colunas de diferença que não são do mês atual, remove colunas decimais zeradas, remove última linha (total)
3. **Fluxo de Caixa Analítico**: Remove colunas PORT, PORTADOR, CLIENTE, NOME_CLIENTE do resultado

### 11.8. Registro de Atividades

1. **Acesso a Páginas**: UPSERT em `ACESSO_VISUALIZACAO_PAGINA` (se já existe, incrementa; senão, insere com quantidade=1)
2. **Geração de Relatórios**: INSERT em `REGISTRO_RELATORIOS` (usuário, página, data/hora, tipo relatório, IP)
3. **Alterações CFOP**: Log em tabelas específicas com usuário e data

---

## 12. Fluxos de Dados Críticos

### 12.1. Fluxo de Autenticação

```
[Browser] → POST Default.aspx (login, senha)
  → DaoAcesso.GetUsuario(login, senha) → Oracle: SELECT ... WHERE LOGIN='x' AND SENHA='y' AND ATIVO='S'
  → Se sucesso: DaoAcesso.SalvaAcessoUsuario() → UPDATE ... DATA_HORA_ULTIMO_ACESSO, QUANTIDADE_ACESSO++
  → DaoAcesso.GetPaginas(IDPerfil) → Oracle: SELECT com UNION (filhas + pais)
  → DaoAcesso.GetNode(idUsuario) → Oracle: SELECT from VW_ESTABELECIMENTO_NEW
  → Armazena na Session: Usuario, Paginas, Estabelecimentos
  → Redireciona para Principal.aspx
```

### 12.2. Fluxo de Dashboard Principal

```
[Principal.aspx] → Carrega múltiplos indicadores:
  → DaoPainel.GetPosicaoFinanceiraNew() → packageFinanceiroNew.sp_posicao
  → DaoPainel.GetValoresComiteNew() → packageCompraNf.sp_realizado (3 cursores)
  → DaoVendas.GetVendas() → packageVendas.sp_painel_vendas_mi
  → Exibe gráficos DevExpress e indicadores
```

### 12.3. Fluxo de Compras - Previsto vs Realizado

```
[Browser] → Seleciona filtros (empresa, mês/ano, estabelecimento, comitê, unidade, transferência)
  → DaoCompras.GetPrevistoRealizadoMesNew() → packageCompraNf.sp_previsto_realizado_item
  → DaoCompras.GeraCargaPrevistoRealizado() → packageCompraNf.sp_carga_prev_real_item
  → DaoCompras.GetDetalhePrevistoRealizado() → packageCompraNf.sp_ret_prev_real_it (conexão compartilhada)
  → DaoCompras.GetTotalPrevReal() → packageCompraNf.sp_retorna_total_prev_real
  → DaoCompras.GetNotaFiscal() → packageCompraNf.sp_busca_nf
  → Exibe grid com drill-down hierárquico
```

### 12.4. Fluxo de DRE com Drill-down

```
[Browser] → Seleciona empresa e data
  → DaoPainel.GetFluxoDre() → packageFinanceiro.sp_fluxo_dre (visão anual consolidada)
  → [Clique em conta] → DaoPainel.GetDetalhamentoConta() → packageFinanceiro.sp_fluxo_dre_detalhamento
  → [Clique em valor realizado] → DaoPainel.GetDetalhamentoContaPrev() → packageFinanceiro.sp_fluxo_dre_detalha_prev
  → [Clique em documento] → DaoPainel.GetFluxoDreDoc() → packageFinanceiro.sp_documento (2 cursores: doc + itens)
```

### 12.5. Fluxo de Prazo Médio - Recebimento

```
[Browser] → Seleciona filtros
  → Nível 1: DaoPainel.GetPrazoMedioMensalRecebimento() → PKG_FINANCEIRO.FN_PRAZO_MEDIO_MENSAL_RECEBE
  → [Clique em mês] → Nível 2: DaoPainel.GetPrazoMedioPessoaRecebe() → PKG_FINANCEIRO.FN_PRAZO_MEDIO_PESSOA_RECEBE
  → [Clique em pessoa] → Nível 3: DaoPainel.GetPrazoMedioMesAnoPessoaRecebe() → PKG_FINANCEIRO.FN_PRAZO_MEDIO_RECEBIMENTO
```

---

## 13. Guia de Migração para .NET 8/9

### 13.1. Estrutura de Projetos Alvo

```
TreisTecnovin.Modern.sln
├── src/
│   ├── Treis.Api/                    # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs      # Login, refresh token, alterar senha
│   │   │   ├── UsuarioController.cs   # CRUD usuários
│   │   │   ├── PerfilController.cs    # CRUD perfis + vinculação páginas
│   │   │   ├── PaginaController.cs    # Menu, páginas acessadas
│   │   │   ├── ComprasController.cs   # Resumo anual, comitê, previsão, progressão, CFOP
│   │   │   ├── VendasController.cs    # Análise vendas, ranking, plano
│   │   │   ├── FinanceiroController.cs # Fluxo caixa, posição, DRE, projeção
│   │   │   ├── PrazoMedioController.cs # PZM recebimento e pagamento
│   │   │   ├── AgricolaController.cs  # Compras frutas, safras
│   │   │   ├── CalendarioController.cs # Calendário financeiro
│   │   │   ├── AjusteController.cs    # Ajustes posição financeira
│   │   │   ├── PortadorController.cs  # Saldo portador
│   │   │   ├── AgrupamentoController.cs # Agrupamentos DRE
│   │   │   ├── DbaController.cs       # Sessões, locks (admin)
│   │   │   ├── EstabelecimentoController.cs # Árvore estab.
│   │   │   ├── RelatorioController.cs # Registro de relatórios
│   │   │   └── HealthController.cs    # Health checks
│   │   ├── Middlewares/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Filters/
│   │   │   ├── AuthorizationFilter.cs
│   │   │   └── ValidationFilter.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── Treis.Domain/                  # Class Library (zero dependências)
│   │   ├── Entities/                  # Todas as entidades tipadas
│   │   │   ├── Usuario.cs
│   │   │   ├── Perfil.cs
│   │   │   ├── Pagina.cs
│   │   │   ├── Estabelecimento.cs
│   │   │   ├── Compras/
│   │   │   │   ├── ResumoAnualCompras.cs
│   │   │   │   ├── ComiteComprasNF.cs
│   │   │   │   ├── ProgressaoPrecoNF.cs
│   │   │   │   ├── PrevisaoCompra.cs
│   │   │   │   └── ...
│   │   │   ├── Financeiro/
│   │   │   │   ├── FluxoCaixa.cs
│   │   │   │   ├── PosicaoFinanceira.cs
│   │   │   │   ├── Dre.cs
│   │   │   │   └── ...
│   │   │   └── ... (uma classe por entidade)
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   │   ├── IAcessoRepository.cs
│   │   │   │   ├── IComprasRepository.cs
│   │   │   │   ├── IVendasRepository.cs
│   │   │   │   ├── IFinanceiroRepository.cs
│   │   │   │   ├── IPrazoMedioRepository.cs
│   │   │   │   ├── IAgricolaRepository.cs
│   │   │   │   ├── ICalendarioRepository.cs
│   │   │   │   ├── IDbaRepository.cs
│   │   │   │   └── IAgrupamentoRepository.cs
│   │   │   └── Services/
│   │   │       ├── IAuthService.cs
│   │   │       ├── IComprasService.cs
│   │   │       ├── IVendasService.cs
│   │   │       └── IFinanceiroService.cs
│   │   ├── DTOs/
│   │   │   ├── Requests/              # Request DTOs
│   │   │   └── Responses/             # Response DTOs
│   │   └── Enums/
│   │       ├── StatusPedido.cs
│   │       ├── TipoOperacao.cs
│   │       └── ...
│   │
│   ├── Treis.Data/                    # Class Library
│   │   ├── Repositories/              # Implementações dos repositórios
│   │   │   ├── AcessoRepository.cs
│   │   │   ├── ComprasRepository.cs
│   │   │   ├── VendasRepository.cs
│   │   │   ├── FinanceiroRepository.cs
│   │   │   └── ...
│   │   ├── Oracle/
│   │   │   ├── OracleConnectionFactory.cs
│   │   │   └── OracleParameterHelper.cs
│   │   └── OracleProcedures.cs        # Constantes de procedures/package.função
│   │
│   └── Treis.Worker/                  # Worker Service
│       ├── Jobs/
│       │   ├── ProcessamentoNFJob.cs
│       │   └── EnvioEmailJob.cs
│       └── Program.cs
│
└── tests/
    ├── Treis.UnitTests/
    └── Treis.IntegrationTests/
```

### 13.2. Mapeamento de DAOs → Repositories

| DAO Legado | Repository .NET 8/9 | Entidades |
|------------|---------------------|-----------|
| `DaoAcesso` | `AcessoRepository` | Usuario, Perfil, Pagina, Estabelecimento |
| `DaoCompras` | `ComprasRepository` | ResumoAnualCompras, ComiteComprasNF, ProgressaoPrecoNF, PrevisaoCompra, CfopTransferencia |
| `DaoVendas` | `VendasRepository` | AnaliseVendas, RankingClientes, PlanoVendasResultado |
| `DaoPainel` | `FinanceiroRepository` + `PrazoMedioRepository` + `AgricolaRepository` + `CalendarioRepository` + `AgrupamentoRepository` | FluxoCaixa, PosicaoFinanceira, Dre, ProjecaoFinanceira, PrazoMedio, ComprasFrutas, CalendarioFinanceiro |
| `DaoSessao` | `DbaRepository` | SessaoOracle, TabelaLock, UsuarioLock |

### 13.3. Mapeamento de DataTable → Entidades Tipadas

Cada `DataTable`/`DataSet` retornado pelos DAOs deve ser convertido para classes C# tipadas:

```csharp
// Exemplo: GetValoresComiteNew retorna DataSet com 4 tabelas
// Moderno:
public class ComiteComprasResult
{
    public IEnumerable<ComiteComprasItem> Resultado { get; set; }
    public IEnumerable<ComiteComprasTotal> Totais { get; set; }
    public IEnumerable<ComiteComprasGrafico> Grafico { get; set; }
    public IEnumerable<ComiteComprasGrafico2> Grafico2 { get; set; }
}
```

### 13.4. Mapeamento de Endpoints REST

| Método | Rota | Controller | DAO Legado Equivalente |
|--------|------|------------|------------------------|
| `POST` | `/api/v1/auth/login` | AuthController | `DaoAcesso.GetUsuario(login, senha)` |
| `POST` | `/api/v1/auth/refresh` | AuthController | - (novo) |
| `POST` | `/api/v1/auth/alterar-senha` | AuthController | `DaoAcesso.AlterarSenha()` |
| `GET` | `/api/v1/usuarios` | UsuarioController | `DaoAcesso.GetUsuario(id)` |
| `POST` | `/api/v1/usuarios` | UsuarioController | (novo CRUD) |
| `PUT` | `/api/v1/usuarios/{id}` | UsuarioController | (novo CRUD) |
| `GET` | `/api/v1/perfis` | PerfilController | `DaoAcesso.GetPaginas(perfil)` |
| `GET` | `/api/v1/paginas/menu` | PaginaController | `DaoAcesso.GetPaginas(IDPerfil)` |
| `GET` | `/api/v1/paginas/recentes` | PaginaController | `DaoAcesso.GetPaginasAcessadas()` |
| `POST` | `/api/v1/paginas/acesso` | PaginaController | `DaoAcesso.RegistraAcessoMenu()` |
| `GET` | `/api/v1/estabelecimentos` | EstabelecimentoController | `DaoAcesso.GetNode()` / `GetTree()` |
| `POST` | `/api/v1/estabelecimentos/vinculo` | EstabelecimentoController | `DaoAcesso.RegistraEstabelecimento()` |
| `DELETE` | `/api/v1/estabelecimentos/vinculo` | EstabelecimentoController | `DaoAcesso.ExcluiEstabelecimento()` |
| `GET` | `/api/v1/compras/resumo-anual` | ComprasController | `DaoCompras.GetValoresComiteNew()` |
| `GET` | `/api/v1/compras/comite-nf` | ComprasController | `DaoPainel.GetValoresComiteNF()` |
| `GET` | `/api/v1/compras/previsto-realizado` | ComprasController | `DaoCompras.GetPrevistoRealizadoMesNew()` |
| `GET` | `/api/v1/compras/progressao-preco` | ComprasController | `DaoCompras.GetProgressaoPreco()` |
| `GET` | `/api/v1/compras/centro-custo` | ComprasController | `DaoCompras.GetCentrosContas()` |
| `GET` | `/api/v1/compras/centro-custo/{codigo}/detalhe` | ComprasController | `DaoCompras.GetCentrosContasDetalhe()` |
| `GET` | `/api/v1/compras/cfop` | ComprasController | `DaoCompras.GetCfop()` |
| `POST` | `/api/v1/compras/cfop/transferencia` | ComprasController | `DaoCompras.InsertCfopTransferencia()` |
| `DELETE` | `/api/v1/compras/cfop/transferencia/{id}` | ComprasController | `DaoCompras.DeletaCfopTransferencia()` |
| `GET` | `/api/v1/compras/previsao/produtos` | ComprasController | `DaoCompras.GetProdutosPeriodo()` |
| `GET` | `/api/v1/compras/relatorio` | ComprasController | `DaoCompras.GetRelatorioCompra2()` |
| `GET` | `/api/v1/vendas/analise` | VendasController | `DaoVendas.GetVendas()` |
| `GET` | `/api/v1/vendas/ranking-clientes` | VendasController | `DaoVendas.GetRankingClientes()` |
| `GET` | `/api/v1/vendas/cliente/{id}/produtos` | VendasController | `DaoVendas.DetalhaClienteProduto()` |
| `GET` | `/api/v1/vendas/cliente/{id}/nf` | VendasController | `DaoVendas.DetalhaClienteNf()` |
| `GET` | `/api/v1/vendas/plano-resultado` | VendasController | `DaoVendas.GetVendasResultado()` |
| `GET` | `/api/v1/financeiro/fluxo-caixa` | FinanceiroController | `DaoPainel.GetFluxoDeCaixa()` |
| `GET` | `/api/v1/financeiro/fluxo-caixa/analitico` | FinanceiroController | `DaoPainel.GetFluxoDeCaixaAnalitico()` |
| `GET` | `/api/v1/financeiro/fluxo-contabil` | FinanceiroController | `DaoPainel.GetFluxoContabil()` |
| `GET` | `/api/v1/financeiro/posicao` | FinanceiroController | `DaoPainel.GetPosicaoFinanceiraNew()` |
| `GET` | `/api/v1/financeiro/posicao/semanal` | FinanceiroController | `DaoPainel.GetPosicaoSemana()` |
| `GET` | `/api/v1/financeiro/posicao/portador` | FinanceiroController | `DaoPainel.GetRelatorioPortador()` |
| `GET` | `/api/v1/financeiro/posicao/resumo` | FinanceiroController | `DaoPainel.GetResumoPosicao()` |
| `GET` | `/api/v1/financeiro/posicao/diferenca` | FinanceiroController | `DaoPainel.GetDiferenca()` |
| `GET` | `/api/v1/financeiro/posicao/detalhe` | FinanceiroController | `DaoPainel.GetDetalhePosicaoFinanceiraNew()` |
| `GET` | `/api/v1/financeiro/dre` | FinanceiroController | `DaoPainel.GetFluxoDre()` |
| `GET` | `/api/v1/financeiro/dre/homologado` | FinanceiroController | `DaoPainel.GetDreHomolog()` |
| `GET` | `/api/v1/financeiro/dre/out` | FinanceiroController | `DaoPainel.GetDreOut()` |
| `GET` | `/api/v1/financeiro/dre/detalhe` | FinanceiroController | `DaoPainel.GetDetalhamentoConta()` |
| `GET` | `/api/v1/financeiro/dre/documento` | FinanceiroController | `DaoPainel.GetFluxoDreDoc()` |
| `GET` | `/api/v1/financeiro/projecao` | FinanceiroController | `DaoPainel.GetProjecaoFinanceiraMensal()` |
| `GET` | `/api/v1/prazo-medio/recebimento` | PrazoMedioController | `DaoPainel.GetPrazoMedioMensalRecebimento()` |
| `GET` | `/api/v1/prazo-medio/recebimento/pessoa` | PrazoMedioController | `DaoPainel.GetPrazoMedioPessoaRecebe()` |
| `GET` | `/api/v1/prazo-medio/recebimento/documentos` | PrazoMedioController | `DaoPainel.GetPrazoMedioMesAnoPessoaRecebe()` |
| `GET` | `/api/v1/prazo-medio/pagamento` | PrazoMedioController | `DaoPainel.GetPrazoMedioMensalPagamento()` |
| `GET` | `/api/v1/prazo-medio/pagamento/pessoa` | PrazoMedioController | `DaoPainel.GetPrazoMedioPessoaPaga()` |
| `GET` | `/api/v1/prazo-medio/pagamento/documentos` | PrazoMedioController | `DaoPainel.GetPrazoMedioPagamento()` |
| `GET` | `/api/v1/prazo-medio/pivot/recebimento` | PrazoMedioController | `DaoPainel.GetRecebimentoPivot()` |
| `GET` | `/api/v1/prazo-medio/pivot/pagamento` | PrazoMedioController | `DaoPainel.GetPagamentoPivot()` |
| `GET` | `/api/v1/agricola/compras-frutas` | AgricolaController | `DaoPainel.GetComprasFrutas()` |
| `GET` | `/api/v1/agricola/compras-frutas/detalhe` | AgricolaController | `DaoPainel.GetDetalhesComprasFrutas()` |
| `GET` | `/api/v1/agricola/compras-frutas/nf` | AgricolaController | `DaoPainel.GetDetalhesNotaFiscalComprasFrutas()` |
| `GET` | `/api/v1/calendario` | CalendarioController | `DaoPainel.GetDatas()` |
| `POST` | `/api/v1/calendario` | CalendarioController | `DaoPainel.InsereDatasFinanceiro()` |
| `PUT` | `/api/v1/calendario/{id}` | CalendarioController | `DaoPainel.UpDateCalendario()` |
| `GET` | `/api/v1/agrupamentos-dre` | AgrupamentoController | `DaoPainel.GetAgrupamentos()` |
| `POST` | `/api/v1/agrupamentos-dre` | AgrupamentoController | `DaoPainel.InsertCadastroComite()` |
| `PUT` | `/api/v1/agrupamentos-dre/{id}` | AgrupamentoController | `DaoPainel.UpDateCadastroComite()` |
| `DELETE` | `/api/v1/agrupamentos-dre/{id}` | AgrupamentoController | `DaoPainel.DeleteAgrupamentos()` |
| `GET` | `/api/v1/ajustes` | AjusteController | `DaoPainel.GetAjuste()` |
| `POST` | `/api/v1/ajustes/documento` | AjusteController | `DaoPainel.AlteraRegistro()` |
| `POST` | `/api/v1/ajustes/movimento` | AjusteController | `DaoPainel.AlteraRegistroMovimento()` |
| `PUT` | `/api/v1/ajustes/{id}/inativar` | AjusteController | `DaoPainel.AlteraStatus()` |
| `GET` | `/api/v1/ajustes/lancamentos` | AjusteController | `DaoPainel.GetAjusteLancamentos()` |
| `GET` | `/api/v1/portadores` | PortadorController | `DaoPainel.GetPortadores()` |
| `GET` | `/api/v1/portadores/saldo` | PortadorController | `DaoPainel.GetSaldoPortadorDia()` |
| `POST` | `/api/v1/portadores/saldo` | PortadorController | `DaoPainel.SetSaldoInicial()` |
| `GET` | `/api/v1/ajuda/{chavePagina}` | GeralController | `DaoPainel.GetAjuda()` |
| `GET` | `/api/v1/dba/sessoes` | DbaController | `DaoSessao.GetSessaoUsuario()` |
| `POST` | `/api/v1/dba/sessoes/{sid}/kill` | DbaController | `DaoSessao.MatarSessao()` |
| `GET` | `/api/v1/dba/locks` | DbaController | `DaoSessao.GetSessaoUsuarioLock()` |
| `GET` | `/api/health` | HealthController | - |
| `GET` | `/api/health/database` | HealthController | `DaoAcesso.GetOracleVersion()` |

### 13.5. Regras Críticas de Migração

1. **PRESERVAÇÃO TOTAL DO BANCO ORACLE** ⚠️
   - NENHUMA alteração em tabelas, views, procedures, packages, triggers, sequences
   - Apenas a camada de aplicação é modernizada
   - As mesmas procedures Oracle devem ser chamadas com os mesmos parâmetros

2. **Substituição System.Data.OracleClient → Oracle.ManagedDataAccess.Core**
   - Namespace: `Oracle.ManagedDataAccess.Client`
   - TODAS as queries com strings concatenadas devem ser migradas para parâmetros Dapper
   - REF CURSORs continuam sendo usados como parâmetros de saída

3. **Migração DataSet/DataTable → Entidades Tipadas**
   - Criar uma classe para cada estrutura de retorno
   - Mapear colunas Oracle (snake_case) para propriedades C# (PascalCase)
   - Dapper lida com a conversão automática de snake_case para PascalCase

4. **Autenticação JWT**
   - Substituir FormsAuthentication + Session por JWT Bearer Token
   - Claims: ID_USUARIO, NOME, LOGIN, ID_PERFIL, EMPRESA, ESTABELECIMENTO
   - Refresh token para renovação

5. **Tratamento de Conexão Compartilhada**
   - Métodos que usam `ref OracleConnection` precisam ser avaliados
   - Alternativa: usar `Unit of Work` ou transação explícita com `IDbTransaction`

6. **Pós-Processamento de Dados**
   - Lógicas de remoção de colunas e linhas devem ser movidas para a camada de serviço
   - Exemplo: remoção de semanas vazias, colunas zeradas, última linha de totais

---

## 14. Checklist de Verificação para IA

Ao recriar este sistema em .NET 8/9, verifique:

### 14.1. Cobertura de Funcionalidades
- [ ] Todos os 43+ endpoints REST mapeados
- [ ] Todas as 20+ packages Oracle referenciadas
- [ ] Todas as 30+ tabelas Oracle mapeadas
- [ ] Todos os 6 DAOs com seus métodos cobertos por repositories
- [ ] Todas as 3 entidades tipadas (Usuario, Pagina, Estabelecimento) + ~25 entidades de negócio criadas como classes

### 14.2. Regras de Negócio
- [ ] Autenticação com validação ATIVO='S' e ATUALIZA_SENHA
- [ ] Menu hierárquico com subquery UNION (páginas filhas + pais)
- [ ] TOP 10 páginas mais acessadas
- [ ] Controle de acesso multi-empresa com EXISTS nas queries
- [ ] 3 níveis de drill-down no Prazo Médio
- [ ] 3 versões de Posição Financeira (legado, new, sreal)
- [ ] 3 versões de DRE (padrão, homologado, out)
- [ ] 7 tipos de operação no PZM Pagamento (G, I, O, U, L, M, S)
- [ ] Calendário financeiro com lógica de cores (vermelho/azul/amarelo)
- [ ] UPSERT manual em acesso a páginas e ajustes financeiros
- [ ] Pós-processamento: remoção de colunas/semanas vazias, colunas zeradas
- [ ] Conexão compartilhada em fluxos de multi-etapa (carga → detalhe → total)

### 14.3. Segurança
- [ ] TODAS queries migradas de concatenação para parâmetros (eliminar SQL Injection)
- [ ] Senha NUNCA concatenada em query (usar hash + parâmetro)
- [ ] JWT com claims de perfil e estabelecimento
- [ ] Validação de permissão por CHAVE_CONTROLE em todos os endpoints
- [ ] Rate limiting nos endpoints de autenticação

### 14.4. Performance
- [ ] Connection pooling gerenciado pelo Oracle.ManagedDataAccess.Core
- [ ] Timeouts configuráveis por endpoint
- [ ] Logging de queries lentas (Serilog)
- [ ] Cache onde apropriado (dados de calendário, catálogos)

### 14.5. Observabilidade
- [ ] Health checks: /api/health, /api/health/database
- [ ] Logging estruturado (Serilog)
- [ ] Métricas de performance
- [ ] Tracing de requests

---

**Fim da Documentação**