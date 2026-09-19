using Dapper;
using Empresa.Data.Models;
using System.Data;

namespace Empresa.Data.Repositories;

public class VendasRepository : IVendasRepository
{
    private readonly DbSession _session;
    public VendasRepository(DbSession session) => _session = session;

    public async Task<IEnumerable<AnaliseVendas>> GetAnaliseVendasAsync(int empresa, int? estabelecimento)
    {
        var p = new DynamicParameters();
        p.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        p.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);
        return await _session.Connection.QueryAsync<AnaliseVendas>(
            "packageComercial.prc_result_vendas", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RankingCliente>> GetRankingClientesAsync(int empresa, int ano)
    {
        var p = new DynamicParameters();
        p.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        p.Add("p_ano", ano, DbType.Int32, ParameterDirection.Input);
        p.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);
        return await _session.Connection.QueryAsync<RankingCliente>(
            "packageBi.sp_receita_empr_cliente_comp", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<PlanoVendasResultado>> GetPlanoVendasResultadoAsync(int empresa, int ano)
    {
        var p = new DynamicParameters();
        p.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        p.Add("p_ano", ano, DbType.Int32, ParameterDirection.Input);
        p.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);
        return await _session.Connection.QueryAsync<PlanoVendasResultado>(
            "packageBi.sp_plano_vendas_resultado", p, commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ComercialMI>> GetComercialMIAsync(int empresa, int ano)
    {
        var p = new DynamicParameters();
        p.Add("p_empresa", empresa, DbType.Int32, ParameterDirection.Input);
        p.Add("p_ano", ano, DbType.Int32, ParameterDirection.Input);
        p.Add("p_resultado", dbType: DbType.Object, direction: ParameterDirection.Output);
        return await _session.Connection.QueryAsync<ComercialMI>(
            "packageVenda.sp_comercial_mi", p, commandType: CommandType.StoredProcedure);
    }
}