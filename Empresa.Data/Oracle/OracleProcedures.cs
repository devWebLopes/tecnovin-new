namespace Empresa.Data.Oracle;

/// <summary>
/// Constantes com os nomes reais das packages Oracle do sistema legado TreisTecnovin
/// Fonte: docs/web.config (appSettings do legado)
/// </summary>
public static class OracleProcedures
{
    // ─── Packages de Acesso ──────────────────────────────
    public const string PackageAcesso = "ACESSO_";

    // ─── Packages de Compras ─────────────────────────────
    public const string PackageCompraNf = "PKG_COMPRA_NF";
    public const string PackageCompra = "pkg_compra";
    public const string PackageCompraNew = "pkg_compra_new";
    public const string PackageComprasBi = "pkg_bi_compras";  // agrícola

    // ─── Packages Financeiro ─────────────────────────────
    public const string PackageFinanceiro = "pkg_financeiro";
    public const string PackageFinanceiroNew = "pkg_posicao_financeira_new";
    public const string PackageFinanceiroHomolog = "pkg_financeiro_homolog";
    public const string PackageFinanceiroOut = "pkg_financeiro_mut";
    public const string PackageFinanceiroLcto = "pkg_financeiro_lcto";
    public const string PackageFinanceira = "pkg_posicao_financeira";
    public const string PackageFinanceiroNewPor = "pkg_posicao_financeira_new_por";
    public const string PKG_POSICAO_FINANCEIRA_SREAL = "PKG_POSICAO_FINANCEIRA_SREAL";
    public const string PackageFinanceiroDev = "pkg_financeiro_dev";
    public const string PackageFinanceiroTeste = "pkg_financeiro_Z9";

    // ─── Packages DRE / Projeção ─────────────────────────
    public const string PackagePlanejamentoDre = "pkg_planejamento_dre";
    public const string PackageProjecaoFinanceira = "pkg_projecao_financeira";

    // ─── Packages Vendas / BI ────────────────────────────
    public const string PackageVendas = "pkg_vendas$";
    public const string PackageVenda = "pkg_venda";
    public const string PackageBi = "pkg_bi";
    public const string PackageComercial = "pkg_comercial";
    public const string PackageBiPeriodo = "PKG_BI_PERIODO";

    // ─── Packages Prazo Médio ────────────────────────────
    public const string PackagePrazoMedio = "pkg_prazo_medio";
    public const string PackagePrazoMedioPmz = "PKG_PRAZO_MEDIO_PMZ";
    public const string PKG_FINANCEIRO = "PKG_FINANCEIRO";

    // ─── Outros ─────────────────────────────────────────
    public const string PackageCadastroSaldo = "pkg_cadastro_saldo";
    public const string PackageDba = "dbms_treis";
    public const string PackagePainel = "pkg_painel";

    // ─── Procedures específicas ─────────────────────────
    public static class Procedures
    {
        // Compras
        public const string SpRealizado = "sp_realizado";
        public const string SpPrevistoRealizadoItem = "sp_previsto_realizado_item";
        public const string SpProgressaoPreco = "sp_progressao_preco";
        public const string SpAgrupContaCcusto = "sp_agrup_conta_ccusto";
        public const string SpAgrupContaCcustoDetalhe = "sp_agrup_conta_ccusto_detalhe";
        public const string SpAgrupContaCcustoDetProd = "sp_agrup_conta_ccusto_det_prod";

        // Financeiro
        public const string SpPosicao = "sp_posicao";
        public const string SpPosicaoSemanal = "sp_posicao_semanal";
        public const string SpResumoPosicao = "sp_resumo_posicao";
        public const string SpFluxoCaixa = "sp_fluxo_caixa";
        public const string SpFluxoCaixaAnalitico = "sp_fluxo_caixa_analitico";
        public const string SpResumoGeralFluxoCaixa = "sp_resumo_geral_flxcxa";
        public const string SpFluxoDre = "sp_fluxo_dre";
        public const string SpDocumento = "sp_documento";

        // Vendas
        public const string SpPainelVendasMi = "sp_painel_vendas_mi";
        public const string PrcResultVendas = "prc_result_vendas";
        public const string SpComercialMi = "sp_comercial_mi";
        public const string SpReceitaEmprClienteComp = "sp_receita_empr_cliente_comp";
        public const string SpPlanoVendasResultado = "sp_plano_vendas_resultado";

        // Agrícola
        public const string SpRecebimentoFrutas = "sp_recebimento_frutas";
        public const string SpRecebimentoFrutasDet = "sp_recebimento_frutas_det";
        public const string SpRecebimentoFrutasDetNf = "sp_recebimento_frutas_det_nf";

        // DBA
        public const string SessoesAgrupadas = "sessoes_agrupadas";

        // Cadastro Saldo
        public const string SpListaPortadores = "sp_lista_Portadores";
        public const string SpSalvaSaldoInicial = "sp_salva_saldo_inicial";
    }

    // ─── Funções table-valued ────────────────────────────
    public static class Functions
    {
        // PZM
        public const string FnRecebimentoMensal = "FN_RECEBIMENTO_MENSAL";
        public const string FnRecebimentoPessoa = "FN_RECEBIMENTO_PESSOA";
        public const string FnRecebimento = "FN_RECEBIMENTO";
        public const string FnPagamentoMensal = "FN_PAGAMENTO_MENSAL";
        public const string FnPagamentoPessoa = "FN_PAGAMENTO_PESSOA";
        public const string FnPagamento = "FN_PAGAMENTO";

        // BI
        public const string FnReceitaEmprProduto = "FN_RECEITA_EMPR_PRODUTO";
        public const string FnReceitaEmpresaDoc = "FN_RECEITA_EMPRESA_DOC";
    }
}