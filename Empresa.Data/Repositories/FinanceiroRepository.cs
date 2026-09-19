using Dapper;
using Empresa.Data.Models;
using System.Data;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório financeiro — Posição, Fluxo de Caixa, DRE, Projeção, Ajustes, Portador
/// </summary>
public class FinanceiroRepository : IFinanceiroRepository
{
    private readonly DbSession _session;

    public FinanceiroRepository(DbSession session)
    {
        _session = session;
    }

    // ─── Posição Financeira (3 versões) ────────────────

    private static string GetPosicaoPackage(string versao) => versao switch
    {
        "legado" => "packageFinanceira",
        "sreal" => "PKG_POSICAO_FINANCEIRA_SREAL",
        "new" => "packageFinanceiroNew",
        _ => "packageFinanceiroNew"
    };

    public async Task<IEnumerable<PosicaoFinanceira>> GetPosicaoFinanceiraAsync(
        int empresa, string versao, DateTime? dataInicial, DateTime? dataFinal)
    {
        var package = GetPosicaoPackage(versao);
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_data_inicial", dataInicial ?? DateTime.Now.AddMonths(-1), DbType.Date, ParameterDirection.Input);
        parameters.Add("p_data_final", dataFinal ?? DateTime.Now, DbType.Date, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<PosicaoFinanceira>(
            $"{package}.sp_posicao", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<PosicaoFinanceiraResumo>> GetResumoPosicaoAsync(int empresa)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);
        parameters.Add("p_externo", dbType: DbType.Object, direction: ParameterDirection.Output);
        parameters.Add("p_aplicacoes", dbType: DbType.Object, direction: ParameterDirection.Output);

        using var multi = await _session.Connection.QueryMultipleAsync(
            "packageFinanceiroNew.sp_resumo_posicao", parameters,
            commandType: CommandType.StoredProcedure);

        return await multi.ReadAsync<PosicaoFinanceiraResumo>();
    }

    public async Task<IEnumerable<PosicaoSemanal>> GetPosicaoSemanalAsync(int empresa)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<PosicaoSemanal>(
            "packageFinanceiroNew.sp_posicao_semanal", parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── Fluxo de Caixa ────────────────────────────────

    public async Task<IEnumerable<FluxoCaixaMaster>> GetFluxoCaixaAsync(
        int empresa, char tipo, DateTime dataInicial, DateTime dataFinal)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_tipo", tipo.ToString(), DbType.String, ParameterDirection.Input);
        parameters.Add("p_data_inicial", dataInicial, DbType.Date, ParameterDirection.Input);
        parameters.Add("p_data_final", dataFinal, DbType.Date, ParameterDirection.Input);
        parameters.Add("r_resultado_master", dbType: DbType.Object, direction: ParameterDirection.Output);
        parameters.Add("r_resultado_detalhe", dbType: DbType.Object, direction: ParameterDirection.Output);

        using var multi = await _session.Connection.QueryMultipleAsync(
            "packageFinanceiro.sp_fluxo_caixa", parameters,
            commandType: CommandType.StoredProcedure);

        return await multi.ReadAsync<FluxoCaixaMaster>();
    }

    public async Task<IEnumerable<FluxoCaixaDetalhe>> GetFluxoCaixaAnaliticoAsync(
        int empresa, char tipo, DateTime dataInicial, DateTime dataFinal)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_tipo", tipo.ToString(), DbType.String, ParameterDirection.Input);
        parameters.Add("p_data_inicial", dataInicial, DbType.Date, ParameterDirection.Input);
        parameters.Add("p_data_final", dataFinal, DbType.Date, ParameterDirection.Input);
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<FluxoCaixaDetalhe>(
            "packageFinanceiro.sp_fluxo_caixa_analitico", parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<FluxoCaixaTotais?> GetTotaisFluxoCaixaAsync(int empresa)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryFirstOrDefaultAsync<FluxoCaixaTotais>(
            "packageFinanceiro.sp_resumo_geral_flxcxa", parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── DRE ──────────────────────────────────────────

    private static string GetDrePackage(string versao) => versao switch
    {
        "homologado" => "packageFinanceiroHomolog",
        "out" => "packageFinanceiroOut",
        _ => "packageFinanceiro"
    };

    public async Task<IEnumerable<Dre>> GetDreAsync(int empresa, int ano, string versao)
    {
        var package = GetDrePackage(versao);
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_ano", ano, DbType.Int32, ParameterDirection.Input);
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<Dre>(
            $"{package}.sp_fluxo_dre", parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<DreDocumento>> GetDreDocumentosAsync(
        int empresa, string conta, string versao)
    {
        var package = GetDrePackage(versao);
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_conta", conta, DbType.String, ParameterDirection.Input);
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<DreDocumento>(
            $"{package}.sp_documento", parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── Projeção ─────────────────────────────────────

    public async Task<IEnumerable<ProjecaoFinanceira>> GetProjecaoAsync(int empresa, int ano)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_ano", ano, DbType.Int32, ParameterDirection.Input);
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<ProjecaoFinanceira>(
            "packageProjecaoFinanceira.sp_projecao", parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<PercentualAgrupamento>> GetPercentuaisProjecaoAsync(int empresa)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<PercentualAgrupamento>(
            "packagePlanejamentoDre.sp_percentual_agrupamento", parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── Ajustes ──────────────────────────────────────

    public async Task<IEnumerable<AjusteFinanceiro>> GetAjustesAsync(int empresa)
    {
        const string sql = @"
            SELECT ID_AJUSTE AS Id, DOCUMENTO AS Documento,
                   VALOR_ORIGINAL AS ValorOriginal, VALOR_AJUSTADO AS ValorAjustado,
                   STATUS AS Status, USUARIO, DATA_AJUSTE AS DataAjuste
            FROM AJUSTE_FINANCEIRO
            WHERE STATUS = 'A'
            ORDER BY DATA_AJUSTE DESC";

        return await _session.Connection.QueryAsync<AjusteFinanceiro>(sql);
    }

    public async Task<int> CreateAjusteAsync(AjusteFinanceiro ajuste)
    {
        const string sql = @"
            INSERT INTO AJUSTE_FINANCEIRO (DOCUMENTO, VALOR_ORIGINAL, VALOR_AJUSTADO, STATUS, USUARIO)
            VALUES (:Documento, :ValorOriginal, :ValorAjustado, 'A', :Usuario)
            RETURNING ID_AJUSTE INTO :Id";

        var parameters = new DynamicParameters(new
        {
            ajuste.Documento,
            ajuste.ValorOriginal,
            ajuste.ValorAjustado,
            ajuste.Usuario
        });
        parameters.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> InativarAjusteAsync(int id)
    {
        const string sql = "UPDATE AJUSTE_FINANCEIRO SET STATUS = 'I' WHERE ID_AJUSTE = :Id";
        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    // ─── Portador ─────────────────────────────────────

    public async Task<IEnumerable<Portador>> GetPortadoresAsync()
    {
        var parameters = new DynamicParameters();
        parameters.Add("r_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<Portador>(
            "packageCadastroSaldo.sp_lista_Portadores", parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> SetSaldoInicialAsync(int idPortador, decimal saldo)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id_portador", idPortador, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_saldo", saldo, DbType.Decimal, ParameterDirection.Input);
        parameters.Add("p_usuario", dbType: DbType.String, direction: ParameterDirection.Input);

        await _session.Connection.ExecuteAsync(
            "packageCadastroSaldo.sp_salva_saldo_inicial", parameters,
            commandType: CommandType.StoredProcedure);
        return true;
    }
}