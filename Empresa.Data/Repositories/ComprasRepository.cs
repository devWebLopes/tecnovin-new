using Dapper;
using Empresa.Data.Models;
using System.Data;

namespace Empresa.Data.Repositories;

/// <summary>
/// Repositório de compras — executa packages Oracle via Dapper com suporte a REF CURSORs
/// </summary>
public class ComprasRepository : IComprasRepository
{
    private readonly DbSession _session;

    public ComprasRepository(DbSession session)
    {
        _session = session;
    }

    private const string PackageCompraNf = "packageCompraNf";

    // ─── Resumo Anual ─────────────────────────────────────

    public async Task<IEnumerable<ResumoAnualCompras>> GetResumoAnualAsync(int empresa, int? estabelecimento)
    {
        var parameters = CreateSpRealizadoParameters(empresa, estabelecimento);

        using var multi = await _session.Connection.QueryMultipleAsync(
            $"{PackageCompraNf}.sp_realizado",
            parameters,
            commandType: CommandType.StoredProcedure);

        return await multi.ReadAsync<ResumoAnualCompras>();
    }

    public async Task<ResumoAnualComprasTotais?> GetResumoAnualTotaisAsync(int empresa, int? estabelecimento)
    {
        var parameters = CreateSpRealizadoParameters(empresa, estabelecimento);

        using var multi = await _session.Connection.QueryMultipleAsync(
            $"{PackageCompraNf}.sp_realizado",
            parameters,
            commandType: CommandType.StoredProcedure);

        await multi.ReadAsync<ResumoAnualCompras>();
        return await multi.ReadFirstOrDefaultAsync<ResumoAnualComprasTotais>();
    }

    public async Task<IEnumerable<ResumoAnualComprasGrafico>> GetResumoAnualGraficoAsync(int empresa, int? estabelecimento)
    {
        var parameters = CreateSpRealizadoParameters(empresa, estabelecimento);

        using var multi = await _session.Connection.QueryMultipleAsync(
            $"{PackageCompraNf}.sp_realizado",
            parameters,
            commandType: CommandType.StoredProcedure);

        await multi.ReadAsync<ResumoAnualCompras>();
        await multi.ReadAsync<ResumoAnualComprasTotais>();
        return await multi.ReadAsync<ResumoAnualComprasGrafico>();
    }

    private static DynamicParameters CreateSpRealizadoParameters(int empresa, int? estabelecimento)
    {
        var parameters = new DynamicParameters();

        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_data", DateTime.Now, DbType.Date, ParameterDirection.Input);
        parameters.Add("p_atual", "S", DbType.String, ParameterDirection.Input);
        parameters.Add("p_vigencia", "S", DbType.String, ParameterDirection.Input);
        parameters.Add("p_mostraEstabelecimento", estabelecimento.HasValue ? "S" : "N", DbType.String, ParameterDirection.Input);
        parameters.Add("p_estabelecimento", estabelecimento ?? 0, DbType.Int32, ParameterDirection.Input);

        // REF CURSORs de saída
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);
        parameters.Add("p_total", dbType: DbType.Object, direction: ParameterDirection.Output);
        parameters.Add("p_grafico", dbType: DbType.Object, direction: ParameterDirection.Output);
        parameters.Add("p_grafico2", dbType: DbType.Object, direction: ParameterDirection.Output);

        return parameters;
    }

    // ─── Comitê Compras NF ────────────────────────────────

    public async Task<IEnumerable<ComiteComprasNF>> GetComiteComprasNFAsync(int empresa, DateTime dataInicial, DateTime dataFinal)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_data_inicial", dataInicial, DbType.Date, ParameterDirection.Input);
        parameters.Add("p_data_final", dataFinal, DbType.Date, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<ComiteComprasNF>(
            $"{PackageCompraNf}.sp_comite_compras",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── Previsto vs Realizado ────────────────────────────

    public async Task<IEnumerable<PrevisaoCompra>> GetPrevistoRealizadoAsync(int empresa, int? estabelecimento)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_estabelecimento", estabelecimento ?? 0, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<PrevisaoCompra>(
            $"{PackageCompraNf}.sp_previsto_realizado_item",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── Progressão de Preço ──────────────────────────────

    public async Task<IEnumerable<ProgressaoPrecoNF>> GetProgressaoPrecoAsync(int empresa, string? produto)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_produto", produto ?? "", DbType.String, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<ProgressaoPrecoNF>(
            $"{PackageCompraNf}.sp_progressao_preco",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    // ─── CFOP ─────────────────────────────────────────────

    public async Task<IEnumerable<CfopTransferencia>> GetCfopTransferenciasAsync()
    {
        const string sql = @"
            SELECT c.ID_CFOP_TRANSFERENCIA AS Id, c.CFOP, c.DESCRICAO,
                   c.DATA_CADASTRO, c.USUARIO_CADASTRO, c.DATA_ALTERACAO, c.USUARIO_ALTERACAO
            FROM CFOP_TRANSFERENCIA c
            ORDER BY c.CFOP";

        return await _session.Connection.QueryAsync<CfopTransferencia>(sql);
    }

    public async Task<int> CreateCfopTransferenciaAsync(CfopTransferencia cfop)
    {
        const string sql = @"
            INSERT INTO CFOP_TRANSFERENCIA (CFOP, DESCRICAO, DATA_CADASTRO, USUARIO_CADASTRO)
            VALUES (:Cfop, :Descricao, SYSDATE, :UsuarioCadastro)
            RETURNING ID_CFOP_TRANSFERENCIA INTO :Id";

        var parameters = new DynamicParameters(new
        {
            cfop.Cfop,
            cfop.Descricao,
            cfop.UsuarioCadastro
        });
        parameters.Add(":Id", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        await _session.Connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":Id");
    }

    public async Task<bool> DeleteCfopTransferenciaAsync(int id)
    {
        const string sql = "DELETE FROM CFOP_TRANSFERENCIA WHERE ID_CFOP_TRANSFERENCIA = :Id";
        var rows = await _session.Connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    // ─── Centro de Custo ──────────────────────────────────

    public async Task<IEnumerable<CentroCusto>> GetCentroCustoAsync(int empresa, int? estabelecimento)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_estabelecimento", estabelecimento ?? 0, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<CentroCusto>(
            $"{PackageCompraNf}.sp_agrup_conta_ccusto",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheAsync(string conta, int? estabelecimento)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_conta", conta, DbType.String, ParameterDirection.Input);
        parameters.Add("p_estabelecimento", estabelecimento ?? 0, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<CentroCusto>(
            $"{PackageCompraNf}.sp_agrup_conta_ccusto_detalhe",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CentroCusto>> GetCentroCustoDetalheProdutoAsync(string conta, int? estabelecimento)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_conta", conta, DbType.String, ParameterDirection.Input);
        parameters.Add("p_estabelecimento", estabelecimento ?? 0, DbType.Int32, ParameterDirection.Input);
        parameters.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);

        return await _session.Connection.QueryAsync<CentroCusto>(
            $"{PackageCompraNf}.sp_agrup_conta_ccusto_det_prod",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}