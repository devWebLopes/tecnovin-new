using Dapper;
using Empresa.Data.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Empresa.Data.Repositories;

public class MetaCompraRepository : IMetaCompraRepository
{
    private readonly DbSession _session;
    public MetaCompraRepository(DbSession session) => _session = session;

    public async Task<IEnumerable<MetaCompra>> GetAllAsync()
    {
        await EnsureSeededAsync();

        const string sql = @"
            SELECT ID_META_COMPRA, CD_LINHA, SAFRA, DT_INICIAL, DT_FINAL, META_QTDE, CD_EMPRESA
              FROM META_COMPRAS
             ORDER BY SAFRA DESC, CD_EMPRESA, CD_LINHA";
        return await _session.Connection.QueryAsync<MetaCompra>(sql);
    }

    private async Task EnsureSeededAsync()
    {
        try
        {
            var count2026 = await _session.Connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM META_COMPRAS WHERE SAFRA = '2026'");
            if (count2026 > 0) return;

            var seeds = new[]
            {
                new { CdLinha = 73, Safra = "2026", DtInicial = new DateTime(2026, 3, 9), DtFinal = new DateTime(2026, 5, 29), MetaQtde = 4000000m, CdEmpresa = 2 },
                new { CdLinha = 71, Safra = "2026", DtInicial = new DateTime(2026, 1, 2), DtFinal = new DateTime(2026, 3, 31), MetaQtde = 99000000m, CdEmpresa = 2 },
                new { CdLinha = 73, Safra = "2026", DtInicial = new DateTime(2026, 1, 2), DtFinal = new DateTime(2026, 12, 31), MetaQtde = 89000000m, CdEmpresa = 200 },
                new { CdLinha = 23, Safra = "2026", DtInicial = new DateTime(2026, 5, 4), DtFinal = new DateTime(2026, 9, 25), MetaQtde = 1500000m, CdEmpresa = 300 },
                new { CdLinha = 72, Safra = "2026", DtInicial = new DateTime(2026, 5, 4), DtFinal = new DateTime(2026, 12, 18), MetaQtde = 45000000m, CdEmpresa = 2 },
                new { CdLinha = 72, Safra = "2025", DtInicial = new DateTime(2025, 4, 15), DtFinal = new DateTime(2025, 12, 31), MetaQtde = 42000000m, CdEmpresa = 2 },
                new { CdLinha = 71, Safra = "2025", DtInicial = new DateTime(2025, 1, 2), DtFinal = new DateTime(2025, 3, 28), MetaQtde = 94300000m, CdEmpresa = 2 },
                new { CdLinha = 73, Safra = "2025", DtInicial = new DateTime(2025, 3, 3), DtFinal = new DateTime(2025, 5, 23), MetaQtde = 10000000m, CdEmpresa = 2 },
                new { CdLinha = 23, Safra = "2025", DtInicial = new DateTime(2025, 4, 15), DtFinal = new DateTime(2025, 9, 10), MetaQtde = 1200000m, CdEmpresa = 300 },
                new { CdLinha = 73, Safra = "2025", DtInicial = new DateTime(2025, 1, 2), DtFinal = new DateTime(2025, 12, 31), MetaQtde = 100000000m, CdEmpresa = 2 }
            };

            const string insertSql = @"
                INSERT INTO META_COMPRAS (CD_LINHA, SAFRA, DT_INICIAL, DT_FINAL, META_QTDE, CD_EMPRESA)
                VALUES (:CdLinha, :Safra, :DtInicial, :DtFinal, :MetaQtde, :CdEmpresa)";

            foreach (var seed in seeds)
            {
                await _session.Connection.ExecuteAsync(insertSql, seed);
            }
        }
        catch
        {
            // Ignora se não for possível inserir por restrição de ambiente
        }
    }

    public async Task<MetaCompra?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT ID_META_COMPRA, CD_LINHA, SAFRA, DT_INICIAL, DT_FINAL, META_QTDE, CD_EMPRESA
              FROM META_COMPRAS
             WHERE ID_META_COMPRA = :IdMetaCompra";
        return await _session.Connection.QueryFirstOrDefaultAsync<MetaCompra>(sql, new { IdMetaCompra = id });
    }

    public async Task<long> InsertAsync(MetaCompra meta)
    {
        const string sql = @"
            INSERT INTO META_COMPRAS (CD_LINHA, SAFRA, DT_INICIAL, DT_FINAL, META_QTDE, CD_EMPRESA)
            VALUES (:CdLinha, :Safra, :DtInicial, :DtFinal, :MetaQtde, :CdEmpresa)
            RETURNING ID_META_COMPRA INTO :IdMetaCompra";

        var p = new DynamicParameters(meta);
        p.Add(":IdMetaCompra", dbType: DbType.Int64, direction: ParameterDirection.Output);

        await _session.Connection.ExecuteAsync(sql, p);

        return p.Get<long>(":IdMetaCompra");
    }

    public async Task<bool> UpdateAsync(MetaCompra meta)
    {
        const string sql = @"
            UPDATE META_COMPRAS
               SET CD_LINHA = :CdLinha, SAFRA = :Safra, DT_INICIAL = :DtInicial,
                   DT_FINAL = :DtFinal, META_QTDE = :MetaQtde, CD_EMPRESA = :CdEmpresa
             WHERE ID_META_COMPRA = :IdMetaCompra";

        var rows = await _session.Connection.ExecuteAsync(sql, meta);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM META_COMPRAS WHERE ID_META_COMPRA = :IdMetaCompra";
        var rows = await _session.Connection.ExecuteAsync(sql, new { IdMetaCompra = id });
        return rows > 0;
    }
}
